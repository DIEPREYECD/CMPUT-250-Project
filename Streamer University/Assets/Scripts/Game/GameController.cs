using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

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
        PlayerController.Instance.ResetStats();
        Debug.Log($"Starting Fame: {PlayerController.Instance.Fame}, Stress: {PlayerController.Instance.Stress}");

        // Put avatar in the starting pose/anchors
        avatarCenter.ApplyTo(playerAvatar);

        GameFlowController.Instance.SetState(GameState.MainGameplay);
        // Kick the loop
        loopCoro = StartCoroutine(StreamLoop());
    }

    private void Update()
    {
        stressBar.SetFill(PlayerController.Instance.Stress / 100f);
        fameBar.SetFill(PlayerController.Instance.Fame / 100f);

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
                SceneManager.LoadScene("GameEnd");
                Unsubscribe();
                if (loopCoro != null)
                    StopCoroutine(loopCoro);
                break;
            }
        }
    }

    private IEnumerator StreamLoop()
    {
        while (true)
        {
            state = GCState.Streaming;

            // If a minigame is active for any reason, wait here
            while (GameFlowController.Instance.CurrentState == GameState.Minigame)
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
            state = GCState.ShowingChoices;

            // Wait until the player chooses and EventManager closes the UI
            while (EventManager.Instance.IsShowingEvent)
                yield return null;

            state = GCState.Resolving;

            // If a minigame was launched by the choice, wait for it to finish
            while (GameFlowController.Instance.CurrentState == GameState.Minigame)
                yield return null;
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
