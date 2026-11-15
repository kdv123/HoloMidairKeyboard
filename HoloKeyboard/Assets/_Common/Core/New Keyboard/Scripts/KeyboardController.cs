using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using Unity.VisualScripting;
public class KeyboardController : MonoBehaviour
{
    public string VALUE;
    [ContextMenu("ADD")]
    public void TEST()
    {
        OnKeyPressed(VALUE);
    }
    public static KeyboardController Instance;
    private void Awake()
    {
        Instance = this;
        StartCoroutine(CursorCoroutine());
    }
    public delegate void OnSubmit(string contents);
    public static event OnSubmit _onSubmit;

    [Header("Objects")]
    [SerializeField] private TextMeshPro _inputDisplay;
    [SerializeField] private AudioSource _audio;

    [Header("Data")]
    /// <summary>
    /// List of all keys on the keyboard.
    /// </summary>
    [SerializeField] private List<StandardKey> keys = new List<StandardKey>();

    /// <summary>
    /// Current input string we are dealing with. CurrentInput needs to be updated instead of _currentInput if you want to trigger a UI update and update the alternate action count
    /// </summary>
    private string _currentInput = string.Empty;
    private bool _isShowingCursor = false;
    private bool _enableWordPrediction = false;
    public string CurrentInput
    {
        get
        {
            return _currentInput;
        }
        set
        {
            _currentInput = value;
            if (IsCurrentlyAlternate) _alternateInputsRemaining -= 1;
            UpdateKeys();
        }
    }
    private IEnumerator CursorCoroutine()
    {
        while (true)
        {
            if (_isShowingCursor)
            {
                _isShowingCursor = false;
            }
            else
            {
                _isShowingCursor = true;
            }
            yield return new WaitForSeconds(0.8f);
        }
    }
    public void EnableWordPrediction()
    {
        _enableWordPrediction = true;
    }
    private void Update()
    {
        if (_isShowingCursor)
        {
            _inputDisplay.text = CurrentInput + "|";
        }
        else
        {
            _inputDisplay.text = CurrentInput;
        }
    }
    /// <summary>
    /// How many times the keyboard should register the alt input instead of the usual. 
    /// </summary>
    private int _alternateInputsRemaining = 0;
    public int AlternateInputsRemaining => _alternateInputsRemaining;
    public bool IsCurrentlyAlternate => _alternateInputsRemaining > 0;

    public void ClearInput()
    {
        _currentInput = "";
    }
    /// <summary>
    /// Called by a key when it registers that a press has occured.
    /// </summary>
    /// <param name="value">Value of the key sending the event.</param>
    public void OnKeyPressed(string value)
    {
        switch (value)
        {
            case "BCKSP": OnBackspace(); break;
            case "SHIFT": OnShift(); break;
            case "ENTER": OnEnter(); break;
            case "CTRL": OnControl(); break;
            case "CAPS": OnCapslock(); break;
            case "TAB": OnTab(); break;
            default: OnBasicKey(value); break;
        }
        LogManager.Instance.CreateKeyEntryLog(value);
        if(_enableWordPrediction) SendMessageToServer();
        PlayClickSound();
    }
    [ContextMenu("Play")]
    public void eee()
    {
        OnKeyPressed("a");
    }
    private void PlayClickSound()
    {
        _audio.Play();
        Debug.Log("Played");
    }
    private void SendMessageToServer()
    {
        _ = Client.Instance.CreateMessage(_currentInput);
    }
    /// <summary>
    /// Backspace event triggered if a key's value was BCKSP
    /// </summary>
    private void OnBackspace()
    {
        //Don't backspace an empty string
        if (_currentInput.Length == 0) return;
        CurrentInput = CurrentInput.Remove(CurrentInput.Length - 1, 1);
    }
    /// <summary>
    /// Shift event triggered if a key's value was SHIFT
    /// </summary>
    private void OnShift()
    {
        _alternateInputsRemaining += 1;
        UpdateKeys();
    }
    /// <summary>
    /// Enter event triggered if a key's value was ENTER
    /// </summary>
    private void OnEnter()
    {
        if (CurrentInput == "" || CurrentInput == string.Empty) return;
        CurrentInput += "\n";
        _onSubmit?.Invoke(_currentInput);
    }
    /// <summary>
    /// Control event triggered if a key's value was CTRL
    /// </summary>
    private void OnControl()
    {

    }
    /// <summary>
    /// Capslock event triggered if a key's value was CAPS
    /// </summary>
    private void OnCapslock()
    {
        //Also capslock even if shift is pressed, which is why we check greater than one and not zero.
        if (_alternateInputsRemaining > 1)
        {
            _alternateInputsRemaining = 0;
        }
        else
        {
            _alternateInputsRemaining = int.MaxValue;
        }
        UpdateKeys();
    }
    /// <summary>
    /// Tab event triggered if a key's value was TAB
    /// </summary>
    private void OnTab()
    {
        CurrentInput += "     ";
    }
    /// <summary>
    /// Standard event for keys that don't have a special function. 
    /// </summary>
    /// <param name="val">Value the key holds</param>
    private void OnBasicKey(string val)
    {
        CurrentInput += val;
    }

    public void ReplaceText(string text, int numToDelete)
    {
        string input = CurrentInput.Substring(0, CurrentInput.Length - numToDelete);
        input = input + text;
        CurrentInput = input;
    }

    /// <summary>
    /// Helper function to assign the text of each key in the inspector.
    /// </summary>
    [ContextMenu("Setup keys")]
    private void UpdateKeys()
    {
        foreach (StandardKey key in keys)
        {
            key.UpdateKey();
        }
    }
}
[Serializable]
public class KeyData
{
    public string DefaultInput;
    public string AlternateInput;
    public bool DisplayBoth = false;
}