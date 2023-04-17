using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using Random = System.Random;
using UnityEngine.Events;
using System;

/**
 * Author      : Cecilia Schmitz
 * Email       : cmschmit@mtu.edu
 * Description : Contains all methods and key event processing for the multi-finger keyboard.
 */
public class Keyboard : MonoBehaviour
{

    public TextMeshPro typeText; // Reference to the instructions text
    public EyeTracking et; // Reference to the eyetracker
    public string typeCase = ""; // Instructions text for each case
    public string typeStart = "Please type the following phrase:\n\n"; // Text to display before the sentence
    private string[] practiceSentences = new string[2]; // List of practice sentences
    private string[] testSentences = new string[8]; // List of test sentences
    private double[] wpm = new double[10]; // An array to store the WPM for each sentence
    private double[] errorRate = new double[10]; // An array to store the Error Rate % for each sentence
    private int sentence = 0; // Which sentence the user is currently on
    private Random r = new Random(); // Random to shuffle/choose random sentences

    public UnityEvent caseclosed; // Runs when a case is completed

    public Key[] keys = new Key[48]; // Array to store all Keys.
    public Key backspace; // Reference to the backspace key
    public Key capslock; // Reference to the capslock key
    public Key leftshift; // Reference to the left shift key
    public Key rightshift; // Reference to the right shift key
    public Key spacebar; // Reference to the spacebar
    public Key enter; // Reference to the enter key
    public float defaultThreshold = 0.5f; // The default threshold of all keys in the "keys" array
    public float spacebarThreshold = 0.9f; // The default threshold of the spacebar
    public float cooldownTime = 0.5f; // Time in seconds between registered key presses

    public TextMeshPro displayText; // The textmeshpro to output the typed string to
    public Logger log; // Logger script that writes key information

    string typed = ""; // Keeps track of what has been typed
    string prevtyped = ""; // Keeps track of what has been entered
    char value; // Temporarily stores the value of a pressed key
    bool shifted = false; // Whether or not a shift key is toggled
    bool capslocked = false; // Whether or not the capslock is toggled
    bool cursorOn = false; // Whether or not the cursor is on
    bool cooldown = false; // Whether or not the cooldown is active
    bool over = false; // Whether or not to override the shift keys
    bool displayStats = false; // Whether to display stats or the next sentence
    DateTime start = DateTime.MinValue; // DateTime to store when the user started typing (for WPM calculation)
    DateTime end = DateTime.MinValue; // DateTime to store when the user stopped typing (for WPM calculation)

    // List of sentence to choose from
    private List<string> sentences = new List<string>
    {
        "Is she done yet?",
        "How are you?",
        "Yes, I am playing.",
        "We are all fragile.",
        "I would like to attend if so.",
        "I can return earlier.",
        "I am trying again.",
        "I will bring John Brindle.",
        "What do you hear?",
        "What's his problem?",
        "See you soon!",
        "It reads like she is in.",
        "I am walking in now.",
        "They have capacity now.",
        "A gift isn't necessary.",
        "Not even close.",
        "Chris Foster is in!",
        "Could you try ringing her?",
        "Do you need it today?",
        "Keep me posted!",
        "John this message concerns me.",
        "Call me to give me a heads up.",
        "And leave my school alone.",
        "What is in the plan?",
        "I am almost speechless.",
        "Ava, please put me on the list.",
        "Take what you can get.",
        "I'm glad you liked it.",
        "This looks fine.",
        "I've never worked with her.",
        "Get with Mary for format.",
        "I hope you are feeling better.",
        "I'm glad she likes her tree.",
        "Have a great trip.",
        "Can you help?",
        "Has anyone else heard anything?",
        "Is it over?",
        "I'll get you one.",
        "OK with me.",
        "What's going on?",
        "You can talk to Becky!",
        "I talked to Duran.",
        "I agreed terms with Greg.",
        "I am at the lake.",
        "I told you silly.",
        "Thanks for your concern.",
        "Thursday works better for me.",
        "What is the mood?",
        "I am on my way.",
        "Do we need to discuss?",
        "Just playing with you!",
        "What's your phone number?",
        "Thanks for checking with me.",
        "This is very sensitive.",
        "Can we have them until we move?",
        "On the plane, doors closing.",
        "Are you in today?",
        "Let it rip.",
        "We just need a sitter.",
        "We must be consistent.",
        "She has absolutely everything.",
        "His is good I think.",
        "We can have wine and catch up.",
        "Money wise that is.",
        "What is wrong?",
        "Where are you?",
        "Thanks good job.",
        "Why do you ask?",
        "Mike, are you aware of this?",
        "I was planning to attend.",
        "That would be great.",
        "Can you help me here?",
        "What is the cost issue?",
        "Please send me an email.",
        "What a jerk.",
        "No material impact.",
        "I will be back Friday.",
        "If not can I call you?",
        "What's your proposal?",
        "Can we meet at 3:30?",
        "Both of us are still here.",
        "Not even in yet.",
        "How soon do you need it?",
        "Are you feeling better?",
        "You have a nice holiday too.",
        "What about Jay?",
        "Ken agreed yesterday.",
        "Neil has been asking around.",
        "Are you available?",
        "Good for you.",
        "We will keep you posted.",
        "Do we have anyone in Portland?",
        "No surprise there.",
        "Hope you guys are doing fine.",
        "Are you going to call?",
        "Did that happen?",
        "I would be glad to participate.",
        "I have a request.",
        "You're the greatest.",
        "What is this?",
        "Travis is in charge.",
        "Can you handle?",
        "Thanks I will.",
        "Are you being a baby?",
        "Did you get this?",
        "I'm on a plane.",
        "Florida is great.",
        "I sent it to her.",
        "I will call.",
        "Please revise accordingly.",
        "I'm going to class.",
        "See you on the third.",
        "Did we get ours back?",
        "What is up with ENE?",
        "I'll call you in the morning.",
        "Are you sure?",
        "Sorry about that!",
        "Is that OK?",
        "Jan has a lot of detail.",
        "Need to watch closely.",
        "What do you think?",
        "Can you bring these to 49C1?",
        "Are you there?",
        "If so what was it?",
        "I'm in Stan's office.",
        "Hey TK, how are you doing?",
        "Don't forget the wood.",
        "This seems fine to me.",
        "What a pain.",
        "Pressure to finish my review!",
        "I like it.",
        "I have 30 minutes then.",
        "Will it be delivered?",
        "Not at this time.",
        "I'm still here.",
        "We will get you a copy.",
        "We're on the way.",
        "What is the purpose of this?",
        "No can do.",
        "Nice weather for it.",
        "Thai sounds good.",
        "Did you differ from me?"
    };


