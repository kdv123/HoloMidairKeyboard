using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CSWebSockets.Structs;

/// <summary>
/// Log entry for when a request is sent to the server for a prediction
/// </summary>
[Serializable]
public class LogServerRequest : LogEntryBase
{
    public string Message;
    public PredictionRequest Request;
    public LogServerRequest(PredictionRequest request, TimeSpan entry) : base(entry)
    {
        Message = request.left_context;
        Request = request;
    }
}
