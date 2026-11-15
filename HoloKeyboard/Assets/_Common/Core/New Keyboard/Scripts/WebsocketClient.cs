using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using CSWebSockets.Structs;
using System.Linq;
using UnityEngine;
namespace CSWebSockets{
    public class WebSocketClient
    {

        public Action OnDisconnect;
        public Action<ServerPrediction> OnRecieveResponse;

        [SerializeField] private bool _debugLogging = false;
        private ClientWebSocket _webSocket;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly Uri _uri;
        private readonly SemaphoreSlim _sendSemaphore = new(1, 1);
        private int _nextID = -1;
        public int GetNextID()
        {
            _nextID += 1;
            return _nextID;
        }
        /// <summary>
        /// The current connection state of the websocket, or none if the socket doesn't exist.
        /// </summary>
        public WebSocketState ClientState => _webSocket == null ? WebSocketState.None : _webSocket.State;

        public WebSocketClient(string uri)
        {
            _uri = new Uri(uri);
        }
        ~WebSocketClient()
        {
            if(OnRecieveResponse != null)
            {
                OnRecieveResponse = null;
            }
        }
        public async Task ConnectAsync()
        {
            _webSocket = new ClientWebSocket();
            _cancellationTokenSource = new CancellationTokenSource();

            // Configure WebSocket options
            _webSocket.Options.KeepAliveInterval = TimeSpan.FromSeconds(20);
            // _webSocket.Options.SetRequestHeader("Authorization", $"Bearer {GetAuthToken()}");
            _webSocket.Options.AddSubProtocol("chat");

            try
            {
                await _webSocket.ConnectAsync(_uri, _cancellationTokenSource.Token);
                if(_debugLogging) Debug.Log($"Connected to {_uri}");

                // Start receiving messages
                _ = Task.Run(ReceiveLoop);

                // Send initial message
                // await SendMessageAsync(new { type = "subscribe", channel = "updates" });
            }
            catch (Exception ex)
            {
                Debug.LogError($"Connection failed: {ex.Message}");
                throw;
            }
        }

        private async Task ReceiveLoop(){
            var buffer = new ArraySegment<byte>(new byte[4096]);

            while (_webSocket.State == WebSocketState.Open){
                try{
                    var result = await _webSocket.ReceiveAsync(
                        buffer,
                        _cancellationTokenSource.Token
                    );
                    if (result.MessageType == WebSocketMessageType.Binary){
                        await HandleBinaryMessage(buffer.Array.Take(result.Count).ToArray());
                    }else{
                        throw new Exception("Should have been binary");
                    }
                }
                catch (WebSocketException ex)
                {
                    Debug.LogError($"WebSocket error: {ex.Message}");
                    await HandleError(ex);
                    break;
                }
            }
        }

        public async Task SendBinaryAsync(byte[] data){
            if (_webSocket.State != WebSocketState.Open){
                throw new InvalidOperationException("WebSocket is not connected");
            }

            await _sendSemaphore.WaitAsync();
            try{
                await _webSocket.SendAsync(
                    new ArraySegment<byte>(data),
                    WebSocketMessageType.Binary,
                    true,
                    _cancellationTokenSource.Token
                );
            }
            finally{
                _sendSemaphore.Release();
            }
        }

        public async Task DisconnectAsync()
        {
            if (_webSocket.State == WebSocketState.Open){
                await _webSocket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Client disconnecting",
                    CancellationToken.None
                );
            }

            _cancellationTokenSource?.Cancel();
            _webSocket?.Dispose();
        }

        private async Task HandleBinaryMessage(byte[] data){
            ServerPrediction predictions = JsonConvert.DeserializeObject<ServerPrediction>(Encoding.ASCII.GetString(data));
            if (_debugLogging) Debug.Log($"Received binary data: {data.Length} bytes. Data saved to struct. Raw Data:\n{Encoding.ASCII.GetString(data)}\nStruct Content:\nID: {predictions.ID}\nPredictions: ");
            foreach (string predict in predictions.predictions){
                if (_debugLogging) Debug.Log($"{predict}, ");
            }
            OnRecieveResponse?.Invoke(predictions);
            // Process binary data

        }

        private async Task HandleClose(WebSocketReceiveResult result){
            Debug.Log($"Connection closed: {result.CloseStatus} - {result.CloseStatusDescription}");
        }

        private async Task HandleError(Exception ex)
        {
            Debug.LogError($"Error: {ex.Message}");
            // Implement reconnection logic
        }

        private string GetAuthToken()
        {
            // Retrieve authentication token
            return "your-auth-token";
        }
    }
}