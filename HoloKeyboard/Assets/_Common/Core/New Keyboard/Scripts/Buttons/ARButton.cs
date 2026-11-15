using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class ARButton : MonoBehaviour
{
    /// <summary>
    /// List of objects that should be active for visuals in each state.
    /// </summary>
    [SerializeField] private MeshRenderer _backgroundRenderer;
    [SerializeField] private Material _standardMaterial;
    [SerializeField] private Material _pressedMaterial;

    [SerializeField] private GameObject[] _onObjects;
    [SerializeField] private GameObject[] _offObjects;
    [SerializeField] private UnityEvent<bool> _onPress;

    private bool _active = false;

    public bool ALLOW_PRESS = true;
    private void Start()
    {
        if (PredictionMenuCreator.Instance != null)
        {
            ALLOW_PRESS = PredictionMenuCreator.Instance.GetAllowPredictionHits();
        }
        //Register button with the gaze button manager so it can be affected by eye gaze input
        GazeButtonManager.Instance.RegisterButton(this);
    }
    public void OnStatusChanged()
    {
        for(int i = 0; i < _onObjects.Length; i++)
        {
            _onObjects[i].SetActive(_active);
        }
        for (int i = 0; i < _offObjects.Length; i++)
        {
            _offObjects[i].SetActive(!_active);
        }
        _onPress?.Invoke(_active);
    }
    private bool _currentlyPressed = false;
    public void PressButton()
    {
        //Don't allow super quick multi presses
        if (_currentlyPressed) return;
        if (!ALLOW_PRESS) return;
        _active = !_active;
        OnStatusChanged();
        StartCoroutine(PressedCoroutine());
    }
    private IEnumerator PressedCoroutine()
    {
        _currentlyPressed = true;
        _backgroundRenderer.sharedMaterial = _pressedMaterial;
        yield return new WaitForSeconds(0.2f);
        _backgroundRenderer.sharedMaterial = _standardMaterial;
        _currentlyPressed = false;
        yield break;
    }
    private void OnTriggerEnter(Collider other)
    {
        //13 is the "Fingertip" layer.
        if (other.gameObject.layer != 13) return;
        PressButton();
    }
}
