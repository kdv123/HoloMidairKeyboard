using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
public class ResizeableButton : MonoBehaviour
{
    [SerializeField] private TextMeshPro[] _text;
    [SerializeField] private Vector3 _cachedScale;
    private void Awake()
    {
        _cachedScale = transform.localScale;
    }
    public void Resize(Vector3 startPos, Vector3 endPos, bool localSpace)
    {
        float scaleX = Vector3.Distance(new Vector3(startPos.x, 0, 0), new Vector3(endPos.x, 0, 0));
        float scaleY = Vector3.Distance(new Vector3(0, startPos.y, 0), new Vector3(0, endPos.y, 0));
        //transform.position = centerPos;
        transform.localScale = new Vector3(scaleX / 10, 0.05f, scaleY / 10);
        Vector3 pos = new Vector3(startPos.x + endPos.x, startPos.y + endPos.y, startPos.z + endPos.z) / 2f;
        if (localSpace) transform.localPosition = pos;
        else transform.position = pos;
        ResizeText();
    }
    private void ResizeText()
    {
        Vector3 textScale = new Vector3(
            _cachedScale.x / transform.localScale.x,
            _cachedScale.y / transform.localScale.y,
            _cachedScale.z / transform.localScale.z);
        for(int i = 0; i < _text.Length; i++)
        {
            _text[i].transform.localScale = textScale;
        }
    }
}
