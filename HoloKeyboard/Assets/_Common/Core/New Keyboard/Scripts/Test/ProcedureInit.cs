using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProcedureInit : MonoBehaviour
{
    [SerializeField] private DisplayScreen _display;
    [SerializeField] private Procedure _procedure;
    private bool _hasEnteredID = false;
    // Start is called before the first frame update
    void Start()
    {
        KeyboardController._onSubmit += OnKeyboardSubmitted;
        _display.DisplayMessage("Enter participant ID");
    }
    /// <summary>
    /// Will get called once on first submit, should contain participant ID
    /// </summary>
    private void OnKeyboardSubmitted(string message)
    {
        if (!_hasEnteredID)
        {
            LogManager.Instance.OpenLog(message);
            KeyboardController.Instance.ClearInput();
            KeyboardController.Instance.EnableWordPrediction();
            _hasEnteredID = true;
            _procedure.StartTest();
        }
        else
        {
            string target = _procedure.GetCurrentSentence();
            int num = _procedure.GetCurrentSentenceNumber();
            LogManager.Instance.CreateProcedurePhaseSubmittedLog(target, message, num);
            KeyboardController.Instance.ClearInput();
            _procedure.Advance();
        }
    }
}
