using Microsoft.MixedReality.Toolkit.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GazeButtonManager : MonoBehaviour
{
    /// <summary>
    /// Public instance of the manager
    /// </summary>
    public static GazeButtonManager Instance;

    /// <summary>
    /// Visual used to indicate the progress on a gaze button
    /// </summary>
    [SerializeField] private GazeIndicator _indicator;
    /// <summary>
    /// Eyetracking used to update current hit data
    /// </summary>
    [SerializeField] private UpdatedEyeTracking _eyeTracking;
    /// <summary>
    /// How long the gaze needs to hover over a key for it to register as a press.
    /// </summary>
    [SerializeField] private float _pressDuration;
    /// <summary>
    /// How long after a successful press the keyboard should wait before trying a new press.
    /// </summary>
    [SerializeField] private float _rePressDelay;
    /// <summary>
    /// Eye raycast data
    /// </summary>
    private MixedRealityRaycastHit _eyeData;
    /// <summary>
    /// What button, if any, is currently being targeted by the eye gaze
    /// </summary>
    private ARButton _targetButton;
    /// <summary>
    /// How far, between 0 and 1, the gaze is to completing a button press.
    /// </summary>
    private float _progress = 0;
    /// <summary>
    /// How much longer is left before the keyboard can start a new press
    /// </summary>
    private float _rePressCooldown = 0;

    /// <summary>
    /// List of all registered buttons tying use for looking up their AR script from their object
    /// </summary>
    private Dictionary<GameObject, ARButton> _buttons = new Dictionary<GameObject, ARButton>();
    public void RegisterButton(ARButton button)
    {
        _buttons.Add(button.gameObject, button);
    }

    private void Awake()
    {
        if (Instance)
        {
            Debug.LogError("An instance of GazeButtonManager already exists!");
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        if (_eyeTracking.IsDataUpdated)
        {
            _eyeData = _eyeTracking.Hit;
            UpdateGazeButtons();
        }
        else
        {
            _progress = 0;
            _indicator.SetProgress(0);
        }
    }
    private void UpdateGazeButtons()
    {
        //Decrement press cooldown
        _rePressCooldown -= Time.deltaTime;
        //If on cooldown, don't check for a press.
        if (_rePressCooldown > 0) return;

        //Check if the output of the gaze hit is a key
        if (_buttons.ContainsKey(_eyeData.transform.gameObject))
        {
            //If a new key is targeted, update the indicator position, and reset the key progress
            if (_targetButton != _buttons[_eyeData.transform.gameObject])
            {
                _progress = 0;
                _indicator.SetPosition(_eyeData.transform.position);
            }
            _targetButton = _buttons[_eyeData.transform.gameObject];
            UpdateTargetButton();
        }
        //If it isn't, set the press progress to zero.
        else
        {
            _progress = 0;
            _indicator.SetProgress(0);
        }
    }

    /// <summary>
    /// Runs every frame while the eye gaze has a valid key target
    /// </summary>
    private void UpdateTargetButton()
    {
        //Increment the progress by the time that passed over the last frame.
        _progress += Time.deltaTime;
        //Update the scale of the gaze indicator to visualize progress.
        _indicator.SetProgress(Mathf.InverseLerp(0, _pressDuration, _progress));
        //If progress passes the target, initiate a keypress.
        if (_progress >= _pressDuration)
        {
            //Set the delay before the next press
            _rePressDelay = _rePressCooldown;
            //Do the key press
            _targetButton.PressButton();
            //Set the target to be null.
            _targetButton = null;
            _progress = 0;
        }
    }
}
