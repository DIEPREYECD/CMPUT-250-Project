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

    public AudioSource BGMSource, SFXSource;
    public AudioClip menuBGM, streamSceneBGM, onEvent, openSideCard, closeSideCard, chooseEvent, select, catMeow, textBeep;
    private AudioClip currentClip = null;
    private float defaultVolume;

    private void Awake()
    {
        // Struggling to decide whether I should put the AudioController object in every scene or just make it not destroy 
        // Doing the former because cat button in StreamScene need a object reference (Matthew L)

        // Destroy duplicates
        /* Destroy old
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        */
        // Destory new
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
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
            BGMSource.clip = streamSceneBGM;
        }
        else if (sceneName == "MainMenu")
        {
            Debug.Log("play main menu bgm");
            BGMSource.clip = menuBGM;
        }
        else if ( sceneName == "GameEnd")
        {
            Debug.Log("play game over bgm");
            BGMSource.clip = menuBGM; // Use the same track as main menu
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
        if (BGMSource.isPlaying && currentClip == BGMSource.clip)
        {
            Debug.Log("Stop BGM");
            BGMSource.Stop();
            return; // Do nothing if it's already playing
        }
        // Set the AudioSource to loop
        BGMSource.loop = true;

        currentClip = BGMSource.clip;
        
        // Play the music
        BGMSource.Play();
    }

    public void PlayOnEvent()
    {
        SFXSource.PlayOneShot(onEvent);
    }

    public void PlayOpenSideCard()
    {
        SFXSource.PlayOneShot(openSideCard);
    }

    public void PlayChooseEvent()
    {
        SFXSource.PlayOneShot(chooseEvent);
    }
    public void PlaySelect()
    {
        SFXSource.PlayOneShot(select);
    }
    public void PlayCatMeow()
    {
        SFXSource.PlayOneShot(catMeow);
    }
    public void PlayBeep()
    {
        SFXSource.PlayOneShot(textBeep);
    }
}
