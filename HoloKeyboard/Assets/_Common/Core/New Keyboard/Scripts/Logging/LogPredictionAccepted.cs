using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CSWebSockets.Structs;
[Serializable]
public class LogPredictionAccepted : LogEntryBase
{
    public Prediction AcceptedPrediction;
    public LogPredictionAccepted(Prediction accepted, TimeSpan entry) : base(entry)
    {
        AcceptedPrediction = accepted;
    }
}
