using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;
/// <summary>
/// Code for sunflower translated from https://stackoverflow.com/questions/28567166/uniformly-distribute-x-points-inside-a-circle
/// </summary>
public class RaycastCreator : MonoBehaviour
{
    public float _maxOffset = 0.01416f;
    public HeatmapDisplay _heatmap;
    /// <summary>
    /// LayerMask to use when checking collisions
    /// </summary>
    [SerializeField] private LayerMask _layerMask;
    /// <summary>
    /// List of points to perform raycast on (not accounting for offset)
    /// </summary>
    private List<Vector3> _points = new List<Vector3>();
    /// <summary>
    /// Gaussian distribution
    /// </summary>
    private List<Vector3> _guassian = new List<Vector3>();
    /// <summary>
    /// Min coords of the raycasts
    /// </summary>
    private Vector2 _minPoint = Vector2.zero;
    /// <summary>
    /// Max coords of the raycasts
    /// </summary>
    private Vector2 _maxPoint = Vector2.zero;
    /// <summary>
    /// Min coords of the guassian
    /// </summary>
    private Vector2 _minGuassianPoint = Vector2.zero;
    /// <summary>
    /// Max coords of the guassian
    /// </summary>
    private Vector2 _maxGuassianPoint = Vector2.zero;
    /// <summary>
    /// Temp
    /// </summary>
    public GameObject obj;
    /// <summary>
    /// Eyetracking used to update current hit data
    /// </summary>
    [SerializeField] private UpdatedEyeTracking _eyeTracking;
    /// <summary>
    /// NativeArray to access raycast job results
    /// </summary>
    private NativeArray<RaycastHit> _hits;
    /// <summary>
    /// List of points as a native array so it can be passed to a job
    /// </summary>
    private NativeArray<Vector3> _raycastBasePoints;
    /// <summary>
    /// Timer used for calculating job delay
    /// </summary>
    private Stopwatch _timer = new Stopwatch();
    private bool _showVisuals = false;
    /// <summary>
    /// Weight of the different keys
    /// </summary>
    private Dictionary<string, KeyWeightValue> _keyWeights = new Dictionary<string, KeyWeightValue>();
    private List<GameObject> _hitVisuals = new List<GameObject>();

