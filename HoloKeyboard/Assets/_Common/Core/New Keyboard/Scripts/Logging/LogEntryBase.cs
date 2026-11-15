using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System;

[Serializable]
public class LogEntryBase
{
    public TimeSpan EntryTime;
    public LogEntryBase(TimeSpan entryTime)
    {
        EntryTime = entryTime;
    }
}
