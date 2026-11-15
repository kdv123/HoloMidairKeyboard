using CSWebSockets.Structs;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LogPredictionPhaseCompleted : LogEntryBase
{
    public string Target;
    public string Actual;
    public int NumCompleted;
    public LogPredictionPhaseCompleted(string target, string actual, int numCompleted, TimeSpan entry) : base(entry)
    {
        Target = target;
        Actual = actual;
        NumCompleted = numCompleted;
    }
}
