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
    public AudioClip minigameLaneDodgeBGM;
    public AudioClip minigameDetangleBGM;

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
    // The default volume would just be set to the current volume of the sources
    [System.NonSerialized]
    public float SFXDefaultVolume;
    [System.NonSerialized]
    public float BGMDefaultVolume;

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

        SFXDefaultVolume = SFXSource.volume;
        BGMDefaultVolume = BGMSource.volume;
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
        UnityEngine.Debug.Log(sceneName); // for testing

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
        else if (sceneName == "MiniGame_LaneDodge")
        {
            Debug.Log("play Lane Dodge bgm");
            BGMSource.clip = minigameLaneDodgeBGM;
        }
        else if (sceneName == "MiniGame_Detangle")
        {
            Debug.Log("play Detangle bgm");
            BGMSource.clip = minigameDetangleBGM;
        }
        else
        {
            Debug.Log("No bgm");
            return;
        }
        toggleBGM();
    }

    public void toggleBGM(string currentScene = null)
    {
        if (currentScene != null) // For minigames that doesn't function normally because it's loaded weirdly
        {
            if (currentScene == "Wordle") { BGMSource.clip = minigameWordleBGM; }
            else if (currentScene == "LaneDodge") { BGMSource.clip = minigameLaneDodgeBGM; }
            else if (currentScene == "Detangle") { BGMSource.clip = minigameDetangleBGM; }
        }
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

    public void setSFXDefaultVol() => SFXSource.volume = SFXDefaultVolume;
    public void setBGMDefaultVol() => BGMSource.volume = BGMDefaultVolume;
    // Change volume percentage
    public void setSFXVol(float volumePer) => SFXSource.volume = SFXDefaultVolume * volumePer;
    public void setBGMVol(float volumePer) => BGMSource.volume = BGMDefaultVolume * volumePer;

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
