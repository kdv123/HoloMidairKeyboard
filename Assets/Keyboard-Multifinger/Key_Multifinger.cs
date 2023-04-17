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
    [SerializeField] private float upThreshold = 0.8f; // The distance they key must be pushed up before it will no longer move.

    private bool isPressed; // If the key is currently pressed down
    private bool interactable = true; // If the key is currently interactable
    private Vector3 startPosition; // The initial position of the key
    private ConfigurableJoint joint; // The joint attaching the key to the backplate
    public UnityEvent onPressed, onReleased; // Events triggered when the key is pressed/released
    private string finger = ""; // The finger pressing the key

    void Start()
    {
        startPosition = transform.localPosition;
        joint = GetComponent<ConfigurableJoint>();
    }

    void FixedUpdate()
    {
        // Checks if the key has been pressed
        if (!isPressed && getValue() + threshold >= 1) Pressed();
        if (isPressed && getValue() - threshold <= 0) Released();
        if (!interactable && getValue() + upThreshold >= 0) Interactable();
        if (interactable && getValue() - upThreshold <= -1) NonInteractable();
        if(!interactable)
        {
            GetComponent<Rigidbody>().velocity = new Vector3(0, -0.02f, 0);
        }
    }

    /**
     * Description: Returns how far down the key has been pressed (how far away it is from the starting position) as a percentage.
     * ex. If 0.5 is returned, the distance the key has been pressed in is equal to half of it's height.
     */
    private float getValue()
    {
        float value = Vector3.Distance(startPosition, transform.localPosition) / joint.linearLimit.limit;
        if (Mathf.Abs(value) < deadzone) value = 0;
        if (transform.localPosition.y > startPosition.y) value *= -1;

        return Mathf.Clamp(value, -1, 1);
    }

    /**
     * Activates collision for the key
     **/
    private void Interactable()
    {
        gameObject.layer = 10; // Layer 10 is the "Keys" layer
        interactable = true;
    }

    /**
     * Deactivates collision for the key
     **/
    private void NonInteractable()
    {
        gameObject.layer = 11; // Layer 11 is the "NoCollision" layer
        interactable = false;
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

    /**
     * Tracks the finger that last touched the key
     **/
    private void OnCollisionEnter(Collision collision)
    {
        finger = collision.gameObject.name;
    }

    /**
     * Returns the finger that last touched the key
     **/
    public string getFinger()
    {
        return finger;
    }
}
