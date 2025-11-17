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

    [Header("Soundtracks")]
    public AudioClip menuBGM;
    public AudioClip streamSceneBGM;
    public AudioClip minigameClickerBGM;
    public AudioClip minigameWordleBGM;
    
    [Header("Sound effects")]
    public AudioClip textBeep;
    public AudioClip onEvent;
    public AudioClip openSideCard;
    public AudioClip closeSideCard;
    public AudioClip chooseEvent;
    public AudioClip select;
    public AudioClip catMeow;
    public AudioClip winMinigame;
    public AudioClip loseMinigame;
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

    /// <summary>
    /// Check the current scene and toggle BGM for respective scene automatically
    /// </summary>
    public void checkScene()
    {
        // Get the name of the current scene
        string sceneName = SceneManager.GetActiveScene().name;
        // UnityEngine.Debug.Log(sceneName); for testing

        if (sceneName == "StreamScene")
        {
            Debug.Log("play stream bgm");
            BGMSource.clip = streamSceneBGM;
        }
        else if (sceneName == "MainMenu" || sceneName == "GameEnd")
        {
            Debug.Log("play main menu bgm");
            BGMSource.clip = menuBGM;
        }
        else if (sceneName == "MiniGame_Clicker")
        {
            Debug.Log("play clicker bgm");
            BGMSource.clip = minigameClickerBGM;
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
    private void playSFX(AudioClip clip)
    {
        SFXSource.PlayOneShot(clip);
    }
    public void PlayOnEvent() => playSFX(onEvent);
    public void PlayOpenSideCard() => playSFX(openSideCard);
    public void PlayCloseSideCard() => playSFX(closeSideCard);
    public void PlayChooseEvent() => playSFX(chooseEvent);
    public void PlaySelect() => playSFX(select);
    public void PlayCatMeow() => playSFX(catMeow);
    public void PlayBeep() => playSFX(textBeep);
    public void PlayWinMinigame() => playSFX(winMinigame);
    public void PlayLoseMinigame() => playSFX(loseMinigame);
}
