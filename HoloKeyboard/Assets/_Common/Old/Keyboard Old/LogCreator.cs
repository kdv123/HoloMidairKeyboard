using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using CoordinateType = CoordinateSystem.CoordinateType;
using CapturePosition = CoordinateSystem.CapturePosition;
using System.Runtime.InteropServices.WindowsRuntime;
using JetBrains.Annotations;
using System.Text;
using Unity.VisualScripting;
public class LogCreator : MonoBehaviour
{
    public static LogCreator Instance { get; private set; }
    private List<LogData> _sessionEntries = new List<LogData>();
    private void Awake()
    {
        Instance = this;
    }
    /// <summary>
    /// Creates a new LogData that is blank. It is reccommended to use this function as the base for creating new LogData from outside sources in order to maintain session entry list consistency.
    /// </summary>
    /// <returns>The newly created LogData</returns>
    public LogData CreateDefaultEntry()
    {
        LogData data = new LogData();
        _sessionEntries.Add(data);
        return data;
    }

    /// <summary>
    /// Creates a new LogData and adds a coordinate component to it
    /// </summary>
    /// <param name="inputPosition">Input position to capture</param>
    /// <returns>The newly created LogData</returns>
    public LogData CreateCoordinateEntry(CapturePosition mode, Vector2? inputPosition, string captureFinger)
    {
        LogData data = CreateDefaultEntry();
        CoordinateLog coordLog = new CoordinateLog(mode, inputPosition, captureFinger);
        data.AddComponent(coordLog);
        return data;
    }

}
/// <summary>
/// Base class used for building log components
/// </summary>
[Serializable]
public class LogComponent : ICloneable
{
    public LogComponent()
    {

    }

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}
[Serializable]
public class CoordinateLog : LogComponent
{
    /// <summary>
    /// What interaction space the coordinates were captured in
    /// </summary>
    public CoordinateType InteractionSpace { get; private set; }
    /// <summary>
    /// What trigger inititated the coordinate capture
    /// </summary>
    public CapturePosition CaptureMode { get; private set; }
    /// <summary>
    /// The coordinates captured by this log, or null if the log is not for capturing coordinates.
    /// </summary>
    public Vector2? InputCoordinates { get; private set; }
    /// <summary>
    /// Name of the finger that initiated the input 
    /// </summary>
    public string CaptureFinger { get; private set; }
    /// <summary>
    /// Creates a new CoordianteLog object. The InteractionSpace is automatically set to the current interaction spaced.
    /// </summary>
    /// <param name="mode">CapturePosition being used</param>
    /// <param name="inputCoordinates">Coordinates to capture</param>
    /// <param name="captureFinger">What finger initiated the capture</param>
    public CoordinateLog(CapturePosition mode, Vector2? inputCoordinates, string captureFinger) : base()
    {
        CaptureMode = mode;
        InteractionSpace = master.Instance.CoordinateType;
        InputCoordinates = inputCoordinates;
        CaptureFinger = captureFinger;
    }

    /// <summary>
    /// Default logging behavior displays the coordinate space and coordiante position, as well as the base component information. Fallback is base ToString();
    /// </summary>
    public override string ToString()
    {
        try
        {
            return $"Coordinate space: {InteractionSpace.ToString()} Capture mode: {CaptureMode.ToString()}Input position: {InputCoordinates.ToString()} Capture finger: {CaptureFinger}";
        }
        //Default case
        catch (Exception e)
        {
            return base.ToString();
        }
    }
}
[Serializable]
public class EyeTrackingLog : LogComponent
{
    /// <summary>
    /// The eye gaze coordinates captured - this is null if the eye gaze didn't receive data at the time the log was created.
    /// </summary>
    public Vector2? GazeCoordinates { get; private set; }
    /// <summary>
    /// Creates a new Eye tracking log.
    /// </summary>
    /// <param name="gazeCoordinates">Coordinates of the eye gaze at the time of capture</param>
    public EyeTrackingLog( Vector2? gazeCoordinates) : base()
    {
        GazeCoordinates = gazeCoordinates;
    }

