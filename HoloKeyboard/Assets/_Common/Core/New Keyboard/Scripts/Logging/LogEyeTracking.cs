using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogEyeTracking : LogEntryBase
{
    public float[] Position = {0.0f, 0.0f, 0.0f};
    public LogEyeTracking(Vector3 position, TimeSpan entry) : base(entry)
    {
        Position[0] = position.x;
        Position[1] = position.y;
        Position[2] = position.z;
    }
}
