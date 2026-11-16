using Microsoft.MixedReality.Toolkit.Input;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GazeKeyboard : MonoBehaviour
{
    /// <summary>
    /// Eyetracking used to update current hit data
    /// </summary>
    [SerializeField] private UpdatedEyeTracking _eyeTracking;
    /// <summary>
    /// Visual used to indicate the progress on the gaze keyboard.
    /// </summary>
    [SerializeField] private GazeIndicator _indicator;
    /// <summary>
    /// How long the gaze needs to hover over a key for it to register as a press.
    /// </summary>
    [SerializeField] private float _pressDuration;
    /// <summary>
    /// How long after a successful press the keyboard should wait before trying a new press.
    /// </summary>
    [SerializeField] private float _rePressDelay;
    /// <summary>
    /// List of all keys on the keyboard
    /// </summary>
    [SerializeField] private Dictionary<GameObject, StandardKey> _keys = new Dictionary<GameObject, StandardKey>();
    /// <summary>
    /// Eye raycast data
    /// </summary>
    private MixedRealityRaycastHit _eyeData;
    /// <summary>
    /// What key, if any, is currently being targeted by the eye gaze
    /// </summary>
    private StandardKey _targetKey;
    /// <summary>
    /// How far, between 0 and 1, the gaze is to completing a keypress.
    /// </summary>
    private float _progress = 0;
    /// <summary>
    /// How much longer is left before the keyboard can start a new press
    /// </summary>
    private float _rePressCooldown = 0;
    private void Start()
    {
        //Cache all the available keys in the scene
        StandardKey[] keys = FindObjectsOfType<StandardKey>();
        foreach(StandardKey key in keys)
        {
            _keys.Add(key.gameObject, key);
        }
    }
    private void Update()
    {
        if (_eyeTracking.IsDataUpdated)
        {
            _eyeData = _eyeTracking.Hit;
            UpdateGazeKeyboard();
        }
        else
        {
            _progress = 0;
            _indicator.SetProgress(0);
        }
    }
    private void UpdateGazeKeyboard()
    {
        //Decrement press cooldown
        _rePressCooldown -= Time.deltaTime;
        //If on cooldown, don't check for a press.
        if (_rePressCooldown > 0) return;
        //Check if the output of the gaze hit is a key
        if (_keys.ContainsKey(_eyeData.transform.gameObject))
        {
            //If a new key is targeted, update the indicator position, and reset the key progress
            if(_targetKey != _keys[_eyeData.transform.gameObject])
            {
                _progress = 0;
                _indicator.SetPosition(_eyeData.transform.position);
            }
            _targetKey = _keys[_eyeData.transform.gameObject];
            UpdateTargetKey();
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
    private void UpdateTargetKey()
    {
        //Increment the progress by the time that passed over the last frame.
        _progress += Time.deltaTime;
        //Update the scale of the gaze indicator to visualize progress.
        _indicator.SetProgress(Mathf.InverseLerp(0, _pressDuration, _progress));
        //If progress passes the target, initiate a keypress.
        if(_progress >= _pressDuration)
        {
            //Set the delay before the next press
            _rePressDelay = _rePressCooldown;
            //Do the key press
            _targetKey.Press();
            //Set the target to be null.
            _targetKey = null;
            _progress = 0;
        }
    }
}
