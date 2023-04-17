using System.Collections;
using System.Collections.Generic;
using System;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

public class EyeTracking : MonoBehaviour
{

    public Key[] keys;
    public Key leftShift;
    public Key rightShift;
    public Key capslock;
    public Key spacebar;
    public GameObject[] spacebarPositions = new GameObject[4];
    public float radius = 0.5f;
    public Logger log;
    public char prev;
    bool isNull = true;
    bool isTracking = true;
    bool keyboardActive = true;
    bool shifted = false;
    bool capslocked = false;

    private void Start()
    {
        enableAll();
    }

    private void FixedUpdate()
    {
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
            //Vector3 pos = CoreServices.InputSystem.GazeProvider.HitPosition;
            Vector2 pos = new Vector2(CoreServices.InputSystem.GazeProvider.HitPosition.x, CoreServices.InputSystem.GazeProvider.HitPosition.z);
            if (k != null)
            {
                if (!k.getValue().Equals(prev) || isNull)
                {
                    log.write_gaze(k.getValue() + "");
                }
            } else {
                log.write_gaze("BACKPLATE");
            }

            isNull = false;

            if(isTracking && keyboardActive)
            {
                foreach (Key key in keys)
                {
                    //if(Vector3.Distance(key.transform.position, pos) < radius)
                    if(Vector2.Distance(new Vector2(key.transform.position.x, key.transform.position.z), pos) < radius)
                    {
                        key.setEnabled();
                    } 
                    else
                    {
                        key.setDisabled();
                    }
                }

                foreach (GameObject p in spacebarPositions)
                {
                    //if(Vector3.Distance(p.transform.position, pos) < radius)
                    if(Vector2.Distance(new Vector2(p.transform.position.x, p.transform.position.z), pos) < radius)
                    {
                        spacebar.setEnabled();
                    }
                }

                setToggleKey(rightShift, pos, shifted);
                setToggleKey(leftShift, pos, shifted);
                setToggleKey(capslock, pos, capslocked);
            }
            if(k != null)
                prev = k.getValue();
        } catch (NullReferenceException e)
        {
            if(isTracking && keyboardActive) disableAll();
            if(!isNull)
            {
                log.write_gaze("NULL");
                isNull = true;
            }
        }
    }

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

    private void disableAll()
    {
        foreach(Key key in keys)
        {
            key.setDisabled();
        }

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

    public void setEnabled(bool e)
    {
        isTracking = e;
        if (!isTracking) enableAll();
    }

    public Vector3 getPosition()
    {
        if(isNull)
            return Vector3.zero;
        else
            return CoreServices.InputSystem.GazeProvider.HitPosition;
    }

    public void setShift(bool b)
    {
        shifted = b;
    }

    public void setCaps(bool b)
    {
        capslocked = b;
    }

    public void setKeyboard(bool b)
    {
        keyboardActive = b;
        if (!keyboardActive) disableAll();
        else if (!isTracking) enableAll();
    }
}
