using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class master : MonoBehaviour
{
    public TextMeshPro typeText; // Reference to the instructions text
    public EyeTracking et; // Reference to the eyetracker
    public HandTracking ht; // Reference to the handtracker
    public Keyboard keyboard; // Reference to the keyboard
    public Logger log; // Reference to the Log
    public int delay = 30; // Time delay (in seconds) between each case
    int state = 0; // Current state
    int curcase = 0; // Current case index
    int casenum; // Current case
    bool isDelayed = false; // Whether or not the user is currently between cases

    // List of all possible case orders
    // CASE 1: Index fingers only
    // CASE 2: Ten fingers only
    // CASE 3: Ten fingers with eye tracking
    // CASE 4: Index fingers with eye tracking
    int[,] cases = new int[,]
    {
        {1, 2, 3, 4},
        {1, 2, 4, 3},
        {1, 3, 2, 4},
        {1, 3, 4, 2},
        {1, 4, 2, 3},
        {1, 4, 3, 2},
        {2, 1, 3, 4},
        {2, 1, 4, 3},
        {2, 3, 1, 4},
        {2, 3, 4, 1},
        {2, 4, 1, 3},
        {2, 4, 3, 1},
        {3, 1, 2, 4},
        {3, 1, 4, 2},
        {3, 2, 1, 4},
        {3, 2, 4, 1},
        {3, 4, 1, 2},
        {3, 4, 2, 1},
        {4, 1, 2, 3},
        {4, 1, 3, 2},
        {4, 2, 1, 3},
        {4, 2, 3, 1},
        {4, 3, 1, 2},
        {4, 3, 2, 1}
    };

    // List of case instructions
    string[] typeCase = { 
        "Type using only your index fingers.\n", 
        "Type using all ten fingers.\n", 
        "Type using all ten fingers. Look at the keys as you type.\n",
        "Type using only your index fingers. Look at the keys as you type.\n"
    };

    void Start()
    {
        typeText.text = "Please type your participant number.";
        ht.trackTwoFingers();
        et.setEnabled(false);
        state = 0;
    }

    /**
     * Called when the enter key is pressed.
     * Verifies the participant's ID number
     **/
    public void enterPressed()
    {
        if (state == 0)
        {
            string s = keyboard.getTypedText();
            if (int.TryParse(s, out int i))
            {
                casenum = i % 24;
                curcase = cases[casenum, state];
                state = 1;
                updateCase();
            }
            else
            {
                typeText.text = "The number you entered was invalid.\nPlease type your participant number.";
            }
        }
        log.write_all_data();
    }

    /**
     * Updates the eyetracker and handtracker to match the case number
     **/
    private void updateCase()
    {
        log.write("CASE," + curcase);
        Debug.Log("CASE: " + curcase);
        keyboard.setTypeCase(typeCase[curcase - 1]);
        /******* CASE 1 *******/
        if (curcase == 1)
        {
            et.setEnabled(false);
            ht.trackTwoFingers();
            keyboard.startSentences();
        }
        /******* CASE 2 *******/
        else if (curcase == 2)
        {
            et.setEnabled(false);
            ht.trackTenFingers();
            keyboard.startSentences();
        }
        /******* CASE 3 *******/
        else if (curcase == 3)
        {
            et.setEnabled(true);
            ht.trackTenFingers();
            keyboard.startSentences();
        }
        /******* CASE 4 *******/
        else if (curcase == 4)
        {
            et.setEnabled(true);
            ht.trackTwoFingers();
            keyboard.startSentences();
        }
    }

    /**
     * Moves to the next case
     **/
    public void nextCase()
    {
        if (state != 4 && !isDelayed)
        {
            isDelayed = true;
            IEnumerator c = caseDelay(delay);
            StartCoroutine(c);
        }
        else if (!isDelayed)
        {
            typeText.text = "You have now completed the study. Thank you for your participation!";
        }

    }

    /**
     * Displays the delay timer
     * After the delay, moves to the next case
     **/
    private IEnumerator caseDelay(int d)
    {
        while (d > 0)
        {
            typeText.text = "Please take a short break.\n" + d / 60 + ":" + (d % 60).ToString("D2") + 
                "\n\nAverage Error Rate: " + keyboard.getAverageErrorRate().ToString("F1") + "%\nAverage WPM: " + keyboard.getAverageWPM().ToString("F1");
            et.setKeyboard(false);
            d--;
            yield return new WaitForSeconds(1);
        }
        curcase = cases[casenum, state];
        state++;
        updateCase();
        et.setKeyboard(true);
        isDelayed = false;
    }

}