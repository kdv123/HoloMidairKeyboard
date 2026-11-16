using CSWebSockets.Structs;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Log entry for when the server provides a response to a prediction request
/// </summary>
[Serializable]
public class LogServerResponse : LogEntryBase
{
    public PredictionSet Response;
    public double? ElapsedMs;
    public PredictionResponse ServerResponse;
    public LogServerResponse(PredictionSet predictions, TimeSpan entryTime,
        double? elapsedMs = null, PredictionResponse serverResponse = null) : base(entryTime)
    {
        Response = predictions;
        ElapsedMs = elapsedMs;
        ServerResponse = serverResponse;
    }
}
