using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICollideable
{
    public void OnCustomCollisionEnter(Vector3? worldPosition = null);
}
