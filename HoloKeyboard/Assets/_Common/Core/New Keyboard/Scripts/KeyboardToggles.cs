using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HandTrackingMode = HandTracking.HandTrackingMode;
/// <summary>
/// Collection of various toggles for the keyboard.
/// </summary>
public class KeyboardToggles : MonoBehaviour
{
    [SerializeField] private HandTracking _handTracking;
    [SerializeField] private UpdatedEyeTracking _eyeTracking;
    [SerializeField] private KeyboardController _keyboardController;

    public void OnKeyboardInputSwitched(bool active)
    {
        _handTracking.SetTrackingMode(active ? HandTrackingMode.TenFinger : HandTrackingMode.IndexFinger);
    }

    public void OnEyeTrackingStatusChanged(bool active)
    {
        _eyeTracking.SetUsingEyeTracking(active);
    }

    public void OnPlayTextPressed(bool active)
    {
        TTSManager.Stop();
        TTSManager.Say(_keyboardController.CurrentInput);
    }
}
