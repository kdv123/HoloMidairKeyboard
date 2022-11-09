using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Key_Multifinger : MonoBehaviour
{

    [SerializeField] private float threshold = 0.3f;
    [SerializeField] private float deadzone = 0.025f;

    private bool isPressed;
    private Vector3 startPosition;
    private ConfigurableJoint joint;
    public UnityEvent onPressed, onReleased;

    void Start()
    {
        startPosition = transform.localPosition;
        joint = GetComponent<ConfigurableJoint>();
    }

    void FixedUpdate()
    {
        if (!isPressed && getValue() + threshold >= 1) Pressed();
        if (isPressed && getValue() - threshold <= 0) Released();
            
    }

    private float getValue()
    {
        float value = Vector3.Distance(startPosition, transform.localPosition) / joint.linearLimit.limit;
        if (Mathf.Abs(value) < deadzone) value = 0;

        return Mathf.Clamp(value, -1, 1);
    }

    private void Pressed()
    {
        isPressed = true;
        onPressed.Invoke();
        Debug.Log("Pressed");
    }

    private void Released()
    {
        isPressed = false;
        onReleased.Invoke();
        Debug.Log("Released");
    }
}
