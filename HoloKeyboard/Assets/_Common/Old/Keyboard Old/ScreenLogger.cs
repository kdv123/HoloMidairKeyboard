using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScreenLogger : MonoBehaviour{
    static ScreenLogger instance;
    static TextMeshPro tmp;
    
    private void Awake(){
        if (instance == null){
            instance = this;
            tmp = GetComponent<TextMeshPro>();
        }else{
            Destroy(this);
        }
    }

    public static void Log(string message){
        tmp.text += message;
    }
}
