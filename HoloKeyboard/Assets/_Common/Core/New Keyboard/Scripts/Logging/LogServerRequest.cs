using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Log entry for when a request is sent to the server for a prediction
/// </summary>
[Serializable]
public class LogServerRequest : LogEntryBase
{
    public string Message;
    public LogServerRequest(string message, TimeSpan entry) : base(entry)
    {
        Message = message;
    }
}
