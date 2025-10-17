using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class EventManager : MonoBehaviour
{
    [Header("Data")]
    public TextAsset storyletsCsv;
    public Sprite defaultSprite;
    public AudioController audioController;

    [Header("UI Prefabs")]
    public GameObject mainCardPrefab;                 // shows picture + situation + 2 buttons: View Option 1 / View Option 2
    public GameObject sideChoicePrefab;               // small card showing choice text + image + "Choose this" button
    public Transform uiRoot;                          // parent for cards

    // Add to EventManager.cs (fields)
    private RectTransform clusterRoot;         // NEW: centered container for main + side cards
    [SerializeField] private readonly float cardGap = 20f;
    private float nudgeX;
    [SerializeField] private readonly float animTime = 0.20f;

    private Dictionary<string, EventDef> db;               // all loaded events
    private readonly Queue<EventDef> queued = new Queue<EventDef>();                   // for chains
    private readonly HashSet<string> flags = new HashSet<string>();                    // simple global flags
    private readonly Dictionary<string, int> cooldown = new Dictionary<string, int>();         // eventId -> turns left
    private readonly HashSet<string> consumed = new HashSet<string>();                 // oncePerRun

    // Events for external systems (like GameController)
    public event System.Action OnEventOpened;          // fired when main card appears
    public event System.Action OnEventClosed;          // fired after a choice is made and UI cleared

    private GameObject currentMain;
    private GameObject sideLeft, sideRight;
    private EventDef currentEvent;

    // Make this a singleton for easy access
    private static EventManager _instance;
    public static EventManager Instance { get { return _instance; } }

    public void setFlags(List<string> flags)
    {
        foreach (string flag in flags) {
            this.flags.Add(flag);
        }
    }

    public void clearFlags(List<string> flags)
    {
        foreach (string flag in flags)
        {
            this.flags.Remove(flag);
        }
    }

    private void Awake()
    {
        _instance = this;
    }

    void Start()
    {
        // Check that all the required fields are present
        if (storyletsCsv == null || mainCardPrefab == null || sideChoicePrefab == null || uiRoot == null || defaultSprite == null)
        {
            Debug.LogError("EventManager is not fully configured.");
            return;
        }

        db = CsvStoryletLoader.Load(storyletsCsv);

        // Calculate nudgeX based on prefab sizes also applying the local scale and gap
        var mainRect = mainCardPrefab.GetComponent<RectTransform>().rect;
        var sideRect = sideChoicePrefab.GetComponent<RectTransform>().rect;
        nudgeX = mainRect.width * mainCardPrefab.transform.localScale.x / 2f
               + sideRect.width * sideChoicePrefab.transform.localScale.x / 2f
               + cardGap;

        Debug.Log($"Total events loaded: {db.Count}");
        Debug.Log($"NudgeX calculated as: {nudgeX}");
    }

    public void NextTurn()
    {
        // reduce cooldowns
        foreach (var key in cooldown.Keys.ToList())
            cooldown[key] = Math.Max(0, cooldown[key] - 1);

        // pick next event (chain has priority)
        currentEvent = queued.Count > 0 ? queued.Dequeue() : PickRandomEligible();
        if (currentEvent == null)
        {
            Debug.Log("No eligible events.");
            return;
        }

        Debug.Log($"Next event: {currentEvent.id} - {currentEvent.situation}");

        ShowMainCard(currentEvent);
    }

    EventDef PickRandomEligible()
    {
        var pool = new List<(EventDef e, int weight)>();
        foreach (var e in db.Values)
        {
            if (e.oncePerRun && consumed.Contains(e.id)) continue;
            if (cooldown.TryGetValue(e.id, out var left) && left > 0) continue;

            // stat gates
            if (e.conditions.minFame != null && PlayerController.Instance.Fame < e.conditions.minFame) continue;
            if (e.conditions.maxFame != null && PlayerController.Instance.Fame > e.conditions.maxFame) continue;
            if (e.conditions.minStress != null && PlayerController.Instance.Stress < e.conditions.minStress) continue;
            if (e.conditions.maxStress != null && PlayerController.Instance.Stress > e.conditions.maxStress) continue;

            // flag gates
            if (e.conditions.requiresAllFlags.Any(req => !flags.Contains(req))) continue;
            if (e.conditions.forbidsAnyFlags.Any(f => flags.Contains(f))) continue;

            pool.Add((e, Math.Max(1, e.weight)));
        }

        if (pool.Count == 0) return null;

        int total = pool.Sum(p => p.weight);
        int r = UnityEngine.Random.Range(0, total);
        foreach (var (e, weight) in pool)
        {
            if (r < weight) return e;
            r -= weight;
        }
        // As a fallback (shouldn't happen), return the first event
        return pool[0].e;
    }

    void ShowMainCard(EventDef e)
    {
        ClearUI();

        var clusterGO = new GameObject("EventCluster", typeof(RectTransform));
        clusterRoot = clusterGO.GetComponent<RectTransform>();
        clusterRoot.SetParent(uiRoot, worldPositionStays: false);
        clusterRoot.anchorMin = clusterRoot.anchorMax = clusterRoot.pivot = new Vector2(0.5f, 0.5f);
        clusterRoot.anchoredPosition = Vector2.zero;
        clusterRoot.sizeDelta = Vector2.zero;
        clusterRoot.gameObject.layer = uiRoot.gameObject.layer;

        // add layout components at runtime if not on a prefab
        // var hlg = clusterGO.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
        // hlg.childAlignment = TextAnchor.MiddleCenter;
        // hlg.spacing = 32f;
        // hlg.childForceExpandWidth = false;
        // hlg.childForceExpandHeight = false;

        var csf = clusterGO.AddComponent<UnityEngine.UI.ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;


        currentMain = Instantiate(mainCardPrefab, clusterRoot);

        // Bind sprite + situation text + two “View” buttons
        var img = currentMain.transform.Find("Image").GetComponent<Image>();
        img.sprite = LoadSpriteOrDefault(e.spritePath);

        var txt = currentMain.transform.Find("TextBox").GetChild(0).GetComponent<TMPro.TextMeshProUGUI>();
        txt.text = e.situation;

        var btnViewA = currentMain.transform.Find("BtnViewA").GetComponent<Button>();
        var btnViewB = currentMain.transform.Find("BtnViewB").GetComponent<Button>();

        btnViewA.onClick.AddListener(() => ShowSideChoice(e.choices[0], left: true));
        btnViewB.onClick.AddListener(() => ShowSideChoice(e.choices[1], left: false));

        // Set showing state and notify external systems
        IsShowingEvent = true;
        Debug.Log("Invoking OnEventOpened");
        OnEventOpened?.Invoke();

        // play sound
        audioController.PlayOnEvent();
    }

    void ShowSideChoice(EventChoice c, bool left)
    {
        // If already showing, destroy and return
        if (left && sideLeft) { Destroy(sideLeft); sideLeft = null; StartCoroutine(NudgeMain(0f)); return; }
        if (!left && sideRight) { Destroy(sideRight); sideRight = null; StartCoroutine(NudgeMain(0f)); return; }

        // Destroy opposite side if present and nudge the main back to center
        if (left && sideRight) { Destroy(sideRight); sideRight = null; }
        if (!left && sideLeft) { Destroy(sideLeft); sideLeft = null; }
        StartCoroutine(NudgeMain(0f));

        // Log choice info
        Debug.Log($"Showing side choice: {c.text}");
        Debug.Log($"Delta Fame: {c.deltaFame}, Delta Stress: {c.deltaStress}");

        var side = Instantiate(sideChoicePrefab, clusterRoot);

        var img = side.transform.Find("Image").GetComponent<Image>();
        img.sprite = LoadSpriteOrDefault(c.spritePath);

        var txt = side.transform.Find("TextBox").GetChild(0).GetComponent<TMPro.TextMeshProUGUI>();
        txt.text = c.text;

        var chooseBtn = side.transform.Find("BtnChoose").GetComponent<Button>();
        chooseBtn.onClick.AddListener(() => Choose(c));

        // store references
        if (left) { if (sideLeft) Destroy(sideLeft); sideLeft = side; }
        else { if (sideRight) Destroy(sideRight); sideRight = side; }

        // Add CanvasGroup for fade
        var cg = side.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        side.transform.localScale *= 0.98f;

        // Animate: nudge main, then fade/slide in side
        StartCoroutine(NudgeMain(left ? +nudgeX : -nudgeX));
        StartCoroutine(FadeInAndSlide(side.GetComponent<RectTransform>(), cg,
                                      targetLocalX: left ? -nudgeX : +nudgeX, animTime));

        // play sound
        audioController.PlayOpenSideCard();
    }

    void Choose(EventChoice c)
    {
        Debug.Log($"Chose: {c.text}");
        Debug.Log($"Delta Fame: {c.deltaFame}, Delta Stress: {c.deltaStress}");
        if (!string.IsNullOrWhiteSpace(c.nextEventId))
            Debug.Log($"Next event in chain: {c.nextEventId}");

        // apply outcome
        PlayerController.Instance.ApplyDelta(c.deltaFame, c.deltaStress);

        foreach (var f in c.setFlags) flags.Add(f);
        foreach (var f in c.clearFlags) flags.Remove(f);

        // mark cooldown & once
        if (currentEvent.cooldownTurns > 0)
            cooldown[currentEvent.id] = currentEvent.cooldownTurns;
        if (currentEvent.oncePerRun)
            consumed.Add(currentEvent.id);

        // enqueue chain
        if (!string.IsNullOrWhiteSpace(c.nextEventId) && db.TryGetValue(c.nextEventId, out var next))
            queued.Enqueue(next);

        // play sound
        audioController.PlayChooseEvent();

        ClearUI();

        // Close the event first so the loop knows the card is gone
        IsShowingEvent = false;
        OnEventClosed?.Invoke();

        // If this choice launches a minigame, run it modally
        if (!string.IsNullOrWhiteSpace(c.miniGame))
        {
            // NOTE: do NOT apply minigame result deltas here; MiniGameLoader does it
            MiniGameLoader.Instance.RunMiniGame(c.miniGame);
        }
    }

    Sprite LoadSpriteOrDefault(string path)
    {
        if (!string.IsNullOrWhiteSpace(path))
        {
            var s = Resources.Load<Sprite>(path);
            if (s != null) return s;
        }
        return defaultSprite;
    }

    void ClearUI()
    {
        if (currentMain) Destroy(currentMain);
        if (sideLeft) Destroy(sideLeft);
        if (sideRight) Destroy(sideRight);
        if (clusterRoot) { Destroy(clusterRoot.gameObject); clusterRoot = null; }
    }

    private IEnumerator NudgeMain(float x)
    {
        if (!currentMain) yield break;
        var rt = currentMain.GetComponent<RectTransform>();
        var start = rt.anchoredPosition;
        var end = new Vector2(x, start.y);
        float t = 0f;
        while (t < animTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / animTime);
            rt.anchoredPosition = Vector2.Lerp(start, end, k);
            yield return null;
        }
        rt.anchoredPosition = end;
    }

    private IEnumerator FadeInAndSlide(RectTransform rt, CanvasGroup cg, float targetLocalX, float time)
    {
        // start slightly offset so it "slides in" towards center
        Vector2 start = new Vector2(targetLocalX * 0.5f, 0f);
        Vector2 end = Vector2.zero;
        rt.anchoredPosition = start;

        float t = 0f;
        while (t < time)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / time);
            rt.anchoredPosition = Vector2.Lerp(start, end, k);
            cg.alpha = Mathf.Lerp(0f, 1f, k);
            yield return null;
        }
        rt.anchoredPosition = end;
        cg.alpha = 1f;
    }

    public bool IsShowingEvent { get; private set; }   // true while a storylet is on screen
    public void ShowNextEvent() => NextTurn();
}