    private void Start()
    {
        InvokeRepeating("blinkCursor", 0f, 0.75f);
        // Update all key thresholds
        foreach (Key key in keys)
        {
            key.GetComponent<Key_Multifinger>().setThreshold(defaultThreshold);
        }
        spacebar.GetComponent<Key_Multifinger>().setThreshold(spacebarThreshold);
    }

    /**
     * Starts a new sequence of sentences
     **/
    public void startSentences()
    {
        getSentences();
        displayStats = true;
        start = DateTime.MinValue;
        nextSentence();
    }

    /**
     * Description : Called when a key is pressed. Appends the value of that key to the current typed string.
     */
    public void keyPressed(Key key)
    {
        if (!cooldown && key.getEnabled())
        {
            if (start == DateTime.MinValue) start = DateTime.Now;
            value = key.getValue();
            typed += value;
            displayText.text = cursorOn ? typed + "_" : typed;
            if (key.GetComponent<Key_Multifinger>() != null)
                log.write_key(value + "", key.GetComponent<Key_Multifinger>().getFinger());
            else
                log.write_key(value + "", "NULL");
            key.playAudio();

            if (shifted)
            {
                over = true;
                updateShift();
            }
            key.setActive();

            cooldown = true;
            StartCoroutine("runCooldown");
        } else if (!key.getEnabled())
        {
            if (key.GetComponent<Key_Multifinger>() != null)
                log.write_inactive_key(key.getValue() + "", key.GetComponent<Key_Multifinger>().getFinger());
            else
                log.write_inactive_key(key.getValue() + "", "NULL");
        }
    }


    /**
     * Description : Removes one character from the typed string (called when backspace key is pressed)
     */
    public void backspacePressed()
    {
        if (!cooldown && backspace.getEnabled() && typed.Length > 0)
        {
            typed = typed.Substring(0, typed.Length - 1);
            displayText.text = cursorOn ? typed + "_" : typed;
            if (backspace.GetComponent<Key_Multifinger>() != null)
                log.write_key("<--", backspace.GetComponent<Key_Multifinger>().getFinger());
            else
                log.write_key("<--", "NULL");
            backspace.playAudio();
            backspace.setActive();

            cooldown = true;
            StartCoroutine("runCooldown");
        }
        else if (!backspace.getEnabled())
        {
            if (backspace.GetComponent<Key_Multifinger>() != null)
                log.write_inactive_key("<--", backspace.GetComponent<Key_Multifinger>().getFinger());
            else
                log.write_inactive_key("<--", "NULL");
        }

    }

