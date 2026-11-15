using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DisplayScreen : MonoBehaviour
{
    [SerializeField] private TextMeshPro _displayText;
    private void Awake()
    {
        _displayText.text = string.Empty;
    }
    public void DisplayMessage(string message)
    {
        _displayText.text = message;

    }
}
