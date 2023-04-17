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
    bool tenfingers = true;

    void Start()
    {
        // Instantiate all finger collider prefabs
        right_thumb = Instantiate(fingerCollider, this.transform);
        right_thumb.name = "RIGHT_THUMB";
        right_index = Instantiate(fingerCollider, this.transform);
        right_index.name = "RIGHT_INDEX";
        right_middle = Instantiate(fingerCollider, this.transform);
        right_middle.name = "RIGHT_MIDDLE";
        right_ring = Instantiate(fingerCollider, this.transform);
        right_ring.name = "RIGHT_RING";
        right_pinky = Instantiate(fingerCollider, this.transform);
        right_pinky.name = "RIGHT_PINKY";

        left_thumb = Instantiate(fingerCollider, this.transform);
        left_thumb.name = "LEFT_THUMB";
        left_index = Instantiate(fingerCollider, this.transform);
        left_index.name = "LEFT_INDEX";
        left_middle = Instantiate(fingerCollider, this.transform);
        left_middle.name = "LEFT_MIDDLE";
        left_ring = Instantiate(fingerCollider, this.transform);
        left_ring.name = "LEFT_RING";
        left_pinky = Instantiate(fingerCollider, this.transform);
        left_pinky.name = "LEFT_PINKY";
    }

    public void trackTwoFingers()
    {
        // Destroy finger colliders for non-index fingers
        Destroy(right_thumb);
        Destroy(right_middle);
        Destroy(right_ring);
        Destroy(right_pinky);

        Destroy(left_thumb);
        Destroy(left_middle);
        Destroy(left_ring);
        Destroy(left_pinky);

        tenfingers = false;
    }

    public void trackTenFingers()
    {
        // Instantiate finger collider prefabs
        right_thumb = Instantiate(fingerCollider, this.transform);
        right_thumb.name = "RIGHT_THUMB";
        right_middle = Instantiate(fingerCollider, this.transform);
        right_middle.name = "RIGHT_MIDDLE";
        right_ring = Instantiate(fingerCollider, this.transform);
        right_ring.name = "RIGHT_RING";
        right_pinky = Instantiate(fingerCollider, this.transform);
        right_pinky.name = "RIGHT_PINKY";

        left_thumb = Instantiate(fingerCollider, this.transform);
        left_thumb.name = "LEFT_THUMB";
        left_middle = Instantiate(fingerCollider, this.transform);
        left_middle.name = "LEFT_MIDDLE";
        left_ring = Instantiate(fingerCollider, this.transform);
        left_ring.name = "LEFT_RING";
        left_pinky = Instantiate(fingerCollider, this.transform);
        left_pinky.name = "LEFT_PINKY";

        tenfingers = true;
    }

    void FixedUpdate()
    {
        // For each fingertip joint: Check if it exists (i.e. the Hololens can see the joint), and if so, attach the associated collider
        if (tenfingers && HandJointUtils.TryGetJointPose(TrackedHandJoint.ThumbTip, Handedness.Right, out pose))
        {
            right_thumb.GetComponent<Renderer>().enabled = true;
            right_thumb.GetComponent<Rigidbody>().MovePosition(pose.Position);
        } else if (tenfingers)
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

        if (tenfingers && HandJointUtils.TryGetJointPose(TrackedHandJoint.MiddleTip, Handedness.Right, out pose))
        {
            right_middle.GetComponent<Renderer>().enabled = true;
            right_middle.GetComponent<Rigidbody>().MovePosition(pose.Position);
        }
        else if(tenfingers)
        {
            right_middle.GetComponent<Renderer>().enabled = false;
        }

        if (tenfingers && HandJointUtils.TryGetJointPose(TrackedHandJoint.RingTip, Handedness.Right, out pose))
        {
            right_ring.GetComponent<Renderer>().enabled = true;
            right_ring.GetComponent<Rigidbody>().MovePosition(pose.Position);
        }
        else if (tenfingers)
        {
            right_ring.GetComponent<Renderer>().enabled = false;
        }

        if (tenfingers && HandJointUtils.TryGetJointPose(TrackedHandJoint.PinkyTip, Handedness.Right, out pose))
        {
            right_pinky.GetComponent<Renderer>().enabled = true;
            right_pinky.GetComponent<Rigidbody>().MovePosition(pose.Position);
        }
        else if (tenfingers)
        {
            right_pinky.GetComponent<Renderer>().enabled = false;
        }


        if(tenfingers && HandJointUtils.TryGetJointPose(TrackedHandJoint.ThumbTip, Handedness.Left, out pose))
        {
            left_thumb.GetComponent<Renderer>().enabled = true;
            left_thumb.GetComponent<Rigidbody>().MovePosition(pose.Position);
        }
        else if (tenfingers)
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

        if (tenfingers && HandJointUtils.TryGetJointPose(TrackedHandJoint.MiddleTip, Handedness.Left, out pose))
        {
            left_middle.GetComponent<Renderer>().enabled = true;
            left_middle.GetComponent<Rigidbody>().MovePosition(pose.Position);
        }
        else if (tenfingers)
        {
            left_middle.GetComponent<Renderer>().enabled = false;
        }

        if (tenfingers && HandJointUtils.TryGetJointPose(TrackedHandJoint.RingTip, Handedness.Left, out pose))
        {
            left_ring.GetComponent<Renderer>().enabled = true;
            left_ring.GetComponent<Rigidbody>().MovePosition(pose.Position);
        }
        else if (tenfingers)
        {
            left_ring.GetComponent<Renderer>().enabled = false;
        }

        if (tenfingers && HandJointUtils.TryGetJointPose(TrackedHandJoint.PinkyTip, Handedness.Left, out pose))
        {
            left_pinky.GetComponent<Renderer>().enabled = true;
            left_pinky.GetComponent<Rigidbody>().MovePosition(pose.Position);
        }
        else if (tenfingers)
        {
            left_pinky.GetComponent<Renderer>().enabled = false;
        }
    }
}
