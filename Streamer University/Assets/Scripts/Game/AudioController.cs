using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    private static AudioController _instance;
    public static AudioController Instance { get { return _instance; } }

    public AudioSource audioSource;
    public AudioClip menuBGM, streamSceneBGM, onEvent, openSideCard, chooseEvent;

    private void Awake()
    {
        _instance = this;
    }
    
    public void toggleBGM(string scene)
    {
        // Check if the BGM is already playing to avoid restarting it every time
        if (audioSource.isPlaying)
        {
            Debug.Log("Stop BGM");
            return; // Do nothing if it's already playing
        }

        // Assign the BGM clip to the AudioSource
        if (scene == "stream")
        {
            audioSource.clip = streamSceneBGM;
        } else if (scene == "menu")
        {
            audioSource.clip = menuBGM;
        }


    // Set the AudioSource to loop
    audioSource.loop = true;
    
    // Play the music
    audioSource.Play();
    }

    public void PlayOnEvent()
    {
        audioSource.PlayOneShot(onEvent);
    }

    public void PlayOpenSideCard()
    {
        audioSource.PlayOneShot(openSideCard);
    }

    public void PlayChooseEvent()
    {
        audioSource.PlayOneShot(chooseEvent);
    }
}
