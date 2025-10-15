using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    // ====== Data ======
    [Header("Data")]
    [SerializeField] private PlayerStatsSO playerStats;   // ScriptableObject instance

    // ====== UI Refs ======
    [Header("UI")]
    [SerializeField] private RectTransform playerAvatar;  // UI avatar RectTransform (Image)
    [SerializeField] private BarUI stressBar;
    [SerializeField] private BarUI fameBar;

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

    // ====== Internal State ======
    public enum GCState { Streaming, ShowingChoices, Resolving }
    private GCState state = GCState.Streaming;
    private Coroutine loopCoro;

    private static GameController _instance;
    public static GameController Instance { get { return _instance; } }

    private void Awake()
    {
        Assert.IsNotNull(playerStats, "Assign PlayerStats on GameController.");
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
    }

    private void Unsubscribe()
    {
        if (EventManager.Instance != null)
        {
            Debug.Log("Unsubscribing from EventManager events.");
            EventManager.Instance.OnEventOpened -= HandleEventOpened;
            EventManager.Instance.OnEventClosed -= HandleEventClosed;
        }
    }

    private void Start()
    {
        Subscribe();

        Debug.Log("Welcome to Streamer U!");
        playerStats.ResetStats();
        Debug.Log($"Starting Fame: {playerStats.Fame}, Stress: {playerStats.Stress}");

        // Put avatar in the starting pose/anchors
        avatarCenter.ApplyTo(playerAvatar);

        // Kick the loop
        loopCoro = StartCoroutine(StreamLoop());
    }

    private void Update()
    {
        stressBar.SetFill(playerStats.Stress / 100f);
        fameBar.SetFill(playerStats.Fame / 100f);

        if (Input.GetKeyDown(KeyCode.S) && !MiniGameLoader.Instance.isRunningGame())
            MiniGameLoader.Instance.LaunchMiniGame("MiniGame_Clicker");
    }

    private IEnumerator StreamLoop()
    {
        while (true)
        {
            state = GCState.Streaming;

            // Wait before next event prompt (but don't overlap if one is already showing)
            float t = 0f;
            while (t < secondsBetweenEvents)
            {
                if (!EventManager.Instance.IsShowingEvent) t += Time.deltaTime;
                yield return null;
            }

            // Ask EventManager to present the next storylet
            EventManager.Instance.ShowNextEvent();     // -> EventManager sets IsShowingEvent=true and spawns UI
            state = GCState.ShowingChoices;

            // Wait until the player chooses and EventManager closes the UI
            while (EventManager.Instance.IsShowingEvent)
                yield return null;

            state = GCState.Resolving;

            // End conditions (simple sample)
            if (playerStats.Fame <= 0 || playerStats.Stress >= 100)
            {
                Debug.Log("Stream ended: burnout or lost all fame.");
                Debug.Log($"Final Fame: {playerStats.Fame}, Final Stress: {playerStats.Stress}");
                SceneManager.LoadScene("GameOver");
            }
            else if (playerStats.Fame >= 100)
            {
                Debug.Log("Stream ended: reached maximum fame! You win!");
                Debug.Log($"Final Fame: {playerStats.Fame}, Final Stress: {playerStats.Stress}");
                SceneManager.LoadScene("GameWin");
            }
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

    private void QuitGame()
    {
        Debug.Log("Quitting game...");
        Unsubscribe();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
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
