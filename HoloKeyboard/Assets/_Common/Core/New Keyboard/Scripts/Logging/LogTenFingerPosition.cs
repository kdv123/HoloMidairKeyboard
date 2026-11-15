using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public class LogTenFingerPosition : LogEntryBase
{
    public float[] LeftThumbPosition = { 0.0f, 0.0f, 0.0f };
    public float[] LeftIndexPosition = { 0.0f, 0.0f, 0.0f };
    public float[] LeftMiddlePosition = { 0.0f, 0.0f, 0.0f };
    public float[] LeftRingPosition = { 0.0f, 0.0f, 0.0f };
    public float[] LeftPinkyPosition = { 0.0f, 0.0f, 0.0f };

    public float[] RightThumbPosition = { 0.0f, 0.0f, 0.0f };
    public float[] RightIndexPosition = { 0.0f, 0.0f, 0.0f };
    public float[] RightMiddlePosition = { 0.0f, 0.0f, 0.0f };
    public float[] RightRingPosition = { 0.0f, 0.0f, 0.0f };
    public float[] RightPinkyPosition = { 0.0f, 0.0f, 0.0f };
    public LogTenFingerPosition(Vector3 LThumb, Vector3 LIndex, Vector3 LMiddle, Vector3 LRing, Vector3 LPinky, Vector3 RThumb, Vector3 RIndex, Vector3 RMiddle, Vector3 RRing, Vector3 RPinky, TimeSpan entry) : base(entry)
    {
        LeftThumbPosition[0] = LThumb.x;
        LeftThumbPosition[1] = LThumb.y;
        LeftThumbPosition[2] = LThumb.z;

        LeftIndexPosition[0] = LIndex.x;
        LeftIndexPosition[1] = LIndex.y;
        LeftIndexPosition[2] = LIndex.z;

        LeftMiddlePosition[0] = LMiddle.x;
        LeftMiddlePosition[1] = LMiddle.y;
        LeftMiddlePosition[2] = LMiddle.z;

        LeftRingPosition[0] = LRing.x;
        LeftRingPosition[1] = LRing.y;
        LeftRingPosition[2] = LRing.z;

        LeftPinkyPosition[0] = LPinky.x;
        LeftPinkyPosition[1] = LPinky.y;
        LeftPinkyPosition[2] = LPinky.z;



        RightThumbPosition[0] = RThumb.x;
        RightThumbPosition[1] = RThumb.y;
        RightThumbPosition[2] = RThumb.z;

        RightIndexPosition[0] = RIndex.x;
        RightIndexPosition[1] = RIndex.y;
        RightIndexPosition[2] = RIndex.z;

        RightMiddlePosition[0] = RMiddle.x;
        RightMiddlePosition[1] = RMiddle.y;
        RightMiddlePosition[2] = RMiddle.z;

        RightRingPosition[0] = RRing.x;
        RightRingPosition[1] = RRing.y;
        RightRingPosition[2] = RRing.z;

        RightPinkyPosition[0] = RPinky.x;
        RightPinkyPosition[1] = RPinky.y;
        RightPinkyPosition[2] = RPinky.z;
    }
}
