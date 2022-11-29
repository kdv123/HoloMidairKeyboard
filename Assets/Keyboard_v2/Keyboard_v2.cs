using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Keyboard_v2 : MonoBehaviour
{

    public Key[] keys = new Key[48]; // Array to store all Keys.
    public Key backspace;
    public Key capslock;
    public Key leftshift;
    public Key rightshift;
    public Key spacebar;
    public bool multifinger;
    public float defaultThreshold = 0.5f;
    public float spacebarThreshold = 0.9f;
    public float cooldownTime = 0.5f;

    public TextMeshPro displayText;

    string typed = "";
    char value;
    bool shifted = false;
    bool capslocked = false;
    bool cursorOn = false;
    bool cooldown = false;


    private void Start()
    {
        InvokeRepeating("blinkCursor", 0f, 0.75f);
        if (multifinger)
        {
            foreach (Key key in keys)
            {
                key.GetComponent<Key_Multifinger>().setThreshold(defaultThreshold);
            }
            spacebar.GetComponent<Key_Multifinger>().setThreshold(spacebarThreshold);
        }
    }



    /**
     * Description : Called when a key is pressed. Appends the value of that key to the current typed string.
     */
    public void keyPressed(Key key)
    {
        if (!cooldown)
        {
            value = key.getValue();
            typed += value;
            displayText.text = cursorOn ? typed + "_" : typed;
            key.playAudio();

            if (shifted) updateShift();
            key.setActive();

            cooldown = true;
            StartCoroutine("runCooldown");
        }
    }


    /**
     * Description : Removes one character from the typed string (called when backspace key is pressed)
     */
    public void backspacePressed()
    {
        if(!cooldown)
        {
            typed = typed.Substring(0, typed.Length - 1);
            displayText.text = cursorOn ? typed + "_" : typed;
            backspace.playAudio();
            backspace.setActive();

            cooldown = true;
            StartCoroutine("runCooldown");
        }
        
    }


    /**
     * Description : Toggles the value of the shift key(S) (called when either shift key is pressed)
     */
    public void updateShift()
    {
        if (!cooldown)
        {
            shifted = !shifted;
            updateCase();
            leftshift.playAudio();

            cooldown = true;
            StartCoroutine("runCooldown");
        }
    }


    /**
     * Description : Toggles the value of the caps lock key (called when the caps lock key is pressed)
     */
    public void updateCaps()
    {
        if (!cooldown)
        {
            capslocked = !capslocked;
            updateCase();
            capslock.playAudio();

            cooldown = true;
            StartCoroutine("runCooldown");
        }
    }


    /**
     * Description : Updates the case of all keys depending on the current values of the shift & caps lock keys
     */
    void updateCase()
    {
        if (shifted && capslocked) // letters should be lowercase, everything else should be uppercase
        {
            foreach(Key key in keys)
            {
                if(key.isLetter)
                {
                    key.setLowercase();
                } else
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
            foreach(Key key in keys)
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

    private IEnumerator runCooldown()
    {
        yield return new WaitForSeconds(cooldownTime);
        cooldown = false;
    }
}
