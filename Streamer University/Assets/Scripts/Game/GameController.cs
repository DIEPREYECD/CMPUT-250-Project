using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

public class GameController : MonoBehaviour
{
    // ====== Data ======
    [Header("Data")]
    [SerializeField] private PlayerStatsSO playerStats;      // ScriptableObject instance
    [SerializeField] private StreamEventSO[] eventPool;      // Put at least 2 in here

    // ====== UI Refs ======
    [Header("UI")]
    [SerializeField] private RectTransform eventCardsRoot; // container with HorizontalLayoutGroup
    [SerializeField] private GameObject eventCardPrefab;   // the prefab with EventCardUI.cs
    [SerializeField] private RectTransform playerAvatar;   // UI avatar RectTransform (Image)
    [SerializeField] private BarUI stressBar;
    [SerializeField] private BarUI fameBar;

    // ====== Timing ======
    [Header("Timing")]
    [Tooltip("Seconds between event prompts while streaming.")]
    [SerializeField] private float secondsBetweenEvents = 5f;

    // ====== Avatar Layout Targets ======
    [Header("Avatar Layout (Anchors & Size)")]
    [Tooltip("Where the avatar sits during normal streaming (center).")]
    [SerializeField] private RectTarget avatarCenter = RectTarget.Center();
    [Tooltip("Where the avatar moves while event cards are visible (bottom-left).")]
    [SerializeField] private RectTarget avatarDuringChoices = RectTarget.BottomLeft(new Vector2(20, 20), new Vector2(200, 280));
    [SerializeField] private float avatarTweenTime = 0.25f;

    // ====== Internal State ======
    private enum GCState { Streaming, ShowingChoices, Resolving }
    private GCState state = GCState.Streaming;
    private bool awaitingChoice;
    private Coroutine loopCoro;

    private void Awake()
    {
        // Basic sanity checks to help teammates wire things correctly
        Assert.IsNotNull(playerStats, "Assign PlayerStats on GameController.");
        Assert.IsNotNull(eventCardPrefab, "Assign EventCard prefab.");
        Assert.IsNotNull(eventCardsRoot, "Assign EventCardsRoot container.");
        Assert.IsNotNull(playerAvatar, "Assign PlayerAvatar RectTransform (UI).");
    }

    private void Start()
    {
        Debug.Log("Welcome to Streamer U!");
        Debug.Log("Let’s goooo!");
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
    }

    private IEnumerator StreamLoop()
    {
        while (true)
        {
            state = GCState.Streaming;

            // Wait before next event prompt
            yield return new WaitForSeconds(secondsBetweenEvents);

            // Show choices
            SpawnTwoCards();
            ChatOverlay.Instance.SetActive(false);
            yield return MoveAvatar(avatarDuringChoices);
            state = GCState.ShowingChoices;

            // Wait for user to click one
            awaitingChoice = true;
            while (awaitingChoice) yield return null;

            // Resolve choice & return to stream
            state = GCState.Resolving;
            yield return MoveAvatar(avatarCenter);
            ChatOverlay.Instance.SetActive(true);
        }
    }

    // --- Spawn 2 cards (simple: take first two different events; you can randomize later)
    private void SpawnTwoCards()
    {
        ClearChildren(eventCardsRoot);

        // Choose two events (for prototype, just take first two)
        StreamEventSO a = eventPool != null && eventPool.Length > 0 ? eventPool[0] : null;
        StreamEventSO b = eventPool != null && eventPool.Length > 1 ? eventPool[1] : null;

        if (a) CreateCard(a);
        if (b) CreateCard(b);
        eventCardsRoot.gameObject.SetActive(true);
    }

    private void CreateCard(StreamEventSO evt)
    {
        var go = Instantiate(eventCardPrefab, eventCardsRoot);
        var ui = go.GetComponent<UI.EventCardUI>();
        if (!ui) { Debug.LogWarning("EventCard prefab missing EventCardUI component."); return; }
        ui.Bind(evt, OnChooseEvent);
    }

    private void OnChooseEvent(StreamEventSO chosen)
    {
        // Apply hidden consequences
        Debug.Log($"Chose event: {chosen.title}");
        Debug.Log($"Event effects: Fame {chosen.dFame}, Stress {chosen.dStress}");
        playerStats.ApplyDelta(dFame: chosen.dFame, dStress: chosen.dStress);

        if (chosen.dFame > 0)
        {
            Debug.Log("W choice, stream is popping!");
        }
        else
        {
            Debug.Log("Yikes… not the best collab.");
        }
        Debug.Log($"New Fame: {playerStats.Fame}, Stress: {playerStats.Stress}");

        // Play sound
        AudioController.Instance.PlayChooseEvent();

        // Clear cards
        ClearChildren(eventCardsRoot);
        eventCardsRoot.gameObject.SetActive(false);

        // Check end conditions
        if (playerStats.Fame <= 0 || playerStats.Stress >= 100)
        {
            Debug.Log("Stream ended: burnout or lost all fame.");
            Debug.Log($"Final Fame: {playerStats.Fame}, Final Stress: {playerStats.Stress}");
            QuitGame();
            return; // Stop further logic; editor/game will exit
        }

        // Tell loop we're done choosing
        awaitingChoice = false;
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private static void ClearChildren(RectTransform root)
    {
        for (int i = root.childCount - 1; i >= 0; i--)
            Destroy(root.GetChild(i).gameObject);
    }

    // --- Small tween that works in 2019.4 without extra packages
    private IEnumerator MoveAvatar(RectTarget target)
    {
        var rt = playerAvatar;
        // Capture current
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
                sizeDelta = size        // e.g., (160,220)
            };
        }
    }
}
