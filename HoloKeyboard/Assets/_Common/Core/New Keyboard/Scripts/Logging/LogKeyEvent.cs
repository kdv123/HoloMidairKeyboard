using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Log for any keyboard input
/// </summary>
[Serializable]
public class LogKeyEvent : LogEntryBase
{
    public string Key;

    public LogKeyEvent(string key, TimeSpan entry) : base(entry)
    {
        Key = key;
    }
}
