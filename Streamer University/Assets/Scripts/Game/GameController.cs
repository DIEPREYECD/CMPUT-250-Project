using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// serializable struct for setting the conditions for each Game Ending
[System.Serializable]
public struct GameEndingCondition
{
    public GameEndings ending;   // The specific game ending
    public float minFame;
    public float maxFame;
    public float minStress;
    public float maxStress;
}

public class GameController : MonoBehaviour
{
    // ====== Data ======
    [Header("Data")]

    // ====== UI Refs ======
    [Header("UI")]
    [SerializeField] private RectTransform playerAvatar;  // UI avatar RectTransform (Image)
    [SerializeField] private BarUI stressBar;
    [SerializeField] private BarUI fameBar;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private GameObject pauseMenuOverlay;
    [SerializeField] private Button quitButton; // assign in inspector

    // ====== Timing ======
    [Header("Timing")]
    [Tooltip("Seconds between storylet prompts while streaming.")]
    [SerializeField] private float secondsBetweenEvents = 5f;

    // ====== Avatar Layout Targets ======
    [Header("Avatar Layout (Anchors & Size)")]
    [Tooltip("Where the avatar sits during normal streaming (center).")]
    [SerializeField] private RectTarget avatarCenter = RectTarget.Center();
    [Tooltip("Where the avatar moves while event cards are visible (bottom-left).")]
    [SerializeField] private RectTarget avatarDuringChoices = RectTarget.BottomLeft(new Vector2(20, 20), new Vector2(200, 280));
    [SerializeField] private float avatarTweenTime = 0.25f;

    private Coroutine loopCoro;

    [Header("Game Ending Conditions")]
    // List of conditions for triggering each game ending
    /*
        Each condition can specify min/max fame and stress.
        A value of -1 means that condition is not checked.

        The example game endings are:
        No Fame Ending: Fame <= minFame
        Max Stress Ending: Stress >= maxStress
        Max Fame Low Stress Ending: Fame >= maxFame AND Stress <= minStress
        Max Fame High Stress Ending: Fame >= maxFame AND Stress >= maxStress

        Put a note that is shown in the inspector for clarity.
    */
    [Tooltip("Set the conditions for triggering each game ending. Use -1 to ignore a condition. E.g., to trigger No Fame Ending, set minFame to 0 and others to -1.")]
    public List<GameEndingCondition> endingConditions;

    private static GameController _instance;
    public static GameController Instance { get { return _instance; } }

    private void Awake()
    {
        Assert.IsNotNull(playerAvatar, "Assign PlayerAvatar RectTransform (UI).");
        Assert.IsNotNull(stressBar, "Assign Stress Bar.");
        Assert.IsNotNull(fameBar, "Assign Fame Bar.");

        _instance = this;
    }

    private void Subscribe()
    {
        // Optional: react to event open/close to move avatar exactly on those moments.
        if (EventManager.Instance != null)
        {
            Debug.Log("Subscribing to EventManager events.");
            EventManager.Instance.OnEventOpened += HandleEventOpened;
            EventManager.Instance.OnEventClosed += HandleEventClosed;
        }
        // Subscribe to player stat changes to display deltas when they occur
        if (PlayerController.Instance != null)
        {
            Debug.Log("Subscribing to PlayerController stat events.");
            PlayerController.Instance.OnStatsChanged += HandleStatsChanged;
        }
    }

    private void Unsubscribe()
    {
        if (EventManager.Instance != null)
        {
            Debug.Log("Unsubscribing from EventManager events.");
            EventManager.Instance.OnEventOpened -= HandleEventOpened;
            EventManager.Instance.OnEventClosed -= HandleEventClosed;
        }
        if (PlayerController.Instance != null)
        {
            Debug.Log("Unsubscribing from PlayerController stat events.");
            PlayerController.Instance.OnStatsChanged -= HandleStatsChanged;
        }
    }

