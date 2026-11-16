using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using CSWebSockets.Structs;

namespace CSWebSockets
{
    // Transport only. Awaiting callers resume on their Unity synchronization
    // context; the receive loop never touches scene objects or the Unity logger.
    public sealed class WebSocketClient : IDisposable
    {
        private const int MaxResponseBytes = 4 * 1024 * 1024;
        private static readonly Encoding Utf8 = new UTF8Encoding(false, true);
        private readonly ClientWebSocket _socket = new ClientWebSocket();
        private readonly Uri _uri;
        private readonly CancellationTokenSource _stop = new CancellationTokenSource();
        private readonly CancellationToken _stopToken;
        private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);
        private readonly ConcurrentDictionary<string, TaskCompletionSource<PredictionResponse>> _pending =
            new ConcurrentDictionary<string, TaskCompletionSource<PredictionResponse>>();
        private readonly TaskCompletionSource<bool> _ready =
            new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        private Task _receiveTask;
        private volatile bool _isReady;
        private int _disposed;

        public WebSocketState ClientState => _socket.State;
        public bool IsReady => _isReady && _socket.State == WebSocketState.Open;
        public string LastError { get; private set; }

        public WebSocketClient(string uri, string bearerToken = null)
        {
            _uri = new Uri(uri);
            if (_uri.Scheme != "ws" && _uri.Scheme != "wss")
                throw new ArgumentException("Use a ws:// or wss:// prediction endpoint.");
            _stopToken = _stop.Token;
            _socket.Options.KeepAliveInterval = TimeSpan.FromSeconds(20);
            _socket.Options.AddSubProtocol("textslinger.v1");
            if (!string.IsNullOrEmpty(bearerToken))
                _socket.Options.SetRequestHeader("Authorization", "Bearer " + bearerToken);
        }

        public async Task ConnectAsync()
        {
            using (var timeout = CancellationTokenSource.CreateLinkedTokenSource(_stopToken))
            {
                timeout.CancelAfter(TimeSpan.FromSeconds(10));
                using (timeout.Token.Register(() => _ready.TrySetCanceled()))
                {
                    try
                    {
                        await _socket.ConnectAsync(_uri, timeout.Token).ConfigureAwait(false);
                        _receiveTask = ReceiveLoop();
                        await _ready.Task.ConfigureAwait(false);
                    }
                    catch
                    {
                        Dispose();
                        throw;
                    }
                }
            }
        }

        public async Task<PredictionResponse> PredictAsync(PredictionRequest request)
        {
            if (!IsReady) throw new InvalidOperationException("The prediction connection is not ready.");
            var completion = new TaskCompletionSource<PredictionResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
            if (!_pending.TryAdd(request.request_id, completion))
                throw new InvalidOperationException("A request with this ID is already pending.");
            using (var timeout = CancellationTokenSource.CreateLinkedTokenSource(_stopToken))
            {
                // Server deadlines exclude transit; allow another two seconds for the reply.
                timeout.CancelAfter(TimeSpan.FromMilliseconds((long)request.deadline_ms + 2000));
                using (timeout.Token.Register(() => completion.TrySetCanceled()))
                {
                    try
                    {
                        byte[] bytes = Utf8.GetBytes(JsonConvert.SerializeObject(request));
                        await _sendLock.WaitAsync(timeout.Token).ConfigureAwait(false);
                        try
                        {
                            await _socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text,
                                true, timeout.Token).ConfigureAwait(false);
                        }
                        finally { _sendLock.Release(); }
                        return await completion.Task.ConfigureAwait(false);
                    }
                    catch (OperationCanceledException) when (!_stopToken.IsCancellationRequested)
                    {
                        throw new TimeoutException("Timed out waiting for a prediction response.");
                    }
                    finally { _pending.TryRemove(request.request_id, out _); }
                }
            }
        }

        private async Task ReceiveLoop()
        {
            Exception failure = new IOException("The prediction server closed the connection.");
            try
            {
                var buffer = new byte[4096];
                using (var message = new MemoryStream())
                {
                    while (!_stopToken.IsCancellationRequested)
                    {
                        var received = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), _stopToken)
                            .ConfigureAwait(false);
                        if (received.MessageType == WebSocketMessageType.Close) break;
                        if (received.MessageType != WebSocketMessageType.Text)
                            throw new InvalidDataException("The server must send JSON text frames.");
                        if (message.Length + received.Count > MaxResponseBytes)
                            throw new InvalidDataException("Prediction response exceeds the receive limit.");
                        message.Write(buffer, 0, received.Count);
                        if (!received.EndOfMessage) continue;
                        string json = Utf8.GetString(message.ToArray());
                        message.SetLength(0);
                        var response = JsonConvert.DeserializeObject<PredictionResponse>(json);
                        if (response == null || response.version != 1)
                            throw new InvalidDataException("Unsupported prediction protocol version.");
                        if (response.type == "ready")
                        {
                            _isReady = true;
                            _ready.TrySetResult(true);
                        }
                        else if (response.type == "pong") { }
                        else if (response.request_id != null && _pending.TryGetValue(response.request_id, out var pending))
                        {
                            pending.TrySetResult(response);
                        }
                        else if (response.status == "invalid_request" && response.request_id == null)
                        {
                            throw new InvalidDataException("The server rejected a message: " + response.error);
                        }
                    }
                }
            }
            catch (Exception error) { failure = error; }
            finally
            {
                _isReady = false;
                LastError = failure.Message;
                _ready.TrySetException(failure);
                foreach (var pending in _pending.Values) pending.TrySetException(failure);
                if (Volatile.Read(ref _disposed) == 0) _socket.Abort();
            }
        }

        public async Task DisconnectAsync()
        {
            try
            {
                using (var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(2)))
                {
                    await _sendLock.WaitAsync(timeout.Token).ConfigureAwait(false);
                    try
                    {
                        if (_socket.State == WebSocketState.Open || _socket.State == WebSocketState.CloseReceived)
                            await _socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure,
                                "Client disconnecting", timeout.Token).ConfigureAwait(false);
                    }
                    finally { _sendLock.Release(); }
                }
            }
            catch (Exception) { /* Closing must also work after a network failure. */ }
            finally { Dispose(); }
            if (_receiveTask != null) await _receiveTask.ConfigureAwait(false);
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0) return;
            _isReady = false;
            _stop.Cancel();
            _ready.TrySetCanceled();
            foreach (var pending in _pending.Values) pending.TrySetCanceled();
            _socket.Abort();
            _socket.Dispose();
            _stop.Dispose();
        }
    }
}
