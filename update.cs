﻿using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using UnityEngine;

public class update : MonoBehaviour
{
    
    private Vector3 curPos;
    private Vector3 lastPos;
    
    public GameObject keyboard;
    public GameObject buttonCollectionReal;
    public GameObject buttonCollectionFake;
    
    public Collider collider;
    void Update()
    {
        
        curPos = keyboard.gameObject.transform.position;
        
        if(curPos == lastPos)
        {
            //Debug.Log("Not moving");
            buttonCollectionFake.SetActive(false);
            buttonCollectionReal.SetActive(true);
            collider.enabled = false;
        }
        else
        {
            //Debug.Log("moving");
            buttonCollectionFake.SetActive(true);
            buttonCollectionReal.SetActive(false);
            collider.enabled = true;
        }
        
        lastPos = curPos;
    }
    
}
