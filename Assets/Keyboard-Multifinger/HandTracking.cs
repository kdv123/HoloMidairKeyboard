using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Input;

/**
 * Author      : Cecilia Schmitz
 * Email       : cmschmit@mtu.edu
 * Description : Tracks the fingertips on both hands and adds colliders to them.
 *             : NOTE: In order to use the index-finger keyboard, this script must NOT be active
 */
public class HandTracking : MonoBehaviour
{

    public GameObject fingerCollider; // Fingertip collider prefab

    // Objects to store fingertip collider prefabs
    GameObject right_thumb;
    GameObject right_index;
    GameObject right_middle;
    GameObject right_ring;
    GameObject right_pinky;

    GameObject left_thumb;
    GameObject left_index;
    GameObject left_middle;
    GameObject left_ring;
    GameObject left_pinky;

    MixedRealityPose pose; // Pose to temporarily store fetched hand joints

    void Start()
    {
        // Instantiate all finger collider prefabs
        right_thumb = Instantiate(fingerCollider, this.transform);
        right_index = Instantiate(fingerCollider, this.transform);
        right_middle = Instantiate(fingerCollider, this.transform);
        right_ring = Instantiate(fingerCollider, this.transform);
        right_pinky = Instantiate(fingerCollider, this.transform);

        left_thumb = Instantiate(fingerCollider, this.transform);
        left_index = Instantiate(fingerCollider, this.transform);
        left_middle = Instantiate(fingerCollider, this.transform);
        left_ring = Instantiate(fingerCollider, this.transform);
        left_pinky = Instantiate(fingerCollider, this.transform);
    }

    void FixedUpdate()
    {
        // For each fingertip joint: Check if it exists (i.e. the Hololens can see the joint), and if so, attach the associated collider
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.ThumbTip, Handedness.Right, out pose))
        {
            right_thumb.GetComponent<Renderer>().enabled = true;
            right_thumb.GetComponent<Rigidbody>().MovePosition(pose.Position);
        } else
        {
            right_thumb.GetComponent<Renderer>().enabled = false;
        }

        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexTip, Handedness.Right, out pose))
        {
            right_index.GetComponent<Renderer>().enabled = true;
            right_index.GetComponent<Rigidbody>().MovePosition(pose.Position);
        }
        else
        {
            right_index.GetComponent<Renderer>().enabled = false;
        }

        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.MiddleTip, Handedness.Right, out pose))
        {
            right_middle.GetComponent<Renderer>().enabled = true;
            right_middle.GetComponent<Rigidbody>().MovePosition(pose.Position);
        }
        else
        {
            right_middle.GetComponent<Renderer>().enabled = false;
        }

        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.RingTip, Handedness.Right, out pose))
        {
            right_ring.GetComponent<Renderer>().enabled = true;
            right_ring.GetComponent<Rigidbody>().MovePosition(pose.Position);
        }
        else
        {
            right_ring.GetComponent<Renderer>().enabled = false;
        }

        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.PinkyTip, Handedness.Right, out pose))
        {
            right_pinky.GetComponent<Renderer>().enabled = true;
            right_pinky.GetComponent<Rigidbody>().MovePosition(pose.Position);
        }
        else
        {
            right_pinky.GetComponent<Renderer>().enabled = false;
        }


        if(HandJointUtils.TryGetJointPose(TrackedHandJoint.ThumbTip, Handedness.Left, out pose))
        {
            left_thumb.GetComponent<Renderer>().enabled = true;
            left_thumb.GetComponent<Rigidbody>().MovePosition(pose.Position);
        } else
        {
            left_thumb.GetComponent<Renderer>().enabled = false;
        }

        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexTip, Handedness.Left, out pose))
        {
            left_index.GetComponent<Renderer>().enabled = true;
            left_index.GetComponent<Rigidbody>().MovePosition(pose.Position);
        }
        else
        {
            left_index.GetComponent<Renderer>().enabled = false;
        }

        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.MiddleTip, Handedness.Left, out pose))
        {
            left_middle.GetComponent<Renderer>().enabled = true;
            left_middle.GetComponent<Rigidbody>().MovePosition(pose.Position);
        }
        else
        {
            left_middle.GetComponent<Renderer>().enabled = false;
        }

        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.RingTip, Handedness.Left, out pose))
        {
            left_ring.GetComponent<Renderer>().enabled = true;
            left_ring.GetComponent<Rigidbody>().MovePosition(pose.Position);
        }
        else
        {
            left_ring.GetComponent<Renderer>().enabled = false;
        }

        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.PinkyTip, Handedness.Left, out pose))
        {
            left_pinky.GetComponent<Renderer>().enabled = true;
            left_pinky.GetComponent<Rigidbody>().MovePosition(pose.Position);
        }
        else
        {
            left_pinky.GetComponent<Renderer>().enabled = false;
        }
    }
}