    private void Start()
    {
        Subscribe();

        Debug.Log("Welcome to Streamer U!");
        PlayerController.Instance.ResetStats();
        Debug.Log($"Starting Fame: {PlayerController.Instance.Fame}, Stress: {PlayerController.Instance.Stress}");

        // Put avatar in the starting pose/anchors
        avatarCenter.ApplyTo(playerAvatar);

        // Initialize bars to current player values (avoid relying on per-frame polling)
        if (fameBar != null)
            fameBar.SetFill(PlayerController.Instance.Fame / 100f);
        if (stressBar != null)
            stressBar.SetFill(PlayerController.Instance.Stress / 100f);

        GameFlowController.Instance.SetState(GameState.MainGameplay);
        // Kick the loop
        loopCoro = StartCoroutine(StreamLoop());

        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(() =>
            {
                if (GameFlowController.Instance.CurrentState == GameState.MainGameplay)
                {
                    GameFlowController.Instance.SetState(GameState.Paused);
                    Time.timeScale = 0f;
                    if (pauseMenuOverlay != null)
                        pauseMenuOverlay.SetActive(true);
                }
            });

            pauseMenuOverlay.SetActive(false);
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(() =>
            {
                UnPauseGame();
            });
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(() =>
            {
                QuitGame();
            });
        }
    }

    public static void UnPauseGame()
    {
        if (GameFlowController.Instance.CurrentState == GameState.Paused)
        {
            GameFlowController.Instance.SetState(GameState.MainGameplay);
            Time.timeScale = 1f;
            if (Instance.pauseMenuOverlay != null)
                Instance.pauseMenuOverlay.SetActive(false);
        }
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void Update()
    {
        if (GameFlowController.Instance.CurrentState != GameState.MainGameplay)
            return;

        // Bars are updated via PlayerController.OnStatsChanged event (no per-frame polling)

        // Check for game ending conditions
        foreach (var condition in endingConditions)
        {
            /*
                If the min value is -1 and max value is -1 it means that condition is not checked.
                The possible game endings are:

                No Fame Ending: Fame <= minFame
                Max Stress Ending: Stress >= maxStress
                Max Fame High Stress Ending: Fame >= maxFame AND Stress >= maxStress
                Max Fame Low Stress Ending: Fame >= maxFame AND Stress <= minStress
            */
            bool check = true;
            if (condition.minFame != -1.0f)
                check &= PlayerController.Instance.Fame <= condition.minFame;
            if (condition.maxFame != -1.0f)
                check &= PlayerController.Instance.Fame >= condition.maxFame;
            if (condition.minStress != -1.0f)
                check &= PlayerController.Instance.Stress <= condition.minStress;
            if (condition.maxStress != -1.0f)
                check &= PlayerController.Instance.Stress >= condition.maxStress;


            if (check)
            {
                Debug.Log($"Game Ending Triggered: {condition.ending}");
                GameFlowController.Instance.SetEnding(condition.ending);
                GameFlowController.Instance.SetState(GameState.GameEnd);
                Unsubscribe();
                if (loopCoro != null)
                    StopCoroutine(loopCoro);
                GameFlowController.Instance.TransitionToScene("GameEnd");
                break;
            }
        }
    }

    // ---- Player stat change hooks ----
    private void HandleStatsChanged(PlayerController.StatsDelta d)
    {
        StartCoroutine(ShowDeltasCoroutine(d));
    }

    private IEnumerator ShowDeltasCoroutine(PlayerController.StatsDelta d)
    {
        // Show fame delta first
        if (fameBar != null)
        {
            // Don't show if delta is zero
            if (d.deltaFame != 0)
                fameBar.ShowDelta(d.deltaFame);
            fameBar.SetFill(d.newFame / 100f);
        }

        // small stagger so the player perceives separate changes
        yield return new WaitForSeconds(0.12f);

        if (stressBar != null)
        {
            // Don't show if delta is zero
            if (d.deltaStress != 0)
                stressBar.ShowDelta(d.deltaStress);
            stressBar.SetFill(d.newStress / 100f);
        }
    }

    private IEnumerator StreamLoop()
    {
        while (true)
        {
            // If a minigame is active for any reason, wait here
            while (GameFlowController.Instance.CurrentState != GameState.MainGameplay)
                yield return null;

            // Wait before next event prompt (but don't overlap if one is already showing)
            float t = 0f;
            while (t < secondsBetweenEvents)
            {
                if (!EventManager.Instance.IsShowingEvent &&
                    GameFlowController.Instance.CurrentState == GameState.MainGameplay)
                    t += Time.deltaTime;
                yield return null;
            }

            // Ask EventManager to present the next storylet
            EventManager.Instance.ShowNextEvent();     // -> EventManager sets IsShowingEvent=true and spawns UI
        }
    }

    // ---- EventManager hooks (optional but nice for polish) ----
    private void HandleEventOpened()
    {
        // Slide avatar to make space for cards
        StartCoroutine(MoveAvatar(avatarDuringChoices));
    }

    private void HandleEventClosed()
    {
        // Return avatar to normal spot
        StartCoroutine(MoveAvatar(avatarCenter));
    }

    private IEnumerator MoveAvatar(RectTarget target)
    {
        var rt = playerAvatar;
        var start = RectTarget.From(rt);
        var t = 0f;
        while (t < avatarTweenTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / avatarTweenTime);
            RectTarget.Lerp(start, target, k).ApplyTo(rt);
            yield return null;
        }
        target.ApplyTo(rt);
    }

    // ====== Helper struct for RectTransform targets ======
    [System.Serializable]
    private struct RectTarget
    {
        public Vector2 anchorMin, anchorMax, pivot, anchoredPos, sizeDelta;

        public static RectTarget From(RectTransform rt) => new RectTarget
        {
            anchorMin = rt.anchorMin,
            anchorMax = rt.anchorMax,
            pivot = rt.pivot,
            anchoredPos = rt.anchoredPosition,
            sizeDelta = rt.sizeDelta
        };

        public void ApplyTo(RectTransform rt)
        {
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
        }

        public static RectTarget Lerp(RectTarget a, RectTarget b, float t)
        {
            return new RectTarget
            {
                anchorMin = Vector2.Lerp(a.anchorMin, b.anchorMin, t),
                anchorMax = Vector2.Lerp(a.anchorMax, b.anchorMax, t),
                pivot = Vector2.Lerp(a.pivot, b.pivot, t),
                anchoredPos = Vector2.Lerp(a.anchoredPos, b.anchoredPos, t),
                sizeDelta = Vector2.Lerp(a.sizeDelta, b.sizeDelta, t),
            };
        }

        // Common presets
        public static RectTarget Center()
        {
            return new RectTarget
            {
                anchorMin = new Vector2(0.5f, 0.5f),
                anchorMax = new Vector2(0.5f, 0.5f),
                pivot = new Vector2(0.5f, 0.5f),
                anchoredPos = Vector2.zero,
                sizeDelta = new Vector2(200, 300)
            };
        }

        public static RectTarget BottomLeft(Vector2 offset, Vector2 size)
        {
            return new RectTarget
            {
                anchorMin = new Vector2(0f, 0f),
                anchorMax = new Vector2(0f, 0f),
                pivot = new Vector2(0f, 0f),
                anchoredPos = offset,   // e.g., (20,20)
                sizeDelta = size        // e.g., (200,280)
            };
        }
    }
}
