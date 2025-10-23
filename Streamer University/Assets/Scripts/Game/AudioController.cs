using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

public class AudioController : MonoBehaviour
{
    private static AudioController _instance;
    public static AudioController Instance { get { return _instance; } }

    public AudioSource audioSource;
    public AudioClip menuBGM, streamSceneBGM, onEvent, openSideCard, chooseEvent;

    private void Awake()
    {
        _instance = this;
        checkScene();
    }

    private void checkScene()
    {
        // Get the name of the current scene
        string sceneName = SceneManager.GetActiveScene().name;
        
        if (sceneName == "StreamScene")
        {
            Debug.Log("play stream bgm");
            audioSource.clip = streamSceneBGM;
        }
        else if (sceneName == "MainMenu")
        {
            Debug.Log("play main menu bgm");
            audioSource.clip = menuBGM;
        }
        else if (sceneName == "GameWin" || sceneName == "GameOver")
        {
            Debug.Log("play game over bgm");
            audioSource.clip = menuBGM; // Use the same track as main menu
        }
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
