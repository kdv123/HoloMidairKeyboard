using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DebugErrors : MonoBehaviour
{
    public TextMeshPro _worldText;
    void OnEnable()
    {
        // Subscribe to the log event
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        // Always unsubscribe to prevent memory leaks
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        // Check if the received log is an error or exception
        if (type == LogType.Error || type == LogType.Exception)
        {
            _worldText.text += "\n";
            _worldText.text += logString;
        }
    }
}
