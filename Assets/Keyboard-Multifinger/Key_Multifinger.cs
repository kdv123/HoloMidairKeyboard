using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/**
 * Author      : Cecilia Schmitz
 * Email       : cmschmit@mtu.edu
 * Description : Contains all methods and collision processing for the multi-finger keys
 *             : Note: In order for multi-finger keys to function, this script must be attached IN ADDITION to the "Key" script.
 */
public class Key_Multifinger : MonoBehaviour
{

    [SerializeField] private float threshold = 0.3f; // The distance the key must be pressed in order to activate/release. The smaller the threshold, the further the key must be pressed to trigger it.
    [SerializeField] private float deadzone = 0.025f; // The distance within any key movement should be ignored. The larger the deadzone, the more the key has to move in order for a value to be detected.

    private bool isPressed; // If the key is currently pressed down
    private Vector3 startPosition; // The initial position of the key
    private ConfigurableJoint joint; // The joint attaching the key to the backplate
    public UnityEvent onPressed, onReleased; // Events triggered when the key is pressed/released

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

    /**
     * Description: Returns how far down the key has been pressed (how far away it is from the starting position) as a percentage.
     * ex. If 0.5 is returned, the distance the key has been pressed in is equal to half of it's height.
     */
    private float getValue()
    {
        float value = Vector3.Distance(startPosition, transform.localPosition) / joint.linearLimit.limit;
        if (Mathf.Abs(value) < deadzone) value = 0;
        if (transform.localPosition.y > startPosition.y) value = 0;

        return Mathf.Clamp(value, -1, 1);
    }

    /**
     * Description: Called when the key is pressed. Triggers the associated UnityEvent.
     */
    private void Pressed()
    {
        isPressed = true;
        onPressed.Invoke();
    }

    /**
     * Description: Called when the key is released. Triggers the associated UnityEvent.
     */
    private void Released()
    {
        isPressed = false;
        onReleased.Invoke();
    }

    /**
     * Setter method for the threshold variable.
     */
    public void setThreshold(float t)
    {
        threshold = t;
    }
}