    /// <summary>
    /// Default logging behavior displays the captured coordinates from the gaze, or a message if the capture data is null. Fallback is base ToString();
    /// </summary>
    public override string ToString()
    {
        try
        {
            return GazeCoordinates == null ? "Eye gaze data is null" : $"Gaze coordinates: {GazeCoordinates.GetValueOrDefault().ToString()}";
        }
        catch(Exception e)
        {
            return base.ToString();
        }
    }
}
[Serializable]
public class LogData : IComparable<LogData>
{
    /// <summary>
    /// ID representing the log. This is unique to this log
    /// </summary>
    public long ID { get; private set; }
    /// <summary>
    /// Exact time that this log occured
    /// </summary>
    public DateTime Time { get; private set; }
    
    /// <summary>
    /// List of all components contained in this log.
    /// </summary>
    private List<LogComponent> _components = new List<LogComponent>();

    /// <summary>
    /// Gets a copy of the components stored on this log, using a deep copy from MemberwiseClone.
    /// </summary>
    /// <returns>A copy of the components on this log</returns>
    public List<LogComponent> Components()
    {
        List<LogComponent> temp = new List<LogComponent>();
        for(int i = 0; i < _components.Count; i++)
        {
            temp.Add((LogComponent)_components[i].Clone());
        }
        return temp;
    }

    /// <summary>
    /// Adds a component to this log
    /// </summary>
    /// <param name="c">Component to add</param>
    /// <exception cref="ArgumentException">If <paramref name="c"/> already exists on this log.</exception>
    public void AddComponent(LogComponent c)
    {
        for(int i = 0; i < _components.Count; i++)
        {
            if (_components[i] == c)
            {
                throw new ArgumentException("Component " + c.ToString() + "already exists on this log.");
            }
        }
        _components.Add(c);
    }

    /// <summary>
    /// Creates a new LogData object, where the ID is the next available ID and the time is set to the current computer time.
    /// </summary>
    public LogData()
    {
        ID = _currentID;
        Time = DateTime.Now;
    }


    /// <summary>
    /// Member to keep track of the next ID to distribute. The ID the next created log will recieve is one larger than this number.
    /// </summary>
    private static long _currentID = -1;
    /// <summary>
    /// Gets the next ID for a log. This increments the currentID by one, and uses that number.
    /// </summary>
    /// <returns>The ID for the next entry</returns>
    private static long NextID() => ++_currentID;

    /// <summary>
    /// Compares two LogDatas. ID comparison is first used to check if they are equal, then the time of the log occuring is compared.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public int CompareTo(LogData other)
    {
        //First check if they are the same object by comparing ID's
        if (other.ID == this.ID) return 0;
        //Explicit check for equal datettimes to prevent to equal times from returning -1.
        if (this.Time == other.Time) return 0;
        //Behavior after is to compare their DateTimes.
        if (this.RegisteredBefore(other)) return -1;
        if (this.RegisteredAfter(other)) return 1;
        throw new InvalidOperationException("Failed to compare given logs - is there an issue with your DateTimes?");
    }

    /// <summary>
    /// Default logging behavior displays the log ID and time, as well as component count. Fallback is default ToString();
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        try
        {
            return $"ID: {ID.ToString()} Time: {Time.ToString()} Component length: {_components.Count}";
        }
        //Default case
        catch (Exception e)
        {
            return base.ToString();
        }
    }
}
public static class LogDataExtensions{
    /// <summary>
    /// If this log was registered before the given log. Inclusive to equals.
    /// </summary>
    /// <param name="data">Log to compare to</param>
    public static bool RegisteredBefore(this LogData self, LogData data)
    {
        return self.Time <= data.Time;
    }
    /// <summary>
    /// If this log was registered after the given log. Inclusive to equals.
    /// </summary>
    /// <param name="data">Log to compare to</param>
    public static bool RegisteredAfter(this LogData self, LogData data)
    {
        return self.Time >= data.Time;
    }
}