    public void enterPressed()
    {
        if(!cooldown && enter.getEnabled() && !(displayStats && typed.Length == 0))
        {
            prevtyped = typed;
            typed = "";
            end = DateTime.Now;
            displayText.text = cursorOn ? typed + "_" : typed;
            if (sentence != 0 && !displayStats)
            {
                et.setKeyboard(true);
                nextSentence();
            }
            else if (sentence != 0 && displayStats)
            {
                et.setKeyboard(false);
                showStats();
            }
            displayStats = !displayStats;
            if (enter.GetComponent<Key_Multifinger>() != null)
                log.write_key("ENTER", enter.GetComponent<Key_Multifinger>().getFinger());
            else
                log.write_key("ENTER", "NULL");
            enter.playAudio();
            enter.setActive();

            cooldown = true;
            StartCoroutine("runCooldown");
        }
    }


    /**
     * Description : Toggles the value of the shift key(S) (called when either shift key is pressed)
     */
    public void updateShift()
    {
        if (!cooldown && (leftshift.getEnabled() || rightshift.getEnabled() || over))
        {
            over = false;
            shifted = !shifted;
            et.setShift(shifted);
            updateCase();
            leftshift.playAudio();
            if (leftshift.GetComponent<Key_Multifinger>() != null)
                log.write_key(value + "SHIFT", leftshift.GetComponent<Key_Multifinger>().getFinger());
            else
                log.write_key(value + "SHIFT", "NULL");

            cooldown = true;
            StartCoroutine("runCooldown");
            if(shifted)
            {
                leftshift.setActive();
                rightshift.setActive();
            } else
            {
                leftshift.setInactive();
                rightshift.setInactive();
            }
        } else if (!leftshift.getEnabled() && !rightshift.getEnabled())
        {
            if (leftshift.GetComponent<Key_Multifinger>() != null)
                log.write_inactive_key("SHIFT", leftshift.GetComponent<Key_Multifinger>().getFinger());
            else
                log.write_inactive_key("SHIFT", "NULL");
        }
    }


    /**
     * Description : Toggles the value of the caps lock key (called when the caps lock key is pressed)
     */
    public void updateCaps()
    {
        if (!cooldown && capslock.getEnabled())
        {
            capslocked = !capslocked;
            et.setCaps(capslocked);
            updateCase();
            capslock.playAudio();
            if (capslock.GetComponent<Key_Multifinger>() != null)
                log.write_key("CAPS", capslock.GetComponent<Key_Multifinger>().getFinger());
            else
                log.write_key("CAPS", "NULL");

            cooldown = true;
            StartCoroutine("runCooldown");
            if (capslocked)
                capslock.setActive();
            else
                capslock.setInactive();
        }
        else if (!capslock.getEnabled())
        {
            if (capslock.GetComponent<Key_Multifinger>() != null)
                log.write_inactive_key("CAPS", capslock.GetComponent<Key_Multifinger>().getFinger());
            else
                log.write_inactive_key("CAPS", "NULL");
        }
    }


    /**
     * Description : Updates the case of all keys depending on the current values of the shift & caps lock keys
     */
    void updateCase()
    {
        if (shifted && capslocked) // letters should be lowercase, everything else should be uppercase
        {
            foreach (Key key in keys)
            {
                if (key.isLetter)
                {
                    key.setLowercase();
                }
                else
                {
                    key.setUppercase();
                }
            }
        }
        else if (!shifted && capslocked) // letters should be uppercase, everything else should be lowercase
        {
            foreach (Key key in keys)
            {
                if (key.isLetter)
                {
                    key.setUppercase();
                }
                else
                {
                    key.setLowercase();
                }
            }
        }
        else if (shifted && !capslocked) // Everything should be uppercase
        {
            foreach (Key key in keys)
            {
                key.setUppercase();
            }
        }
        else // Everything should be lowercase
        {
            foreach (Key key in keys)
            {
                key.setLowercase();
            }
        }
    }


    /**
     * Description: Adds a blinking "cursor" to the end of the currently typed text. Called every 1 second.
     */
    void blinkCursor()
    {
        cursorOn = !cursorOn;
        displayText.text = cursorOn ? typed + "_" : typed;
    }

    /**
     * Sets the cooldown variable to false after a time delay
     **/
    private IEnumerator runCooldown()
    {
        yield return new WaitForSeconds(cooldownTime);
        cooldown = false;
    }

    /**
     * Selects new random sentences
     **/
    private void getSentences()
    {
        sentence = 0;
        shuffle<string>(sentences);

        practiceSentences[0] = sentences[0];
        practiceSentences[1] = sentences[1];
        for(int i = 2; i < 10; i++)
        {
            testSentences[i - 2] = sentences[i];
        }
        
    }

    /**
     * Description      : This method randomly shuffles the list of sentences
     *
     * Parameter - List : The list to be shuffled
     */
    void shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            var temp = list[i];

