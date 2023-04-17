using System.Collections;
using System.Collections.Generic;
using System;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

public class EyeTracking : MonoBehaviour
{

    public Key[] keys;                                              // Array of all keys to be tracked
    public Key leftShift;                                           // Leftshift key reference
    public Key rightShift;                                          // Rightshift key reference
    public Key capslock;                                            // Capslock key reference
    public Key spacebar;                                            // Spacebar key reference
    public GameObject[] spacebarPositions = new GameObject[4];      // Additional spacebar positions (necessary to account for the width of the spacebar)
    public float radius = 0.5f;                                     // Radius around the eye gaze position within which keys will be active
    public Logger log;                                              // Reference to the Log
    public char prev;                                               // The last key the user was looking at
    bool isNull = true;                                             // Tracks whether or not the eye gaze position is null
    bool isTracking = true;                                         // If eye tracking is active
    bool keyboardActive = true;                                     // If the keyboard is active
    bool shifted = false;                                           // If the shift keys are toggled on
    bool capslocked = false;                                        // If the capslock key is toggled on

    private void Start()
    {
        // Enables all keys upon startup
        enableAll();
    }

    private void FixedUpdate()
    {
        // Get the eye gaze position
        MixedRealityRaycastHit obj = CoreServices.InputSystem.GazeProvider.HitInfo;
        try {
            Key k = null;
            if(obj.collider != null)
            {
                k = obj.collider.GetComponent<Key>();
            } else
            {
                throw new NullReferenceException();
            }
            // Only the X and Z coordinates should be used to determine distance
            Vector2 pos = new Vector2(CoreServices.InputSystem.GazeProvider.HitPosition.x, CoreServices.InputSystem.GazeProvider.HitPosition.z);
            if (k != null)
            {
                // If the user is looking at a different key
                if (!k.getValue().Equals(prev) || isNull)
                {
                    log.write_gaze(k.getValue() + "");
                }
            } else {
                log.write_gaze("BACKPLATE");
            }

            isNull = false;

            // If the keyboard is active and eye tracking is active
            if(isTracking && keyboardActive)
            {
                foreach (Key key in keys)
                {
                    // If the key is within the radius, enable it, otherwise disable it
                    if(Vector2.Distance(new Vector2(key.transform.position.x, key.transform.position.z), pos) < radius)
                    {
                        key.setEnabled();
                    } 
                    else
                    {
                        key.setDisabled();
                    }
                }

                // Checks the additional spacebar positions to see if the spacebar should be enabled
                foreach (GameObject p in spacebarPositions)
                {
                    if(Vector2.Distance(new Vector2(p.transform.position.x, p.transform.position.z), pos) < radius)
                    {
                        spacebar.setEnabled();
                    }
                }

                // Special cases for toggle keys
                setToggleKey(rightShift, pos, shifted);
                setToggleKey(leftShift, pos, shifted);
                setToggleKey(capslock, pos, capslocked);
            }
            // Store the detected key as the new previous key
            if(k != null)
                prev = k.getValue();
        } catch (NullReferenceException e)
        {
            // Disable the keyboard if the user isn't looking at it
            if(isTracking && keyboardActive) disableAll();
            if(!isNull)
            {
                log.write_gaze("NULL");
                isNull = true;
            }
        }
    }

    /**
     * Enables all keys
     **/
    public void enableAll()
    {
        foreach(Key key in keys)
        {
            key.setEnabled();
        }
        rightShift.setEnabled();
        leftShift.setEnabled();
        capslock.setEnabled();
    }

    /**
     * Disables all keys
     **/
    private void disableAll()
    {
        foreach(Key key in keys)
        {
            key.setDisabled();
        }

        // Check special cases for toggle keys
        if (keyboardActive)
        {
            if(shifted)
            {
                rightShift.disableWithoutLight();
                leftShift.disableWithoutLight();
            }
            else
            {
                rightShift.setDisabled();
                leftShift.setDisabled();
            }
            if(capslocked)
            {
                capslock.disableWithoutLight();
            }
            else
            {
                capslock.setDisabled();
            }
        } 
        else
        {
            rightShift.setDisabled();
            leftShift.setDisabled();
            capslock.setDisabled();
        }
    }

    /**
     * Sets special conditions for toggle keys:
     * If the toggle key is within the radius: Enable it
     * If the toggle key is outside the radius and toggled on: Disable it, but keep it lit up
     * If the toggle key is outside the radius and toggled off: Disable it
     **/
    private void setToggleKey(Key k, Vector2 pos, bool toggled)
    {
        if (Vector2.Distance(new Vector2(k.transform.position.x, k.transform.position.z), pos) < radius)
        {
            k.setEnabled();
        }
        else if (toggled)
        {
            k.disableWithoutLight();
        }
        else
        {
            k.setDisabled();
        }
    }

    /**
     * Sets eye tracking to be enabled or disabled
     **/
    public void setEnabled(bool e)
    {
        isTracking = e;
        if (!isTracking) enableAll();
    }

    /**
     * Returns the position of the eye gaze
     **/
    public Vector3 getPosition()
    {
        if(isNull)
            return Vector3.zero;
        else
            return CoreServices.InputSystem.GazeProvider.HitPosition;
    }

    /**
     * Sets whether the shift keys are toggled on or off
     **/
    public void setShift(bool b)
    {
        shifted = b;
    }

    /**
     * Sets whether the capslock key is toggled on or off
     **/
    public void setCaps(bool b)
    {
        capslocked = b;
    }

    /**
     * Sets whether or not the keyboard is active
     **/
    public void setKeyboard(bool b)
    {
        keyboardActive = b;
        if (!keyboardActive) disableAll();
        else if (!isTracking) enableAll();
    }
}
