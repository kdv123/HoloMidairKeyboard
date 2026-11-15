using CSWebSockets.Structs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Newtonsoft.Json;

public class LogManager : MonoBehaviour
{
    private string _path;
    public static LogManager Instance { get; private set; }
    private Stopwatch _stopwatch;
    private JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.All,
        Formatting = Formatting.Indented
    };
    private void Awake()
    {
        Instance = this;
        
        
    }
    /// <summary>
    /// Creates the log for a participant. Should be called after ID submission
    /// </summary>
    /// <param name="ID"></param>
    public void OpenLog(string ID)
    {
        _stopwatch = Stopwatch.StartNew();
        _path = Path.Combine(
            Application.persistentDataPath,
            DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".json"
        );
        if (!File.Exists(_path))
        {
            using (File.Create(_path)) { }
        }
        Debug.Log("Log file being stored at " + _path);
        File.AppendAllText(_path, "Participant: " + ID);
    }
    public void Write(LogEntryBase entry)
    {

        string json = JsonConvert.SerializeObject(entry, Formatting.Indented, _jsonSettings);
        File.AppendAllText(_path, json);

    }
    public void CreateProcedurePhaseSubmittedLog(string target, string actual, int numberCompleted)
    {
        if (_stopwatch == null) return;
        LogPredictionPhaseCompleted entry = new LogPredictionPhaseCompleted(target, actual, numberCompleted, _stopwatch.Elapsed);
        Write(entry);
    }
    public void CreateServerResponseLog(PredictionSet predictions)
    {
        if (_stopwatch == null) return;
        LogServerResponse entry = new LogServerResponse(predictions, _stopwatch.Elapsed);
        Write(entry);
    }
    public void CreateServerRequestLog(string message)
    {
        if (_stopwatch == null) return;
        LogServerRequest entry = new LogServerRequest(message, _stopwatch.Elapsed);
        Write(entry);
    }
    public void CreateKeyEntryLog(string key)
    {
        if (_stopwatch == null) return;
        LogKeyEvent entry = new LogKeyEvent(key, _stopwatch.Elapsed);
        Write(entry);
    }
    public void CreatePredictionAcceptedLog(Prediction accepted)
    {
        if (_stopwatch == null) return;
        LogPredictionAccepted entry = new LogPredictionAccepted(accepted, _stopwatch.Elapsed);
        Write(entry);
    }
    public void CreateFingerPositionLog(Vector3 L, Vector3 R)
    {
        if (_stopwatch == null) return;
        LogTwoFingerPosition entry = new LogTwoFingerPosition(L, R, _stopwatch.Elapsed);
        Write(entry);
    }
    public void CreateFingerPositionLog(Vector3 LThumb, Vector3 LIndex, Vector3 LMiddle, Vector3 LRing, Vector3 LPinky, Vector3 RThumb, Vector3 RIndex, Vector3 RMiddle, Vector3 RRing, Vector3 RPinky)
    {
        if (_stopwatch == null) return;
        LogTenFingerPosition entry = new LogTenFingerPosition(LThumb, LIndex, LMiddle, LRing, LPinky, RThumb, RIndex, RMiddle, RRing, RPinky, _stopwatch.Elapsed);
        Write(entry);
    }
    public void CreateEyeTrackingPositionLog(Vector3 position)
    {
        if(_stopwatch == null) return;
        LogEyeTracking entry = new LogEyeTracking(position, _stopwatch.Elapsed);
        Write(entry);
    }
    private void Test()
    {
        CreateServerResponseLog(new PredictionSet
        {
            ID = 0,
            predictions = new Prediction[]
            {
                new Prediction
                {
                    ID = 1,
                    Order = 0,
                    Contents = "prediction"
                }
            }
        });
    }
}
