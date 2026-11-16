using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Runtime.CompilerServices;
using CSWebSockets.Structs;
public class PredictionButton : MonoBehaviour
{
    [SerializeField] private Vector3 _originalScale = new Vector3(0.6f, 2.5f, 1);
    [SerializeField] private TextMeshPro _textMeshPro;
    private Prediction _buttonPrediction;
    public Prediction GetPrediction => _buttonPrediction;
    private ARButton _button;
    public void SetPrediction(Prediction buttonPrediction)
    {
        _buttonPrediction = buttonPrediction;
    }
    private float _defaultRatio = 2.5f;

    [SerializeField] private Transform _parentTransform;
    [SerializeField] private Vector3 _originalParentScale;
    private void Awake()
    {
        _button = GetComponent<ARButton>();
    }
    private void Update()
    {
        _button.ALLOW_PRESS = PredictionMenuCreator.Instance.GetAllowPredictionHits();
    }
    public void ResizeText(Vector3 newButtonScale)
    {
        float newScale = _parentTransform.localScale.x / _originalParentScale.x;
        //_textMeshPro.transform.localScale = new Vector3(_originalScale.x / newScale, _originalScale.y, _originalScale.z);
        _textMeshPro.transform.localScale = new Vector3(_originalScale.x, _originalScale.y, _originalScale.z);



        return;
        float newRatio = _textMeshPro.transform.localScale.y / _textMeshPro.transform.localScale.x;
        newRatio = _defaultRatio / newRatio;

        //_textMeshPro.transform.localScale = new Vector3(_originalScale.x / newRatio, _originalScale.y, _originalScale.z);
        _textMeshPro.transform.localScale = new Vector3(newRatio, _originalScale.y, _originalScale.z);
    }
    public void SetText(string t)
    {
        _textMeshPro.text = t;
    }
    public void Submit()
    {
        if (!PredictionMenuCreator.Instance.GetAllowPredictionHits()) return;
        if (!KeyboardController.Instance.TryAcceptPrediction(_buttonPrediction)) return;
        PredictionMenuCreator.Instance.TogglePredictionHits(false);
        PredictionMenuCreator.Instance.DestroyButtons();
    }
}
