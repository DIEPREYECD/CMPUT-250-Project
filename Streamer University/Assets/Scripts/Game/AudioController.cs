using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    private static AudioController _instance;
    public static AudioController Instance { get { return _instance; } }

    public AudioSource audioSource;
    public AudioClip bgm, chooseEvent;

    private void Awake()
    {
        _instance = this;
        toggleBGM();
    }

    public void toggleBGM()
    {
    // Check if the BGM is already playing to avoid restarting it every time
    if (audioSource.isPlaying)
    {
        Debug.Log("Stop BGM");
        return; // Do nothing if it's already playing
    }

    // Assign the BGM clip to the AudioSource
    audioSource.clip = bgm;
    
    // Set the AudioSource to loop
    audioSource.loop = true;
    
    // Play the music
    audioSource.Play();
    }

    public void PlayChooseEvent()
    {
        audioSource.PlayOneShot(chooseEvent);
    }
}
