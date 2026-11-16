using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using CSWebSockets.Structs;
public class PredictionMenuCreator : MonoBehaviour
{
    public static PredictionMenuCreator Instance { get; private set; }
    [Header("Objects")]
    [SerializeField] private Client _client;
    /// <summary>
    /// Template object to be created for the predictions
    /// </summary>
    [SerializeField] private GameObject _predictionButtonTemplate;
    /// <summary>
    /// Parent transform the prediction buttons will be spawned under
    /// </summary>
    [SerializeField] private Transform _predictionButtonParent;
    /// <summary>
    /// Keyboard objects used for calculating the length of the keyboard
    /// </summary>
    [SerializeField] private Transform _keyboardStart;
    [SerializeField] private Transform _keyboardEnd;
    /// <summary>
    /// Keyboard controller 
    /// </summary>
    [SerializeField] private KeyboardController _keyboardController;
    /// <summary>
    /// Loading icon to display while waiting for next server request
    /// </summary>
    [SerializeField] private GameObject _loadingObject;
    [Header("Data")]
    [SerializeField] private float _textScale;
    /// <summary>
    /// How many prediction slots to show
    /// </summary>
    [SerializeField] private int _predictionNumber;
    /// <summary>
    /// How tall the prediction keys should be
    /// </summary>
    [SerializeField] private float _keyHeight;
    /// <summary>
    /// How the keys should be rotated after spawning
    /// </summary>
    [SerializeField] private Vector3 _keyRotation;
    /// <summary>
    /// How large the gap between each prediction key should be
    /// </summary>
    [SerializeField] private float _keySpacing = 0.02f;
    /// <summary>
    /// If a prediction set is queued for next update
    /// </summary>
    private PredictionSet? _queuedPredictionSet = null;
    /// <summary>
    /// If the next prediction hit can actually be counted or not
    /// </summary>
    private bool _allowPredictionHits = true;
    public bool GetAllowPredictionHits()
    {
        return _allowPredictionHits;
    }
    public void TogglePredictionHits(bool allow)
    {
        _allowPredictionHits = allow;
    }
    #region Built-In Methods
    private void Start()
    {
        Instance = this;
        _loadingObject.SetActive(false);
    }
    private void OnEnable()
    {
        _client.OnPredictionsUpdated += RequestPrediction;
        _client.OnRequestStateChanged += SetLoading;
        _client.OnPredictionsInvalidated += ClearPredictions;
    }
    private void OnDisable()
    {
        _client.OnPredictionsUpdated -= RequestPrediction;
        _client.OnRequestStateChanged -= SetLoading;
        _client.OnPredictionsInvalidated -= ClearPredictions;
        ClearPredictions();
    }
    private void Update()
    {
        if (_queuedPredictionSet.HasValue)
        {
            CreatePredictionSlots(_queuedPredictionSet.Value);
        }
        if (_loadingObject.activeInHierarchy)
        {
            _loadingObject.transform.Rotate(Vector3.back * Time.deltaTime * 360);
        }
    }
    #endregion

    /// <summary>
    /// Event response to queue an update to the prediction buttons
    /// </summary>
    /// <param name="set">Predictions for one current text snapshot.</param>
    public void RequestPrediction(PredictionSet set)
    {
        _queuedPredictionSet = set;
    }
    /// <summary>
    /// Response to a prediction request being sent to the server from the client
    /// </summary>
    private void SetLoading(bool loading)
    {
        if (_loadingObject != null) _loadingObject.SetActive(loading);
    }
    private void ClearPredictions()
    {
        _queuedPredictionSet = null;
        SetLoading(false);
        DestroyButtons();
    }
    /// <summary>
    /// Creates the prediction slots for the keyboard. This should be called at the start, or when the number of predictions is updated.
    /// </summary>
    private void CreatePredictionSlots(PredictionSet set)
    {
        _loadingObject.SetActive(false);
        _queuedPredictionSet = null;
        DestroyButtons();
        int count = Mathf.Min(_predictionNumber, set.predictions?.Length ?? 0);
        if (count <= 0 || !_client.IsCurrentPrediction(set.predictions[0])) return;
        
        float keyboardLength = Vector3.Distance(new Vector3(_keyboardStart.localPosition.x, 0, 0), new Vector3(_keyboardEnd.localPosition.x, 0, 0));
        //Key length is the keyboard length divided by the number of keys, minus the gap between each key
        float keyLength = (keyboardLength - _keySpacing * (count - 1)) / count;
        for(int i = 0; i < count; i++)
        {
            ResizeableButton button = Instantiate(_predictionButtonTemplate, Vector3.zero, Quaternion.identity, _predictionButtonParent).GetComponent<ResizeableButton>();
            button.transform.localRotation = Quaternion.Euler(_keyRotation.x, _keyRotation.y, _keyRotation.z);
            //Resize in local space so starts at (0, 0, 0)
            float gap = i * _keySpacing;
            button.Resize(new Vector3((i * keyLength) + gap, -_keyHeight, 0), new Vector3(((i + 1) * keyLength) + gap, 0, 0), localSpace: true);
            PredictionButton pButton = button.GetComponent<PredictionButton>();
            pButton.ResizeText(Vector3.one);
            //pButton.ResizeText(new Vector3((button.transform.localScale.x / _textScale), button.transform.localScale.y, button.transform.localScale.z));
            //pButton.ResizeText(new Vector3(1, 2.5f, 1));
            pButton.SetText(set.predictions[i].Contents);
            pButton.SetPrediction(set.predictions[i]);
        }
    }
    public void DestroyButtons()
    {
        if (_predictionButtonParent == null) return;
        for (int i = _predictionButtonParent.childCount - 1; i >= 0; i--)
        {
            GameObject child = _predictionButtonParent.GetChild(i).gameObject;
            child.SetActive(false);
            Destroy(child);
        }
    }
}
