using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;
using System.Threading.Tasks;
public class NomonGenerator : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Transform _parentTransform;
    [SerializeField] private GameObject _panelPrefab;
    [Tooltip("Text component displaying the current sentance output")]
    [SerializeField] private TextMeshPro _outputText;
    [Tooltip("Object used to scale and move the interface")]
    [SerializeField] private Transform _panelMoveObject;
    [Tooltip("Object used to rotate the interface")]
    [SerializeField] private Transform _panelRotateObject;
    [Header("Parameters")]
    [Tooltip("Amount of panels to place horizontally before going down a row")]
    [SerializeField] private int _panelsPerLine;
    [Tooltip("How far to offset each panel on the X axis")]
    [SerializeField] private float _panelXOffset;
    [Tooltip("How far to offset each panel on the Y axis")]
    [SerializeField] private float _panelYOffset;
    [Tooltip("Rotation speed of the clock hand, in revolutions per second.")]
    [SerializeField] private float _rotationSpeed = 0.3f;
    //Create the list of words we want the interface to support
    private List<string> words = new List<string>() { "the", "be", "to", "of", "and", "a", "in", "that", "have", "I", "it", "for", "not", "on", "with", "he", "as", "you", "do", "at", "this", "but", "his", "by", "from", "they", "we", "say", "her", "she", "or", "an", "will", "my", "one", "all", "would", "there", "their", "what", "so", "up", "out", "if", "about", "who", "get", "which", "go", "me", "when", "make", "can", "like", "time", "no", "just", "him", "know", "take", "people", "into", "year", "your", "good", "some", "could", "them", "see", "other", "than", "then", "now", "look", "only", "come", "its", "over", "think", "also", "back", "after", "use", "two", "how", "our", "work", "first", "well", "way", "even", "new", "want", "because", "any", "these", "give", "day", };
    //Store each panel script against its transform so we can access a panel by its transform later without needing a GetComponent() call
    private Dictionary<Transform, NomonPanel> _panelFromTransform = new Dictionary<Transform, NomonPanel>();
    //List containing all panels that are currently active and selectable
    private List<NomonPanel> _activePanels = new List<NomonPanel>();
    //Current string that has been created by the output selections
    private string _currentOutput = "";
    //Reference to the task to update the panel so it can be aborted at the end of runtime
    private Task _updateMoveRotationTask;
    void Start()
    {
        //Ensure the panel generator has no parent at the start
        transform.parent = null;
        //Calculate the amount of whole rows
        int rows = (int)Mathf.Floor((float)words.Count / _panelsPerLine);
        //Calculate how many panels to place in the remainder row
        int r = words.Count % _panelsPerLine;
        //Keep track of the current spawn positions
        float currX = 0;
        float currY = 0;
        //Keep track of the current word we want to add to a panel
        int wordIndex = 0;

        //Create the grid of panels
        for(int y = 0; y < rows; y++)
        {
            for(int x = 0; x < _panelsPerLine; x++)
            {
                CreatePanel(currX, currY, words[wordIndex]);
                currX += _panelXOffset;
                wordIndex += 1;
            }
            currY -= _panelYOffset;
            currX = 0;
        }
        //Create the final row of any remainders
        for(int x = 0; x < r; x++)
        {
            CreatePanel(currX, currY, words[wordIndex]);
            currX += _panelXOffset;
            wordIndex += 1;
        }

        //Position the moveable transform
        _panelMoveObject.transform.position = _parentTransform.position + new Vector3(_panelXOffset * _panelsPerLine, 0, 0);
        //Add this as a child of the moveable transform so it can be scaled
        transform.parent = _panelRotateObject;
        //Start the task to update the move transform's rotation
        _updateMoveRotationTask = UpdateMoveTransformRotation();
    }
    private void OnDestroy()
    {
        _updateMoveRotationTask?.Dispose();   
    }
    private float _currentRotationOffset = 0;
    private void CreatePanel(float x, float y, string text)
    {
        //Instantiate the panel and rotate it to face the current view
        NomonPanel panel = Instantiate(_panelPrefab, _parentTransform.position + new Vector3(x, y, 0), Quaternion.identity, _parentTransform).GetComponent<NomonPanel>();
        panel.transform.eulerAngles = new Vector3(90, -180, 0);
        //Add the panel to the tracked lists
        _panelFromTransform.Add(panel.transform, panel);
        _activePanels.Add(panel);
        //Initialize the panel with its word, and start the rotation
        panel.SetText(text);
        panel.SetRotationSpeed(_rotationSpeed);
        panel.SetDegrees(_currentRotationOffset);
        _currentRotationOffset += 30;
        if (_currentRotationOffset >= 360) _currentRotationOffset = 0;
    }
    //List used for marking indexes 
    private List<int> _indexesForRemoval = new List<int>();
    /// <summary>
    /// Called when an input is recieved that it should select the current clocks at noon.
    /// </summary>
    public void OnSelect()
    {
        //Keep track of the indexes that should be removed - panels can't be removed directly because they need to be restored if no panel ends up selected.
        bool _hasSelectedPanel = false;
        _indexesForRemoval.Clear();
        //Iterate through all the panels that are currently active
        for(int i = _activePanels.Count - 1; i >= 0; i--)
        {
            //If the error in the panel's clock hand is less than 15 degrees from noon, keep it selected.
            if (_activePanels[i].GetDegrees() < 15f || _activePanels[i].GetDegrees() > 345)
            {
                //Keep panel enabled
                _hasSelectedPanel = true;
            }
            else
            {
                _indexesForRemoval.Add(i);
            }
        }
        if (!_hasSelectedPanel)
        {
            //If no panel's clock rotation matched when we triggered the input, don't remove any panels and just reset the rotation of the different panels.
        }
        else
        {
            //If we did select at least one panel, disable and remove from the active list all the panels that were not selected.
            foreach(int i in _indexesForRemoval)
            {
                _activePanels[i].SetDisabled();
                _activePanels.RemoveAt(i);
            }
            //If there is only one panel left active, add its word to the output and reset.
            if(_activePanels.Count == 1)
            {
                _currentOutput += _activePanels[0].GetWord() + " ";
                UpdateOutput(_currentOutput);
                ResetPanels();
                return;
            }
        }
        _currentRotationOffset = 0;
        foreach(NomonPanel panel in _activePanels)
        {
            panel.SetDegrees(_currentRotationOffset); 
            _currentRotationOffset += 30;
            if (_currentRotationOffset >= 360) _currentRotationOffset = 0;
        }
    }
    private void ResetPanels()
    {
        _currentRotationOffset = 0;
        _activePanels.Clear();
        foreach(NomonPanel p in _panelFromTransform.Values)
        {
            p.SetEnabled();
            p.SetDegrees(_currentRotationOffset);
            _currentRotationOffset += 30;
            if (_currentRotationOffset >= 360) _currentRotationOffset = 0;
            _activePanels.Add(p);
        }
    }
    /// <summary>
    /// Updates the current output text
    /// </summary>
    /// <param name="output">String to display</param>
    private void UpdateOutput(string output)
    {
        _outputText.text = output;
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnSelect();
        }
    }

    private async Task UpdateMoveTransformRotation()
    {
        return;
        while (true)
        {
            await Task.Delay(50);
            _panelMoveObject.transform.rotation = _panelRotateObject.transform.rotation;
            _panelRotateObject.transform.localEulerAngles = Vector3.zero;
        }
    }
}
