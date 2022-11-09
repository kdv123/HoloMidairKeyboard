using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Key : MonoBehaviour
{

    [TextArea] public string textOnLowercase; // Text to display if lowercase
    [TextArea] public string textOnUppercase; // Text to display if uppercase
    public char valueOnLowercase; // Value to return if lowercase
    public char valueOnUppercase; // Value to return if uppercase
    public bool isLetter; // Whether or not the key is a letter

    bool islower = true; // Whether or not the key is currently uppercased or lowercased

    public TextMeshPro tmp; // The TextMeshPro displaying the key text


    void Start()
    {
        // Text should initialize as lowercase
        tmp.text = textOnLowercase;
    }

    /**
     * Description: Sets key text to appear lowercase and return lowercase values if pushed.
     */
    public void setLowercase()
    {
        tmp.text = textOnLowercase;
        islower = true;
    }

    /**
     * Description: Sets key text to appear uppercase and return uppercase values if pushed.
     */
    public void setUppercase()
    {
        tmp.text = textOnUppercase;
        islower = false;
    }

    /**
     * Description: Returns the current value of the key (uppercase or lowercase value)
     */
    public char getValue()
    {
        return islower ? valueOnLowercase : valueOnUppercase;
    }
}
