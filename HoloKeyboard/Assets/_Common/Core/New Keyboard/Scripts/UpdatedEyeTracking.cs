using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

public class UpdatedEyeTracking : MonoBehaviour
{
    /// <summary>
    /// Cursor used to display the location the player is currently looking
    /// </summary>
    [SerializeField] private GameObject _eyeCursor;
    /// <summary>
    /// If eye tracking should be taking place or not
    /// </summary>
    private bool _doEyeTracking = false;
    public bool IsEyeTracking
    {
        get
        {
            return _doEyeTracking;
        }
    }
    public Vector3 CursorPosition => _eyeCursor.transform.position;
    /// <summary>
    /// Hit object used to get data about the eye gaze
    /// </summary>
    private MixedRealityRaycastHit _hit;
    public MixedRealityRaycastHit Hit => _hit;
    /// <summary>
    /// If the eye tracking data is updated for the current fixedupdate.
    /// </summary>
    public bool IsDataUpdated { get; private set; } = false;
    private void Start()
    {
        SetUsingEyeTracking(false);
    }
    private void FixedUpdate()
    {
        DoEyeTracking();
    }

    private void DoEyeTracking()
    {
        //If we aren't eye tracking, we aren't receiving updated data for the frame, and shouldn't continue.
        if (!_doEyeTracking)
        {
            return;
        }

        bool isEyeTrackingEnabled = CoreServices.InputSystem.EyeGazeProvider.IsEyeTrackingEnabled;
        bool? isEyeCalibrationValid = CoreServices.InputSystem.EyeGazeProvider.IsEyeCalibrationValid;
        bool isEyeTrackingDataValid = CoreServices.InputSystem.EyeGazeProvider.IsEyeTrackingDataValid;

        if (isEyeTrackingDataValid)
        {
            _hit = CoreServices.InputSystem.EyeGazeProvider.HitInfo;
            _eyeCursor.transform.position = _hit.point;
            IsDataUpdated = true;
        }
        else
        {
            IsDataUpdated = false;
            _eyeCursor.transform.position = Vector3.one * 1000;
        }
    }
    public void SetUsingEyeTracking(bool enabled)
    {
         _doEyeTracking = enabled;
        if (!enabled) IsDataUpdated = false;
        //Enable or disable the cursor depending on the current mode.
        _eyeCursor.SetActive(enabled);
    }
}