            int randomIndex = r.Next(list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    /**
     * Displays the next sentence and updates the related variables
     **/
    private void nextSentence()
    {
        start = DateTime.MinValue;
        // If the next sentence is a practice sentence
        if (sentence < practiceSentences.Length)
        {
            typeText.text = "Sentence " + (sentence + 1)  + "/" + (practiceSentences.Length + testSentences.Length) + "(PRACTICE)\n\n<color=#888888>" + typeCase + typeStart + "</color>" + practiceSentences[sentence];
            log.write_sentence(practiceSentences[sentence], "PRACTICE", sentence + 1);
            sentence++;
        }
        // If the next sentence is a test sentence
        else if (sentence < (testSentences.Length + practiceSentences.Length))
        {
            typeText.text = "Sentence " + (sentence + 1) + "/" + (practiceSentences.Length + testSentences.Length) + "(TEST)\n\n<color=#888888>" + typeCase + typeStart + "</color>" + testSentences[sentence - practiceSentences.Length];
            log.write_sentence(testSentences[sentence - practiceSentences.Length], "TEST", sentence + 1);
            sentence++;
        }
        // If there are no more sentences for the current case
        else if (sentence == testSentences.Length + practiceSentences.Length)
        {
            caseclosed.Invoke();
        }

    }

    /**
     * Displays the WPM and Error Rate of the sentence the user just entered
     **/
    private void showStats()
    {
        double WPM = calculateWPM();
        double error = calculateErrors();
        if (sentence <= 2)
            typeText.text = "Sentence " + sentence + "/" + (practiceSentences.Length + testSentences.Length) + 
                "(PRACTICE)\n\nERROR Rate: " + error.ToString("F1") + "%\nWPM: " + WPM.ToString("F1") + "\n\n<color=#888888>Sentence:</color>\n" +
                sentences[sentence - 1] + "\n<color=#888888>You typed:</color>\n" + prevtyped + "\n\nPress Enter to continue.";
        else
            typeText.text = "Sentence " + sentence + "/" + (practiceSentences.Length + testSentences.Length) + 
                "(TEST)\n\nERROR Rate: " + error.ToString("F1") + "%\nWPM: " + WPM.ToString("F1") + "\n\n<color=#888888>Sentence:</color>\n" +
                sentences[sentence - 1] + "\n<color=#888888>You typed:</color>\n" + prevtyped + "\n\nPress Enter to continue.";
        wpm[sentence - 1] = WPM;
        errorRate[sentence - 1] = error;

        if(sentence <= 2)
        {
            log.write_sentence_stats(prevtyped, WPM, error);
        } else if(sentence > 2 && sentence < 10)
        {
            log.write_sentence_stats(prevtyped, WPM, error);
        }
    }

    /**
     * Returns the WPM of the sentence the user just typed
     **/
    private double calculateWPM()
    {
        double length = prevtyped.Length / 5.0;
        return length / ((end - start).TotalSeconds / 60.0);
    }

    /**
     * Returns the Error Rate % of the sentence the user just typed
     **/
    private double calculateErrors()
    {
        string s = "";
        if(sentence <= 2)
        {
            s = practiceSentences[sentence - 1];
        } 
        else if (sentence > 2 && sentence < 10)
        {
            s = testSentences[sentence - 1 - practiceSentences.Length];
        }

        int[,] distance = new int[prevtyped.Length + 1, s.Length + 1]; // might not need +1
        for (int i = 0; i <= prevtyped.Length; i++)
            distance[i, 0] = i;
        for (int j = 1; j <= s.Length; j++)
            distance[0, j] = j;

        for (int i = 1; i <= prevtyped.Length; i++)
            for (int j = 1; j <= s.Length; j++)
                distance[i, j] = minimum(
                    distance[i - 1, j] + 1,
                    distance[i, j - 1] + 1,
                    distance[i - 1, j - 1]
                    + ((prevtyped[i - 1] == s[j - 1]) ? 0 : 1));

        return (distance[prevtyped.Length, s.Length] / (double) prevtyped.Length) * 100.0;
    }

    /**
     * Returns the minimum of three integers
     **/
    private int minimum(int a, int b, int c)
    {
        return Math.Min(Math.Min(a, b), c);
    }

    /**
     * Returns what the user just typed
     **/
    public string getTypedText()
    {
        return prevtyped;
    }

    public void setTypeCase(string s)
    {
        typeCase = s;
    }

    /**
     * Calculates and returns the average WPM of the sentences in the current case
     **/
    public double getAverageWPM()
    {
        double average = 0;
        foreach (double d in wpm)
        {
            average += d;
        }

        return average / wpm.Length;
    }

    /**
     * Calculates and returns the average Error Rate % of the sentences in the current case
     **/
    public double getAverageErrorRate()
    {
        double average = 0;
        foreach(double d in errorRate)
        {
            average += d;
        }

        return average / errorRate.Length;
    }
}
