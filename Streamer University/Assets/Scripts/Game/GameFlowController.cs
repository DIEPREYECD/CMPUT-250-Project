using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameFlowController : MonoBehaviour
{
    public static GameFlowController Instance { get; private set; }
    public GameState CurrentState { get; private set; } = GameState.Title;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetState(GameState next) => CurrentState = next;

    public void Start()
    {
        
    }

    private void Update()
    {
        
    }
}

public enum GameState { Title, MainMenu, MainGameplay, Minigame, Results, Saving }
