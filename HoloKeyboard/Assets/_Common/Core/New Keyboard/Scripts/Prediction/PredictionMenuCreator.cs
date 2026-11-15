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
    /// The current word chunk the player is typing
    /// </summary>
    [SerializeField] private string _currentWordChunk = "";
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
        _client.OnOutboundServerRequest += StartOutboundRequest;
    }
    private void OnDisable()
    {
        _client.OnPredictionsUpdated -= RequestPrediction;
        _client.OnOutboundServerRequest -= StartOutboundRequest;
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
        FilterCurrentInputChunk();
    }
    #endregion

    /// <summary>
    /// Event response to queue an update to the prediction buttons
    /// </summary>
    /// <param name="list"></param>
    public void RequestPrediction(List<PredictionSet> list)
    {
        PredictionSet set = list[list.Count - 1];


        _queuedPredictionSet = set;
    }
    /// <summary>
    /// Response to a prediction request being sent to the server from the client
    /// </summary>
    private void StartOutboundRequest()
    {
        _loadingObject.SetActive(true);
    }
    /// <summary>
    /// Creates the prediction slots for the keyboard. This should be called at the start, or when the number of predictions is updated.
    /// </summary>
    private void CreatePredictionSlots(PredictionSet set)
    {
        _loadingObject.SetActive(false);
        string[] text = new string[3];
        text[0] = _currentWordChunk + set.predictions[0].Contents;
        text[1] = _currentWordChunk + set.predictions[1].Contents;
        text[2] = _currentWordChunk + set.predictions[2].Contents;
        //Remove any buttons currently active under the parent.

        DestroyButtons();
        
        float keyboardLength = Vector3.Distance(new Vector3(_keyboardStart.localPosition.x, 0, 0), new Vector3(_keyboardEnd.localPosition.x, 0, 0));
        //Key length is the keyboard length divided by the number of keys, minus the gap between each key
        float keyLength = (keyboardLength / _predictionNumber) - (_keySpacing);
        keyLength += (_keySpacing) * (1 / (float)_predictionNumber);
        for(int i = 0; i < _predictionNumber; i++)
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
            pButton.SetText(text[i]);
            pButton.SetPrediction(set.predictions[i]);
        }
        _queuedPredictionSet = null;
    }
    public void DestroyButtons()
    {
        Transform[] children = _predictionButtonParent.GetComponentsInChildren<Transform>();
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i] != _predictionButtonParent.transform) Destroy(children[i].gameObject);
        }
    }
    /// <summary>
    /// Filters the current input to get the section used for prediction
    /// </summary>
    private void FilterCurrentInputChunk()
    {
        string input = _keyboardController.CurrentInput;
        string[] words = input.Split(' ');
        _currentWordChunk = words[words.Length - 1];
    }
}