    [SerializeField] private Transform _fingerTip;
    private Vector3[] _fingerPoints = new Vector3[10];
    private Vector3 _lastPoint = Vector3.zero;
    private class KeyWeightValue
    {
        public float Value;
        public KeyWeightValue(float value) { Value = value; }
    }
    /// <summary>
    /// Start sets up the list of points to use
    /// </summary>
    void Start()
    {
        CreatePointsList(1000, 0.02f, 2, false);
        for(int i = 0; i < 1000; i++)
        {
            GameObject o = Instantiate(obj, Vector3.one, Quaternion.identity, transform);
            _hitVisuals.Add(o);
        }
        _guassian = CreateGuassian(1, Vector2.one / 2, Vector2.one / 20, _points);
        foreach (Vector3 point in _guassian)
        {
            _heatmap.AddDotToHeatmap(point.x / 5, point.z / 2, point.y, HeatmapDisplay.HeatmapAddMode.Add);
        }
        _heatmap.DrawHeatmap();
        for(int i = 0; i < 10; i++)
        {
            _fingerPoints[i] = Vector3.zero;
        }
        for(int i = 0; i < 9; i++)
        {
            _fingerPoints[i + 1] = _fingerPoints[i];
            //Disable this since when non-holo play mode is inititated it throws errors
            if (_fingerTip != null)
            {
                _fingerPoints[0] = _fingerTip.transform.position;
            }
        }
        StartCoroutine(DrawRay());
    }
    private IEnumerator DrawRay()
    {
        while (true)
        {
            Vector3 point = CalcLastPoint();
            Debug.DrawLine(_lastPoint, point, Color.red, 0.2f);
            _lastPoint = point;
            yield return new WaitForSeconds(0.2f);
        }
    }
    private Vector3 CalcLastPoint()
    {
        Vector3 point = Vector3.zero;
        for(int i = 0; i < 10; i++)
        {
            point.x += _fingerPoints[i].x;
            point.y += _fingerPoints[i].y;
            point.z += _fingerPoints[i].z;
        }
        point.x /= 10;
        point.y /= 10;
        point.z /= 10;
        return point;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //CalculateRaycasts();
        }
    }
    /// <summary>
    /// Dispatch the raycast commands 
    /// </summary>
    [BurstCompile]
    struct SetupJob : IJobParallelFor
    {
        public NativeArray<Vector3> Points;
        public NativeArray<RaycastCommand> Commands;
        public LayerMask Layermask;
        public Vector3 Offset;
        public void Execute(int index)
        {
            QueryParameters param = new QueryParameters(Layermask);
            Commands[index] = new RaycastCommand(Points[index] + Offset, Vector3.down, param);
        }
    }
    public void ToggleShowVisuals()
    {
        _showVisuals = !_showVisuals;
    }
    public void ClearGaussian()
    {
        for(int i = 0; i < _hitVisuals.Count; i++)
        {
            _hitVisuals[i].transform.position = Vector3.one;
        }
    }
    public void DispatchGaussian(Vector3 worldPoint)
    {
        if (_eyeTracking.IsEyeTracking) return;
        CalculateRaycasts(worldPoint);
    }
    /// <summary>
    /// Performs the job to raycast from all points. Completes when all casts are finished.
    /// </summary>
    private async void CalculateRaycasts(Vector3 worldPosition)
    {
        Debug.Log("Starting raycast calculation");
        _timer.Restart();

        //Create a new native array to store job results
        _hits = new NativeArray<RaycastHit>(1000, Allocator.TempJob);
        NativeArray<RaycastCommand> commands = new NativeArray<RaycastCommand>(1000, Allocator.TempJob);
        //Setup the job
        var setupJob = new SetupJob()
        {
            Points = _raycastBasePoints,
            Commands = commands,
            Layermask = _layerMask,
            Offset = worldPosition + new Vector3(0, _maxOffset, 0)
        };

        JobHandle deps = setupJob.Schedule(commands.Length, 1, default(JobHandle));

        deps = RaycastCommand.ScheduleBatch(commands, _hits, 1, deps);

        await CheckJobStatus(deps);

        if (_showVisuals)
        {


            for (int i = 0; i < _hits.Length; i++)
            {
                if (_hits[i].transform != null)
                {
                    //Debug.Log(_hits[i].transform.gameObject.name);
                    //Instantiate(obj, _raycastBasePoints[i] + worldPosition - new Vector3(0, (_maxOffset - (_guassian[i].y * _maxOffset)) - _maxOffset, 0), Quaternion.identity, transform);
                    _hitVisuals[i].transform.position = _raycastBasePoints[i] + worldPosition - new Vector3(0, (_maxOffset - (_guassian[i].y * _maxOffset)) - _maxOffset, 0);
                    // - (Vector3.up - new Vector3(0, Mathf.InverseLerp(transform.position.y, _maxOffset, _points[i].y), 0))
                }
                else
                {
                    _hitVisuals[i].transform.position = Vector3.one;
                }
            }
        }
        CreateKeyWeights();
        _hits.Dispose();
        commands.Dispose();


    }
    private void CreateKeyWeights()
    {
        _keyWeights.Clear();
        List<float> tempWeights = new List<float>();
        //Add each hit to the dictionary
        for(int i = 0; i < _hits.Length; i++){

            if (_hits[i].transform == null) continue;
            if (!_keyWeights.ContainsKey(_hits[i].transform.gameObject.name))
            {
                _keyWeights.Add(_hits[i].transform.gameObject.name, new KeyWeightValue(0));
            }
        }
        //Total the weights of all the valid hits
        float totalWeight = 0;  
        for (int i = 0; i < _hits.Length; i++)
        {
            if (_hits[i].transform == null) continue;
            totalWeight += _guassian[i].y;
            
            _keyWeights[_hits[i].transform.gameObject.name].Value += _guassian[i].y;
            
        }
        foreach(string s in _keyWeights.Keys)
        {
            Debug.Log(_keyWeights[s].Value);
            _keyWeights[s].Value = _keyWeights[s].Value / totalWeight;
            Debug.Log(s + ", " + _keyWeights[s].Value);
        }
    }
    
    private async Task CheckJobStatus(JobHandle handle)
    {
        while (handle.IsCompleted == false) await Task.Delay(1);
        _timer.Stop();
        Debug.Log($"Job completed in {_timer.ElapsedMilliseconds}ms");
        handle.Complete();
    }
    private void OnDestroy()
    {
        _raycastBasePoints.Dispose();
        Debug.Log("Points native array disposed.");
    }
    /// <summary>
    /// Initializes the array of points to use for raycasts
    /// </summary>
    /// <param name="n">Number of points to create</param>
    /// <param name="scale">What the points will be scaled by</param>
    /// <param name="alpha">Range 0 - 2, how uniformly the points try and line up on the edges</param>
    /// <param name="geodesic">idk</param>
    private void CreatePointsList(int n, float scale, float alpha = 0, bool geodesic = false)
    {

        float phi = 2.11803398875f; //(1 + Mathf.Sqrt(5)) / 2;
        float angle_stride = 360 * phi;
        int b = Mathf.RoundToInt(alpha * Mathf.Sqrt(n)); // Num boundry points

        for (int k = 0; k < n; k++)
        {
            float r = Radius(k, n, b);
            float theta = geodesic ? k * 360 * phi : k * angle_stride;
            float x = !float.IsNaN(r * Mathf.Cos(theta)) ? r * Mathf.Cos(theta) : 0;
            float y = !float.IsNaN(r * Mathf.Sin(theta)) ? r * Mathf.Sin(theta) : 0;
            Vector3 point = new Vector3(x * scale, 0, y * scale);
            _points.Add(point);
            if (point.x < _minPoint.x) _minPoint.x = point.x;
            if (point.x > _maxPoint.x) _maxPoint.x = point.x;
            if (point.z < _minPoint.y) _minPoint.y = point.z;
            if (point.z > _maxPoint.y) _maxPoint.y = point.z;
        }

        float Radius(float k, float n, float b)
        {
            if (k > n - b)
            {
                return 1.0f;
            }
            else
            {
                return Mathf.Sqrt(k - 0.5f) / Mathf.Sqrt(n - (b + 1) / 2);
            }
        }
        ConvertPointsListToNativeArray();
    }
    /// <summary>
    /// Converts the calculated raycast point list from a list to a nativearray so it can be passed into the job
    /// </summary>
    private void ConvertPointsListToNativeArray()
    {
        _raycastBasePoints = new NativeArray<Vector3>(_points.Count, Allocator.Persistent);
        for (int i = 0; i < _points.Count; i++)
        {
            _raycastBasePoints[i] = _points[i];
        }
    }

    private List<Vector3> CreateGuassian(float amplitude, Vector2 center, Vector2 spread, List<Vector3> points)
    {
        List<Vector3> result = new List<Vector3>();
        float height = 0;
        float innerX = 0;
        float innerY = 0;
        foreach (Vector3 point in points)
        {
            float adjustedX = Mathf.InverseLerp(_minPoint.x, _maxPoint.x, point.x);
            float adjustedY = Mathf.InverseLerp(_minPoint.x, _maxPoint.x, point.z);
            innerX = Mathf.Pow((adjustedX - center.x), 2) / (spread.x * 2);
            innerY = Mathf.Pow((adjustedY - center.y), 2) / (spread.y * 2);
            height = amplitude * Mathf.Exp(-1 * (innerX + innerY));
            result.Add(new Vector3(adjustedX, height, adjustedY));
        }
        return result;
    }
}
