using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionObject : MonoBehaviour
{
    private void Start()
    {
        CollisionChecker.Instance.AddCollisionChecker(gameObject);
    }
    private void OnDestroy()
    {
        CollisionChecker.Instance.RemoveCollisionChecker(gameObject);
    }

}
