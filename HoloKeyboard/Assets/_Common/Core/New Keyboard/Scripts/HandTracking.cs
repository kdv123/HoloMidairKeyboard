using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Input;
using TMPro;
using System;
using UnityEngine.XR.OpenXR.Input;

public class HandTracking : MonoBehaviour
{
    /// <summary>
    /// What state the hand tracking is currently in
    /// </summary>
    public enum HandTrackingMode
    {
        IndexFinger,
        TenFinger
    }
    [SerializeField] private HandTrackingMode _handTrackingMode;
    /// <summary>
    /// If the hand tracking is currently using ten fingers
    /// </summary>
    public bool IsTenFingers => _handTrackingMode == HandTrackingMode.TenFinger;
    /// <summary>
    /// If the hand tracking is currently using only the index fingers.
    /// </summary>
    public bool IsIndexFingers => _handTrackingMode == HandTrackingMode.IndexFinger;

    /// <summary>
    /// FIngertip collider prefab
    /// </summary>
    [SerializeField] private GameObject _fingerCollider;

    
    /// <summary>
    /// Mixed reality pose we are recieving the data from
    /// </summary>
    private MixedRealityPose _pose;
    /// <summary>
    /// Data storing the information about all of the hand tracking objects
    /// </summary>
    private HandTrackingData _data;

    void Start()
    {
        _data = new HandTrackingData();
        InitHandTracking();
        SetTrackingMode(_handTrackingMode);
    }

    void FixedUpdate()
    {
        UpdateTrackingData();
    }
    /// <summary>
    /// Enables/Disables certain finger colliders depending on the active mode.
    /// </summary>
    /// <param name="mode">What mode the hand tracking should use</param>
    public void SetTrackingMode(HandTrackingMode mode)
    {
        switch (mode)
        {
            case HandTrackingMode.IndexFinger:
                _data.Right_Thumb.SetActive(false);
                _data.Right_Middle.SetActive(false);
                _data.Right_Ring.SetActive(false);
                _data.Right_Pinky.SetActive(false);

                _data.Left_Thumb.SetActive(false);
                _data.Left_Middle.SetActive(false);
                _data.Left_Ring.SetActive(false);
                _data.Left_Pinky.SetActive(false);
                break;
            case HandTrackingMode.TenFinger:
                _data.Right_Thumb.SetActive(true);
                _data.Right_Middle.SetActive(true);
                _data.Right_Ring.SetActive(true);
                _data.Right_Pinky.SetActive(true);

                _data.Left_Thumb.SetActive(true);
                _data.Left_Middle.SetActive(true);
                _data.Left_Ring.SetActive(true);
                _data.Left_Pinky.SetActive(true);
                break;
        }
        _handTrackingMode = mode;
    }
    /// <summary>
    /// Setup function that creates the colliders for each finger and assigns them their proper names.
    /// </summary>
    private void InitHandTracking()
    {
        _data.Right_Thumb = Instantiate(_fingerCollider, this.transform);
        _data.Right_Thumb.name = "RIGHT_THUMB";
        _data.Right_Thumb_Rigidbody = _data.Right_Thumb.GetComponent<Rigidbody>();
        _data.Right_Thumb_Renderer = _data.Right_Thumb.GetComponent<Renderer>();

        _data.Right_Index = Instantiate(_fingerCollider, this.transform);
        _data.Right_Index.name = "RIGHT_INDEX";
        _data.Right_Index_Rigidbody = _data.Right_Index.GetComponent<Rigidbody>();
        _data.Right_Index_Renderer = _data.Right_Index.GetComponent<Renderer>();

        _data.Right_Middle = Instantiate(_fingerCollider, this.transform);
        _data.Right_Middle.name = "RIGHT_MIDDLE";
        _data.Right_Middle_Rigidbody = _data.Right_Middle.GetComponent<Rigidbody>();
        _data.Right_Middle_Renderer = _data.Right_Middle.GetComponent<Renderer>();

        _data.Right_Ring = Instantiate(_fingerCollider, this.transform);
        _data.Right_Ring.name = "RIGHT_RING";
        _data.Right_Ring_Rigidbody = _data.Right_Ring.GetComponent<Rigidbody>();
        _data.Right_Ring_Renderer = _data.Right_Ring.GetComponent<Renderer>();

        _data.Right_Pinky = Instantiate(_fingerCollider, this.transform);
        _data.Right_Pinky.name = "RIGHT_PINKY";
        _data.Right_Pinky_Rigidbody = _data.Right_Pinky.GetComponent<Rigidbody>();
        _data.Right_Pinky_Renderer = _data.Right_Pinky.GetComponent<Renderer>();


        _data.Left_Thumb = Instantiate(_fingerCollider, this.transform);
        _data.Left_Thumb.name = "LEFT_THUMB";
        _data.Left_Thumb_Rigidbody = _data.Left_Thumb.GetComponent<Rigidbody>();
        _data.Left_Thumb_Renderer = _data.Left_Thumb.GetComponent<Renderer>();

        _data.Left_Index = Instantiate(_fingerCollider, this.transform);
        _data.Left_Index.name = "LEFT_INDEX";
        _data.Left_Index_Rigidbody = _data.Left_Index.GetComponent<Rigidbody>();
        _data.Left_Index_Renderer = _data.Left_Index.GetComponent<Renderer>();

        _data.Left_Middle = Instantiate(_fingerCollider, this.transform);
        _data.Left_Middle.name = "LEFT_MIDDLE";
        _data.Left_Middle_Rigidbody = _data.Left_Middle.GetComponent<Rigidbody>();
        _data.Left_Middle_Renderer = _data.Left_Middle.GetComponent<Renderer>();

        _data.Left_Ring = Instantiate(_fingerCollider, this.transform);
        _data.Left_Ring.name = "LEFT_RING";
        _data.Left_Ring_Rigidbody = _data.Left_Ring.GetComponent<Rigidbody>();
        _data.Left_Ring_Renderer = _data.Left_Ring.GetComponent<Renderer>();

        _data.Left_Pinky = Instantiate(_fingerCollider, this.transform);
        _data.Left_Pinky.name = "LEFT_PINKY";
        _data.Left_Pinky_Rigidbody = _data.Left_Pinky.GetComponent<Rigidbody>();
        _data.Left_Pinky_Renderer = _data.Left_Pinky.GetComponent<Renderer>();

    }

