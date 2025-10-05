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
        audioSource.PlayOneShot(bgm);
    }

    public void PlayChooseEvent()
    {
        audioSource.PlayOneShot(chooseEvent);
    }
}
