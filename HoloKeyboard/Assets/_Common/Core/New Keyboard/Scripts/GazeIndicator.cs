using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GazeIndicator : MonoBehaviour
{
    //The 100% completed scale
    private Vector3 _cachedMaxScale;
    private void Start()
    {
        Setup();
    }
    /// <summary>
    /// Sets the scale of the indicator
    /// </summary>
    /// <param name="p">Progress of the indicator, will be clamped between 0 and 1.</param>
    public void SetProgress(float p)
    {
        p = Mathf.Clamp01(p);
        transform.localScale = _cachedMaxScale * p;
    }
    /// <summary>
    /// Sets the position of the indicator in world space.
    /// </summary>
    /// <param name="p">Indicator position</param>
    public void SetPosition(Vector3 p)
    {
        //transform.position = new Vector3(p.x, transform.position.y, p.z);
        transform.position = new Vector3(p.x, p.y, p.z);
    }
    /// <summary>
    /// Sets the rotation of the indicator in world space.
    /// </summary>
    /// <param name="rot">Indicator rotation</param>
    public void SetRotation(Quaternion rot) 
    {
        transform.rotation = rot;   
    }
    /// <summary>
    /// Gets the scale that represents 100% progress.
    /// </summary>
    private void Setup()
    {
        _cachedMaxScale = transform.localScale;
    }
}
