using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Audio;
using UnityEngine;

public class TTSManager : MonoBehaviour
{
    static TTSManager instance;
    static TextToSpeech tts;

    private void Awake(){
        if (instance == null){
            instance = this;
            tts = GetComponent<TextToSpeech>();
        }else{
            Destroy(this);
        }
    }

    public static void Say(string text){
        ScreenLogger.Log("Saying \"" + text + "\"\n");
        tts.StartSpeaking(text);
    }

    public static void Stop(){
        tts.StopSpeaking();
    }
}
