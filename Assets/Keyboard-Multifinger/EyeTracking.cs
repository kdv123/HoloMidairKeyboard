using System.Collections;
using System.Collections.Generic;
using System;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

public class EyeTracking : MonoBehaviour
{

    public Key[] keys;
    public float radius = 0.5f;

    private void Start()
    {
        enableAll();
    }

    private void FixedUpdate()
    {
        MixedRealityRaycastHit obj = CoreServices.InputSystem.GazeProvider.HitInfo;
        try {
            Key k = obj.collider.GetComponent<Key>();
            if(k != null)
            {
                // Debug.Log(k.getValue());
                foreach(Key key in keys)
                {
                    if(Vector3.Distance(key.transform.position, obj.transform.position) < radius)
                    {
                        key.setEnabled();
                    } 
                    else
                    {
                        key.setDisabled();
                    }
                }
            }
        } catch (NullReferenceException e)
        {
            enableAll();   
        }
    }

    private void enableAll()
    {
        foreach(Key key in keys)
        {
            key.setEnabled();
        }
    }

    private void disableAll()
    {
        foreach(Key key in keys)
        {
            key.setDisabled();
        }
    }
}
