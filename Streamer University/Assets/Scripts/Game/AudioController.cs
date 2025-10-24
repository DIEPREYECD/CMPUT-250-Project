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
    public AudioClip menuBGM, streamSceneBGM, onEvent, openSideCard, chooseEvent, select, catMeow;
    private AudioClip currentClip = null;

    private void Awake()
    {
        // Struggling to decide whether I should put the AudioController object in every scene or just make it not destroy 
        // Doing the former because cat button in StreamScene need a object reference (Matthew L)

        // Destroy duplicates
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
    

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
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
        else if (sceneName == "GameWin")
        {
            Debug.Log("play game over bgm");
            audioSource.pitch = 1.3f;
            audioSource.clip = menuBGM; // Use the same track as main menu
        }
        else if ( sceneName == "GameOver")
        {
            Debug.Log("play game over bgm");
            audioSource.pitch = 0.7f;
            audioSource.clip = menuBGM; // Use the same track as main menu
        }
        else
        {
            Debug.Log("No bgm");
            return;
        }
        toggleBGM();
    }
    
    public void toggleBGM()
    {
        // Check if the BGM is already playing to avoid restarting it every time
        if (audioSource.isPlaying && currentClip == audioSource.clip)
        {
            Debug.Log("Stop BGM");
            return; // Do nothing if it's already playing
        }
        // Set the AudioSource to loop
        audioSource.loop = true;

        currentClip = audioSource.clip;
        
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
    public void PlaySelect()
    {
        audioSource.PlayOneShot(select);
    }
    public void PlayCatMeow()
    {
        audioSource.PlayOneShot(catMeow);
    }
}
