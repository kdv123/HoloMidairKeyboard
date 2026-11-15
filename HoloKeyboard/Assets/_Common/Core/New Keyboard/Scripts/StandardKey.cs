using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
[DefaultExecutionOrder(0)]
public class StandardKey : MonoBehaviour, ICollideable
{
    [Header("Objects")]
    [SerializeField] private MeshRenderer _backgroundRenderer;
    [SerializeField] private TextMeshPro _keyText;
    [SerializeField] private KeyboardController Controller
    {
        get
        {
            if(c == null)
            {
                c = FindObjectOfType<KeyboardController>();
            }
            return c;
        }
    }
    private KeyboardController c;
    [Header("Data")]
    [SerializeField] private KeyData _keyData;
    [SerializeField] private Material _standardMaterial;
    [SerializeField] private Material _pressedMaterial;
    private RaycastCreator _raycaster;
    /// <summary>
    /// What alternate value remaining #'s this key should remain highlighted for after being pressed (Inclusive). 0,0 will never remain highlighted.
    /// </summary>
    [SerializeField] private Vector2 _highlightForAlternateBounds = Vector2.zero;
    /// <summary>
    /// The maximum distance below the key the finger collider can be in order for it to still count as a keypress.
    /// </summary>
    [SerializeField] private float _colliderYOffset = 0.005f;
    /// <summary>
    /// If the key should ignore an update to its material because it was just pressed.
    /// </summary>
    private bool _wasJustPressed = false;
    // Start is called before the first frame update
    void Awake()
    {
        c = FindObjectOfType<KeyboardController>();
        _raycaster = FindFirstObjectByType<RaycastCreator>();
    }
    private void OnEnable()
    {
        CollisionChecker.Instance.RegisterCollisionObject(gameObject, this);
    }
    private void OnDisable()
    {
        CollisionChecker.Instance.RemoveCollisionObject(gameObject, this);
    }
    /// <summary>
    /// Updates a key to use the most recent data from the keyboard controller.
    /// </summary>
    public void UpdateKey()
    {
        if (Controller.IsCurrentlyAlternate)
        {
            _keyText.text = _keyData.DisplayBoth ? $"<color=grey>{_keyData.DefaultInput}</color>\n<color=white>{_keyData.AlternateInput}</color>" : $"<color=white>{_keyData.AlternateInput}</color>";
        }
        else
        {
            _keyText.text = _keyData.DisplayBoth ? $"<color=grey>{_keyData.AlternateInput}</color>\n<color=white>{_keyData.DefaultInput}</color>" : $"<color=white>{_keyData.DefaultInput}</color>";
        }

        if(ShouldRemainHighlighted()) _backgroundRenderer.sharedMaterial = _pressedMaterial;
        else if (!_wasJustPressed) _backgroundRenderer.sharedMaterial = _standardMaterial;
    }
    [ContextMenu("SIMULATE")]
    public void Wow()
    {
        Press(null);
    }
    /// <summary>
    /// Simulates a keypress.
    /// </summary>
    public void Press(Vector3? worldPosition = null)
    {
        StartCoroutine(PressedCoroutine(worldPosition));
    }
    /// <summary>
    /// Calculates if a key should remain highlighted even if it is being told to not be. This is mainly used when a key hasn't finished its press animation and UpdateKey() is called, or if the key has finished its animation but has outstanding logic (I.E capslock should remain highlighted)
    /// </summary>
    private bool ShouldRemainHighlighted()
    {
        if (_highlightForAlternateBounds == Vector2.zero) return false;
        if(Controller.AlternateInputsRemaining >= _highlightForAlternateBounds.x && Controller.AlternateInputsRemaining <= _highlightForAlternateBounds.y)
        {
            return true;
        }
        return false;
    }
    /// <summary>
    /// Coroutine that runs when a key is pressed. This plays the press animation, stores locally that the key was just pressed, and sends the value of the input to the key controller.
    /// </summary>
    /// <returns></returns>
    private IEnumerator PressedCoroutine(Vector3? worldPosition = null)
    {
        if (worldPosition != null) _raycaster.DispatchGaussian(worldPosition.Value);
        _wasJustPressed = true;
        _backgroundRenderer.sharedMaterial = _pressedMaterial;
        string value = Controller.IsCurrentlyAlternate ? _keyData.AlternateInput : _keyData.DefaultInput;
        Controller.OnKeyPressed(value);
        yield return new WaitForSeconds(0.2f);
        _wasJustPressed = false;
        if(!ShouldRemainHighlighted()) _backgroundRenderer.sharedMaterial = _standardMaterial;
        yield break;
    }
    /*
    private void OnTriggerEnter(Collider other)
    {
        //13 is the "Fingertip" layer.
        if (other.gameObject.layer != 13) return;
        if (Mathf.Abs(Mathf.Abs(other.transform.position.y) - Mathf.Abs(transform.position.y)) > _colliderYOffset) return;
        Press();
    }
    */
    public void OnCustomCollisionEnter(Vector3? worldPosition = null)
    {
        Press(worldPosition);
    }
}
