using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class NomonPanel : MonoBehaviour
{
    [SerializeField] private TextMeshPro _textMesh;
    [SerializeField] private Transform _handObject;
    [Tooltip("Rotation speed of the clock hand, in revolutions per second.")]
    [SerializeField] private float _rotationSpeed = 0.3f;
    [SerializeField] private Material _enabledPanelMaterial;
    [SerializeField] private Material _disabledPanelMaterial;
    private MeshRenderer _renderer;
    private string _word;
    public string GetWord() => _word;
    public float GetDegrees() => _handObject.localEulerAngles.y;
    public void SetDegrees(float num) => _handObject.localEulerAngles = new Vector3(0, num, 0);

    private bool _rotating = true;
    public void SetDisabled()
    {
        _rotating = false;
        _renderer.material = _disabledPanelMaterial;
    }
    public void SetEnabled()
    {
        _rotating = true;
        _renderer.material = _enabledPanelMaterial;
    }
    public void SetText(string text)
    {
        _textMesh.text = text;
        _word = text;
    }
    public void SetRotationSpeed(float speed)
    {
        _rotationSpeed = speed;
    }
    private void Start()
    {
        _renderer = GetComponent<MeshRenderer>();
    }
    private void Update()
    {
        if (!_rotating) return;
        //_handObject.eulerAngles += new Vector3(0, 180 * Time.deltaTime, 0);
        _handObject.localEulerAngles += new Vector3 (0, 360 * Time.deltaTime * _rotationSpeed, 0);
        if(_handObject.localEulerAngles.y > 360)
        {
            //Clamp possible rotation to be between 0 and 360
            _handObject.localEulerAngles = _handObject.localEulerAngles - new Vector3(0, 360, 0);
        }
    }
}
