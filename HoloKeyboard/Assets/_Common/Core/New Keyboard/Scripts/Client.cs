using CSWebSockets;
using CSWebSockets.Structs;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class Client : MonoBehaviour
{
    public static Client Instance { get; private set; }
    public Action<PredictionSet> OnPredictionsUpdated;
    public Action OnPredictionsInvalidated;
    public Action<bool> OnRequestStateChanged;

    [Header("TextSlinger server")]
    [SerializeField] private string _serverUrl = "wss://cheetah.textslinger.org:8443/api/v1/ws";
    [SerializeField] private string _model = "ngram";
    [SerializeField, Min(1)] private int _predictionCount = 3;
    [SerializeField, Min(1)] private int _deadlineMs = 10000;
    // Supply authentication at runtime if the deployment enables it; do not serialize secrets.
    public string BearerToken { private get; set; }
    public string TestInput;

    private readonly NoiselessPredictionSession _session = new NoiselessPredictionSession();
    private WebSocketClient _socket;
    private bool _connecting;
    private bool _connected;
    private bool _allowReconnect = true;
    private bool _destroyed;
    private bool _wantsPredictions;
    private bool _pending;
    private bool _hasCurrentResult;
    private float _retryAt;
    private float _retryDelay = 2f;

    private void Awake() { Instance = this; }
    private void Start() { _ = TryConnect(); }

    private void Update()
    {
        if (_connected && (_socket == null || !_socket.IsReady))
        {
            _connected = false;
            InvalidatePredictions();
            Debug.LogWarning("Prediction connection closed: " + _socket?.LastError);
            _retryAt = Time.unscaledTime + _retryDelay;
        }
        if (_allowReconnect && !_connecting && !_connected && Time.unscaledTime >= _retryAt)
            _ = TryConnect();
    }

    [ContextMenu("Send data")]
    public void Test() { _ = CreateMessage(TestInput); }

    public async Task<bool> TryConnect()
    {
        if (_destroyed || _connecting) return false;
        if (_socket != null && _socket.IsReady) return true;
        _allowReconnect = true;
        _connecting = true;
        WebSocketClient socket = null;
        try
        {
            _socket?.Dispose();
            InvalidatePredictions();
            socket = new WebSocketClient(_serverUrl, BearerToken);
            _socket = socket;
            await socket.ConnectAsync();
            if (_destroyed || _socket != socket) return false;
            _connected = true;
            _retryDelay = 2f;
            Debug.Log("Connected to TextSlinger: " + _serverUrl);
            // Replay only the latest text, retaining this session's revision history.
            if (_wantsPredictions) _ = CreateMessage(_session.Text);
            return true;
        }
        catch (Exception error)
        {
            socket?.Dispose();
            if (!_destroyed && (socket == null || _socket == socket))
            {
                _connected = false;
                Debug.LogWarning("Unable to connect to TextSlinger: " + error.Message);
                _retryAt = Time.unscaledTime + _retryDelay;
                _retryDelay = Mathf.Min(30f, _retryDelay * 2f);
            }
            return false;
        }
        finally { _connecting = false; }
    }

    public async Task Disconnect()
    {
        _allowReconnect = false;
        _connected = false;
        var socket = _socket;
        _socket = null;
        InvalidatePredictions();
        if (socket != null) await socket.DisconnectAsync();
    }

    // Called immediately for every text mutation, even before requests are enabled.
    public void UpdateText(string text)
    {
        if (_session.SetText(text)) InvalidatePredictions();
    }

    public bool IsCurrentPrediction(Prediction prediction)
    {
        return _connected && _socket != null && _socket.IsReady && _hasCurrentResult &&
            _session.IsCurrent(prediction);
    }

    public async Task<bool> CreateMessage(string text)
    {
        if (_destroyed) return false;
        UpdateText(text);
        _wantsPredictions = true;
        if (_pending || _hasCurrentResult) return true;
        var socket = _socket;
        if (socket == null || !socket.IsReady) return false;
        PredictionRequest request = null;
        try
        {
            request = _session.CreateRequest(_model, _predictionCount, _deadlineMs);
            SetPending(true);
            LogManager.Instance?.CreateServerRequestLog(request);
            var timer = Stopwatch.StartNew();
            var response = await socket.PredictAsync(request);
            if (_destroyed || _socket != socket || !_session.IsCurrent(request)) return false;
            if (!_session.TryReadPredictions(request, response, out var predictions)) return false;
            _hasCurrentResult = true;
            LogManager.Instance?.CreateServerResponseLog(predictions, timer.Elapsed.TotalMilliseconds, response);
            OnPredictionsUpdated?.Invoke(predictions);
            return true;
        }
        catch (Exception error)
        {
            if (!_destroyed && _socket == socket && (request == null || _session.IsCurrent(request)))
            {
                Debug.LogWarning("Prediction request failed: " + error.Message);
                InvalidatePredictions();
            }
            return false;
        }
        finally
        {
            if (!_destroyed && _socket == socket && request != null && _session.IsCurrent(request))
                SetPending(false);
        }
    }

    private void InvalidatePredictions()
    {
        _session.Invalidate();
        _hasCurrentResult = false;
        SetPending(false);
        OnPredictionsInvalidated?.Invoke();
    }

    private void SetPending(bool pending)
    {
        _pending = pending;
        OnRequestStateChanged?.Invoke(pending);
    }

    private void OnDestroy()
    {
        _destroyed = true;
        _allowReconnect = false;
        _session.Invalidate();
        _socket?.Dispose();
        if (Instance == this) Instance = null;
    }
}
