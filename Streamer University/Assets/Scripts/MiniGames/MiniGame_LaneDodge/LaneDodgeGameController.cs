using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LaneDodgeGameController : MiniGameController
{
    [Header("UI References")]
    [Tooltip("Slider representing remaining time until you win.")]
    public Slider timeRemainingBar;

    [Tooltip("Text showing how long the player has stayed alive.")]
    public TMP_Text timeAliveText;

    [Tooltip("Panel shown when the player loses (crashes).")]
    public GameObject losePanel;

    [Tooltip("Panel shown when the player wins (survives long enough).")]
    public GameObject winPanel;

    [Header("Intro UI")]
    [Tooltip("Full screen intro panel with rules + Start button.")]
    public GameObject introPanel;

    [Tooltip("Parent for HUD during gameplay (timer bar, etc.).")]
    public GameObject gameUIRoot;

    [Header("Timing")]
    [Tooltip("How long the player must survive to win (seconds).")]
    public float gameDuration = 20f;

    private float remainingTime;
    private float timeAlive;

    private bool gameStarted = false;

    [Header("Result Effects")]
    [Tooltip("Delta to fame if the player wins.")]
    public int fameOnWin = 15;

    [Tooltip("Delta to stress if the player wins.")]
    public int stressOnWin = -15;

    [Tooltip("Delta to fame if the player loses.")]
    public int fameOnLose = -20;

    [Tooltip("Delta to stress if the player loses.")]
    public int stressOnLose = 20;

    [Header("Audio")]
    [Tooltip("How much quieter SFX should be during this minigame (like clicker).")]
    public float soundEffectVolReduce = 0.4f;

    [Header("Low Time Warning")]
    [Tooltip("When remaining time is below this, the time bar will blink.")]
    public float lowTimeThreshold = 5f;

    [Tooltip("How fast the time bar should blink when low.")]
    public float lowTimeBlinkSpeed = 4f;

    private Image timeBarFillImage;
    private Color timeBarBaseColor;


    // Make this a singleton
    public static LaneDodgeGameController Instance { get; private set; }
    public bool IsGameFinished => finished;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // If you want this controller to survive scene loads, uncomment the next line:
            // DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Debug.LogWarning($"LaneDodgeGameController: Duplicate instance on '{gameObject.name}' - destroying duplicate.");
            Destroy(gameObject);
            return;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Start()
    {
        AudioController.Instance.toggleBGM("LaneDodge"); // Play BGM

        // Initialize the base fields
        this.delta = new Dictionary<string, int>();
        finished = false;
        successDeclared = false;

        remainingTime = gameDuration;
        timeAlive = 0f;
        gameStarted = false;

        // Setup initial UI state
        if (timeRemainingBar != null)
        {
            timeRemainingBar.minValue = 0f;
            timeRemainingBar.maxValue = 1f;
            timeRemainingBar.value = 1f; // full at start
        }

        if (timeRemainingBar != null && timeRemainingBar.fillRect != null)
        {
            timeBarFillImage = timeRemainingBar.fillRect.GetComponent<Image>();
            if (timeBarFillImage != null)
            {
                timeBarBaseColor = timeBarFillImage.color;
            }
        }

        if (timeAliveText != null)
            timeAliveText.text = "Time Alive: 0.0s";

        if (losePanel != null) losePanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);

        // Show intro, hide HUD
        if (introPanel != null) introPanel.SetActive(true);
        if (gameUIRoot != null) gameUIRoot.SetActive(false);

        // Make sure spawner is idle until we start
        if (LaneDodgeSpawner.Instance != null)
            LaneDodgeSpawner.Instance.StopSpawning();
    }

    private void Update()
    {
        if (!gameStarted || finished)
            return;

        // Update timers
        timeAlive += Time.deltaTime;
        remainingTime -= Time.deltaTime;
        if (remainingTime < 0f) remainingTime = 0f;

        // Update UI
        UpdateTimerUI();

        // Difficulty ramp based on survival time
        if (LaneDodgeSpawner.Instance != null)
        {
            float perf = GetPerformanceFactor(); // 0..1 based on timeAlive/gameDuration
            LaneDodgeSpawner.Instance.UpdateDifficulty(perf);
        }

        // Check win condition
        if (remainingTime <= 0f)
        {
            HandleWin();
        }
    }

    private void UpdateTimerUI()
    {
        float normalized = gameDuration > 0f ? (remainingTime / gameDuration) : 0f;

        if (timeRemainingBar != null)
            timeRemainingBar.value = normalized;

        if (timeAliveText != null)
            timeAliveText.text = $"Time Alive: {timeAlive:F1}s";

        // Low-time blinking effect
        if (timeBarFillImage != null)
        {
            bool shouldBlink = gameStarted && !finished && remainingTime > 0f && remainingTime <= lowTimeThreshold;

            if (shouldBlink)
            {
                // Ping-pong between base color and black to simulate blinking
                float t = Mathf.PingPong(Time.time * lowTimeBlinkSpeed, 1f);
                timeBarFillImage.color = Color.Lerp(timeBarBaseColor, Color.black, t);
            }
            else
            {
                // Ensure we reset to the original color when not blinking
                timeBarFillImage.color = timeBarBaseColor;
            }
        }

    }

    // Called by Start button on IntroPanel
    public void StartGame()
    {
        if (gameStarted || finished)
            return;

        // Audio similar to clicker
        if (AudioController.Instance != null)
        {
            AudioController.Instance.PlaySelect();
            AudioController.Instance.SFXSource.volume -= soundEffectVolReduce;
        }


        gameStarted = true;
        finished = false;
        successDeclared = false;

        remainingTime = gameDuration;
        timeAlive = 0f;
        UpdateTimerUI();

        if (LaneDodgePlayerController.Instance != null)
            LaneDodgePlayerController.Instance.ResetVisual();

        if (introPanel != null)
            introPanel.SetActive(false);

        if (gameUIRoot != null)
            gameUIRoot.SetActive(true);

        if (LaneDodgeSpawner.Instance != null)
            LaneDodgeSpawner.Instance.StartSpawning();
    }

    // Called when the player collides with a pickup (clock) to add time
    public void AddTime(float amount)
    {
        if (!gameStarted || finished)
            return;

        remainingTime += amount;
        if (remainingTime > gameDuration)
            remainingTime = gameDuration;

        UpdateTimerUI();
    }

    // Called when the player hits an obstacle (car/truck/trash can)
    public void OnPlayerHitObstacle()
    {
        Debug.Log("LaneDodgeGameController: Player hit an obstacle, handling loss.");
        if (finished)
            return;

        successDeclared = false;
        finished = true;
        gameStarted = false;

        if (LaneDodgePlayerController.Instance != null)
            LaneDodgePlayerController.Instance.SetKnockedDown();


        LaneDodgeSpawner.Instance.StopSpawning();

        AudioController.Instance.toggleBGM(); // stop BGM
        AudioController.Instance.SFXSource.volume += soundEffectVolReduce; // restore SFX volume
        AudioController.Instance.PlayLoseMinigame();

        if (losePanel != null)
            losePanel.SetActive(true);

        // Brief delay so player can see result
        Invoke(nameof(FinishMiniGame), 4f);
    }

    private void HandleWin()
    {
        if (finished)
            return;

        Debug.Log("LaneDodgeGameController: Player survived long enough, handling win.");
        successDeclared = true;
        finished = true;
        gameStarted = false;

        if (LaneDodgeSpawner.Instance != null)
            LaneDodgeSpawner.Instance.StopSpawning();

        if (AudioController.Instance != null)
        {
            AudioController.Instance.toggleBGM(); // stop BGM
            AudioController.Instance.SFXSource.volume += soundEffectVolReduce; // restore SFX volume
            AudioController.Instance.PlayWinMinigame();
        }

        if (winPanel != null)
            winPanel.SetActive(true);

        Invoke(nameof(FinishMiniGame), 4f);
    }

    public override void FinishMiniGame()
    {
        bool success = successDeclared;
        Debug.Log($"LaneDodgeGameController: Finishing minigame, success = {success}");

        this.delta = new Dictionary<string, int>();
        var setFlags = new List<string>();

        // 0..1 based on how long they stayed alive
        float perf = GetPerformanceFactor();

        // Fame:
        //   perf = 0  -> fameOnLose
        //   perf = 1  -> fameOnWin
        int fameDelta = Mathf.RoundToInt(Mathf.Lerp(fameOnLose, fameOnWin, perf));

        // Stress:
        //   perf = 0  -> stressOnLose (worst)
        //   perf = 1  -> stressOnWin  (best / least stress)
        int stressDelta = Mathf.RoundToInt(Mathf.Lerp(stressOnLose, stressOnWin, perf));

        if (fameDelta != 0)
            this.delta["fame"] = fameDelta;
        if (stressDelta != 0)
            this.delta["stress"] = stressDelta;

        // Flags + story events still depend on win/lose
        if (success)
        {
            setFlags.Add("laneDodgeWin");
            if (EventManager.Instance != null)
                EventManager.Instance.addToQueue("EVT_LANEDODGE_WIN");
        }
        else
        {
            setFlags.Add("laneDodgeLose");
            if (EventManager.Instance != null)
                EventManager.Instance.addToQueue("EVT_LANEDODGE_LOSE");
        }

        if (EventManager.Instance != null)
            EventManager.Instance.setFlags(setFlags);

        var result = new MiniGameResult
        {
            success = success,
            delta = this.delta
        };

        if (resultChannel != null)
            resultChannel.Raise(result);
    }

    public bool IsGameStarted()
    {
        return gameStarted;
    }
    private float GetPerformanceFactor()
    {
        // 0.0 = instantly died, 1.0 = survived full gameDuration
        if (gameDuration <= 0f)
            return 0f;

        float normalized = Mathf.Clamp01(timeAlive / gameDuration);
        return normalized;
    }

}
