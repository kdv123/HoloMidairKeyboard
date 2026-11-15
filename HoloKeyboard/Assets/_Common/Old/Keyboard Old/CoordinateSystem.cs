using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class CoordinateSystem : MonoBehaviour
{
    #region Values
    public enum CoordinateTriggerPositionMode
    {
        EventTriggerPosition,
        IdleKeyPosition
    }
    public enum CoordinateType
    {
        Normalized,
        Keyboard
    }
    public enum CapturePosition
    {
        /// <summary>
        /// Capture happened at the moment the key registers a press event
        /// </summary>
        EventTrigger, 
        /// <summary>
        /// Capture happens when a finger first comes into contact with a key
        /// </summary>
        IdleKey
    }
    //Top left and bottom right positions of where they key first counts as 'pressed'
    [SerializeField] private Transform _topLeftEventTriggerPosition;
    [SerializeField] private Transform _bottomRightEventTriggerPosition;
    //Top left and bottom right positions of where the keys cound as their default 'idle'.
    [SerializeField] private Transform _topLeftIdleKeyPosition;
    [SerializeField] private Transform _bottomRightIdleKeyPosition;
    //Heatmaps used to visualize location data
    [SerializeField] private HeatmapDisplay _eventTriggerPositionHeatmap;
    [SerializeField] private HeatmapDisplay _idleKeyPositionHeatmap;
    //Editor fields to show the maximum coordinates in keyboard space
    [SerializeField] private Vector2 _keyboardIdlePositionSpan;
    [SerializeField] private Vector2 _keyboardTriggerPositionSpan;
    //Instance of the coordinate system
    public static CoordinateSystem Instance { get; private set; }
    #endregion

    #region Built-In Methods
    private void Start()
    {
        //Assign the static instance
        Instance = this;
        master._onCoordinateTypeUpdated += OnCoordinateTypeUpdated;
        //Initialize both heatmaps with their respective coordinate pointss
        _eventTriggerPositionHeatmap.InitializeHeatmap(_bottomRightEventTriggerPosition.position.x - _topLeftEventTriggerPosition.position.x, _topLeftEventTriggerPosition.position.z - _bottomRightEventTriggerPosition.position.z);
        _idleKeyPositionHeatmap.InitializeHeatmap(_bottomRightIdleKeyPosition.position.x - _topLeftIdleKeyPosition.position.x, _topLeftIdleKeyPosition.position.z - _bottomRightIdleKeyPosition.position.z);
        RecalculateKeyboardSpan();
    }
    private void OnDestroy()
    {
        master._onCoordinateTypeUpdated -= OnCoordinateTypeUpdated;
    }
    #endregion

    #region Custom Methods
    /// <summary>
    /// Displays the normalized position from a hit point.
    /// </summary>
    /// <param name="position">The world position of the point</param>
    /// /// <param name="mode">The coordinate mode the point was captured with (Trigger or Idle)</param>
    public void DisplayNormalizedPoint(Vector3 position, CoordinateTriggerPositionMode mode)
    {
        Vector3? normalizedPosition = NormalizePointInKeyboardSpace(position, mode);
        if (normalizedPosition.HasValue)
        {
            if (mode == CoordinateTriggerPositionMode.EventTriggerPosition)
            {
                _eventTriggerPositionHeatmap.AddPointToValsNormalized(normalizedPosition.GetValueOrDefault().x, normalizedPosition.GetValueOrDefault().z, 20);
            }
            else
            {
                _idleKeyPositionHeatmap.AddPointToValsNormalized(normalizedPosition.GetValueOrDefault().x, normalizedPosition.GetValueOrDefault().z, 20);
            }
        }
    }
    /// <summary>
    /// Displays the position of a hit point in keyboard space
    /// </summary>
    /// <param name="position">The world position of the point</param>
    /// <param name="mode">The coordinate mode the point was captured with (Trigger or Idle)</param>
    public void DisplayPointInKeyboardSpace(Vector3 position, CoordinateTriggerPositionMode mode)
    {
        Vector3? keyboardPosition = GetPointInKeyboardSpace(position, mode);
        if (keyboardPosition.HasValue)
        {
            if(mode == CoordinateTriggerPositionMode.EventTriggerPosition)
            {
                _eventTriggerPositionHeatmap.AddPointToVals(keyboardPosition.GetValueOrDefault().x, keyboardPosition.GetValueOrDefault().z, 20, _keyboardTriggerPositionSpan);
            }
            else
            {
                _idleKeyPositionHeatmap.AddPointToVals(keyboardPosition.GetValueOrDefault().x, keyboardPosition.GetValueOrDefault().z, 20, _keyboardIdlePositionSpan);
            }
        }
    }
    /// <summary>
    /// Gets the coordinates in world units of an input vector, relative to the position of the keyboard. This means that the bounds are bigger the larger the physical keyboard is.
    /// </summary>
    /// <param name="worldPoint">World position of the input point</param>
    /// <param name="mode">What set of anchors the calculation should be performed with respect to.</param>
    /// <returns>The input point in keyboard space, clamped between 0 and the bounds of the keyboard on both axis.</returns>
    public Vector3? GetPointInKeyboardSpace(Vector3 worldPoint, CoordinateTriggerPositionMode mode)
    {
        //Select the position pair to use
        Transform topLeft = mode == CoordinateTriggerPositionMode.EventTriggerPosition ? _topLeftEventTriggerPosition : _topLeftIdleKeyPosition;
        Transform bottomRight = mode == CoordinateTriggerPositionMode.EventTriggerPosition ? _bottomRightEventTriggerPosition : _bottomRightIdleKeyPosition;

        //Get the point by setting the top left as (0,0)
        float xPos = worldPoint.x - topLeft.position.x;
        float zPos = topLeft.position.z - worldPoint.z;

        //Clamp the output to the bounds of the keyboard
        Vector2 bounds = mode == CoordinateTriggerPositionMode.EventTriggerPosition ? _keyboardTriggerPositionSpan : _keyboardIdlePositionSpan;
        if (xPos < 0) xPos = 0;
        if (xPos > bounds.x) xPos = bounds.x;
        if (zPos < 0) zPos = 0;
        if (zPos > bounds.y) zPos = bounds.y;
        //Return the output vector
        return new Vector3(xPos, 0, zPos);
    }
    /// <summary>
    /// Tries to normalize a point within the keyboard space provided by the scripts bounds.
    /// </summary>
    /// <param name="worldPoint">World position of the point to normalize.</param>
    /// <param name="mode"> What set of anchors the calculation should be performed with respect to. </param>
    /// <returns>The position of the point normalized between 0 and 1 in keyboard space, or null if the provided point is outside of the bounds of the keyboard.</returns>
    public Vector3? NormalizePointInKeyboardSpace(Vector3 worldPoint, CoordinateTriggerPositionMode mode)
    {
        //Select the position pair to use
        Transform topLeft = mode == CoordinateTriggerPositionMode.EventTriggerPosition ? _topLeftEventTriggerPosition : _topLeftIdleKeyPosition;
        Transform bottomRight = mode == CoordinateTriggerPositionMode.EventTriggerPosition ? _bottomRightEventTriggerPosition : _bottomRightIdleKeyPosition;

        //Normalize the point by lerping it between the absolute bounds of the keyboard.
        float xPos = Mathf.InverseLerp(topLeft.position.x, bottomRight.position.x, worldPoint.x);
        float zPos = Mathf.InverseLerp(bottomRight.position.z, topLeft.position.z, worldPoint.z);
        //If the xPos or zPos are outside the bounds of the keyboard (smaller than zero or greater than one), return null - otherwise return the normalized point.
        return (xPos < 0 || xPos > 1 || zPos < 0 || zPos > 1) ? null : new Vector3(xPos, 0, zPos);
    }
    /// <summary>
    /// Clears the connected heatmaps and recalculates the span of the bounds in keyboard space. This should be called when the keyboard is resized in order to maintain consistent data capture.
    /// </summary>
    public void RefreshCoordinateSystem()
    {
        RecalculateKeyboardSpan();
        ClearConnectedHeatmaps();
    }
    /// <summary>
    /// Recalculates the bounds of coordinates in keyboard space. This needs to be called anytime the keyboard is resized, or the relative distance between the anchors is changed.
    /// </summary>
    private void RecalculateKeyboardSpan()
    {
        _keyboardIdlePositionSpan = new Vector2(_bottomRightIdleKeyPosition.position.x - _topLeftIdleKeyPosition.position.x, _topLeftIdleKeyPosition.position.z - _bottomRightIdleKeyPosition.position.z);
        _keyboardTriggerPositionSpan = new Vector2(_bottomRightEventTriggerPosition.position.x - _topLeftEventTriggerPosition.position.x, _topLeftEventTriggerPosition.position.z - _bottomRightEventTriggerPosition.position.z);
    }
    /// <summary>
    /// Clears the heatmaps that are connected to the coordinate system. This needs to be called anytime the keyboard is resized, if keyboard and not normalized space is being used.
    /// </summary>
    private void ClearConnectedHeatmaps()
    {
        _eventTriggerPositionHeatmap.ClearHeatmap(_bottomRightEventTriggerPosition.position.x - _topLeftEventTriggerPosition.position.x, _topLeftEventTriggerPosition.position.z - _bottomRightEventTriggerPosition.position.z);
        _idleKeyPositionHeatmap.ClearHeatmap(_bottomRightIdleKeyPosition.position.x - _topLeftIdleKeyPosition.position.x, _topLeftIdleKeyPosition.position.z - _bottomRightIdleKeyPosition.position.z);
    }
    /// <summary>
    /// Event response to the coordinate type being changed. This triggers a refresh of the connected heatmaps and recalculates keyboard space data.
    /// </summary>
    /// <param name="type"></param>
    private void OnCoordinateTypeUpdated(CoordinateType type)
    {
        RefreshCoordinateSystem();
    }
    #endregion
}
