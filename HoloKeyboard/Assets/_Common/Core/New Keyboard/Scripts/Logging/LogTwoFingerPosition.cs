using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public class LogTwoFingerPosition : LogEntryBase
{
    public float[] LeftIndexPosition = { 0.0f, 0.0f, 0.0f };
    public float[] RightIndexPosition = { 0.0f, 0.0f, 0.0f };
    public LogTwoFingerPosition(Vector3 L, Vector3 R, TimeSpan entry) : base(entry)
    {
        LeftIndexPosition[0] = L.x;
        LeftIndexPosition[1] = L.y;
        LeftIndexPosition[2] = L.z;

        RightIndexPosition[0] = R.x;
        RightIndexPosition[1] = R.y;
        RightIndexPosition[2] = R.z;
    }
}
