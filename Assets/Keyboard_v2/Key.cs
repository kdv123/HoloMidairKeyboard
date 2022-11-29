using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/**
 * Author      : Cecilia Schmitz
 * Email       : cmschmit@mtu.edu
 * Description : Contains all methods for keys (used in both keyboards)
 */
public class Key : MonoBehaviour
{

    [TextArea] public string textOnLowercase; // Text to display if lowercase
    [TextArea] public string textOnUppercase; // Text to display if uppercase
    public char valueOnLowercase; // Value to return if lowercase
    public char valueOnUppercase; // Value to return if uppercase
    public bool isLetter; // Whether or not the key is a letter
    private AudioSource audio; // Audio clip to play when pressed

    bool islower = true; // Whether or not the key is currently uppercased or lowercased

    public TextMeshPro tmp; // The TextMeshPro displaying the key text
    public Renderer activeLight; // The light to activate when the key is pressed


    void Start()
    {
        // Text should initialize as lowercase
        tmp.text = textOnLowercase;
        audio = GetComponent<AudioSource>();
        setInactive();
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

    /**
     * Description: Plays an audioclip when a key is pressed
     */
    public void playAudio()
    {
        if (audio != null)
        {
            audio.Play();
        }
    }

    /**
     * Description: "Activates" the surface light of the key, providing visual feedback to the user
     */
    public void setActive()
    {
        Color c = activeLight.material.color;
        c.a = 255;
        activeLight.material.color = c;
    }

    /**
     * Description: "Deactivates" the surface light of the key, providing visual feedback to the user
     */
    public void setInactive()
    {
        Color c = activeLight.material.color;
        c.a = 0;
        activeLight.material.color = c;
    }
}
