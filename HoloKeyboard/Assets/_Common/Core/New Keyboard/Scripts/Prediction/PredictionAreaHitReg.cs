using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PredictionAreaHitReg : MonoBehaviour
{
    [SerializeField] private PredictionMenuCreator _predictionMenu;
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Fingertip") return;
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag != "Fingertip") return;
        _predictionMenu.TogglePredictionHits(true);
    }
}