    /// <summary>
    /// Called from FixedUpdate to refresh the current hand pose data
    /// </summary>
    private void UpdateTrackingData()
    {
        //Update index fingers
        if(HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexTip, Handedness.Right, out _pose))
        {
            _data.Right_Index_Rigidbody.MovePosition(_pose.Position);
            _data.Right_Index_Renderer.enabled = true;
        } 
        else _data.Right_Index_Renderer.enabled = false;
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexTip, Handedness.Left, out _pose))
        {
            _data.Left_Index_Rigidbody.MovePosition(_pose.Position);
            _data.Left_Thumb_Renderer.enabled = true;
        } 
        else _data.Left_Thumb_Renderer.enabled = false;

        //Don't update any other previews if not in ten finger mode
        if (IsIndexFingers) return;

        //Right hand
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.ThumbTip, Handedness.Right, out _pose))
        {
            _data.Right_Thumb_Rigidbody.MovePosition(_pose.Position);
            _data.Right_Thumb_Renderer.enabled = true;
        }
        else _data.Right_Thumb_Renderer.enabled = false;
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.MiddleTip, Handedness.Right, out _pose))
        {
            _data.Right_Middle_Rigidbody.MovePosition(_pose.Position);
            _data.Right_Middle_Renderer.enabled = true;
        }
        else _data.Right_Middle_Renderer.enabled = false;
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.RingTip, Handedness.Right, out _pose))
        {
            _data.Right_Ring_Rigidbody.MovePosition(_pose.Position);
            _data.Right_Ring_Renderer.enabled = true;
        } 
        else _data.Right_Ring_Renderer.enabled = false;
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.PinkyTip, Handedness.Right, out _pose))
        {
            _data.Right_Pinky_Rigidbody.MovePosition(_pose.Position);
            _data.Right_Pinky_Renderer.enabled = true;
        } 
        else _data.Right_Pinky_Renderer.enabled = false;

        //Left hand
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.ThumbTip, Handedness.Left, out _pose))
        {
            _data.Left_Thumb_Rigidbody.MovePosition(_pose.Position);
            _data.Left_Thumb_Renderer.enabled = true;
        }
        else _data.Left_Thumb_Renderer.enabled = false;
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.MiddleTip, Handedness.Left, out _pose))
        {
            _data.Left_Middle_Rigidbody.MovePosition(_pose.Position);
            _data.Left_Middle_Renderer.enabled = true;
        }
        else _data.Left_Middle_Renderer.enabled = false;
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.RingTip, Handedness.Left, out _pose))
        {
            _data.Left_Ring_Rigidbody.MovePosition(_pose.Position);
            _data.Left_Ring_Renderer.enabled = true;
        }
        else _data.Left_Ring_Renderer.enabled = false;
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.PinkyTip, Handedness.Left, out _pose))
        {
            _data.Left_Pinky_Rigidbody.MovePosition(_pose.Position);
            _data.Left_Pinky_Renderer.enabled = true;
        }
        else _data.Left_Pinky_Renderer.enabled = false;
    }

    [Serializable]
    private class HandTrackingData
    {
        public GameObject Right_Thumb;
        public GameObject Right_Index;
        public GameObject Right_Middle;
        public GameObject Right_Ring;
        public GameObject Right_Pinky;

        public GameObject Left_Thumb;
        public GameObject Left_Index;
        public GameObject Left_Middle;
        public GameObject Left_Ring;
        public GameObject Left_Pinky;



        public Rigidbody Right_Thumb_Rigidbody; 
        public Rigidbody Right_Index_Rigidbody;
        public Rigidbody Right_Middle_Rigidbody;
        public Rigidbody Right_Ring_Rigidbody;
        public Rigidbody Right_Pinky_Rigidbody;

        public Rigidbody Left_Thumb_Rigidbody;
        public Rigidbody Left_Index_Rigidbody;
        public Rigidbody Left_Middle_Rigidbody;
        public Rigidbody Left_Ring_Rigidbody;
        public Rigidbody Left_Pinky_Rigidbody;



        public Renderer Right_Thumb_Renderer;
        public Renderer Right_Index_Renderer;
        public Renderer Right_Middle_Renderer;
        public Renderer Right_Ring_Renderer;
        public Renderer Right_Pinky_Renderer;

        public Renderer Left_Thumb_Renderer;
        public Renderer Left_Index_Renderer;
        public Renderer Left_Middle_Renderer;
        public Renderer Left_Ring_Renderer;
        public Renderer Left_Pinky_Renderer;
    }
}
