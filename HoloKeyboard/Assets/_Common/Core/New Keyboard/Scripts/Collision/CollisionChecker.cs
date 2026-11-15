using Microsoft.MixedReality.Toolkit;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class CollisionChecker : MonoBehaviour
{
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private float _touchCooldown = 0.25f;
    public static CollisionChecker Instance;
    private List<GameObject> _collisionCheckers = new List<GameObject>();
    private HashSet<ICollideable> _collideables = new HashSet<ICollideable>();
    private Dictionary<GameObject, ICollideable> _colFromObject = new Dictionary<GameObject, ICollideable>();
    private List<Vector3> _lastPositions = new List<Vector3>();
    private float _currentCooldown = 0;
    public void AddCollisionChecker(GameObject obj)
    {
        _collisionCheckers.Add(obj);
        _lastPositions.Add(obj.transform.position);
    }
    public void RemoveCollisionChecker(GameObject obj)
    {
        int index = _collisionCheckers.IndexOf(obj);
        _collisionCheckers.Remove(obj); _lastPositions.RemoveAt(index);
    }
    public void RegisterCollisionObject(GameObject obj, ICollideable col)
    {
        _collideables.Add(col);
        _colFromObject.Add(obj, col);
    }
    public void RemoveCollisionObject(GameObject obj, ICollideable col)
    {
        _collideables.Remove(col);
        _colFromObject.Remove(obj);
    }

    private Ray _collisionRay;
    private GameObject _lastHitObject = null;
    private void Awake()
    {
        Instance = this;
    }
    private void Update()
    {
        _currentCooldown -= Time.deltaTime;
    }
    private void FixedUpdate()
    {
        for (int i = 0; i < _collisionCheckers.Count; i++)
        {
            if (_collisionCheckers[i].activeInHierarchy)
            {
                DoCollisionCheck(_collisionCheckers[i].transform.position, _lastPositions[i]);
            }
            _lastPositions[i] = _collisionCheckers[i].transform.position;
        }
    }
    private void DoCollisionCheck(Vector3 curPos, Vector3 lastPos)
    {
        if (_currentCooldown > 0) return;
        //Create ray along the axis from the last position to the current position
        _collisionRay = new Ray(lastPos, curPos - lastPos);
        if (Physics.Raycast(_collisionRay, out RaycastHit hit, Vector3.Distance(lastPos, curPos), _layerMask))
        {
            Debug.Log(hit.transform.gameObject.name);
            if (hit.transform.gameObject == _lastHitObject)
            {
                //return;
            }
            _lastHitObject = hit.transform.gameObject;
            //If an entry exists for the hit object, we should call its collision method. This is cheaper than using getcomponent to form a component list and casting each to check since looking up in a dictionary is O(1)
            if (_colFromObject.ContainsKey(hit.transform.gameObject))
            {
                _currentCooldown = _touchCooldown;
                ICollideable col = _colFromObject[hit.transform.gameObject];
                col.OnCustomCollisionEnter(hit.point);
            }
        }
    }
}
