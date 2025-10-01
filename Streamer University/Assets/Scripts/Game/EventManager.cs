using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class EventManager : MonoBehaviour
{
    [Header("Data")]
    public TextAsset storyletsCsv;
    public PlayerStatsSO playerStats;
    public Sprite defaultSprite;

    [Header("UI Prefabs")]
    public GameObject mainCardPrefab;                 // shows picture + situation + 2 buttons: View Option 1 / View Option 2
    public GameObject sideChoicePrefab;               // small card showing choice text + image + "Choose this" button
    public Transform uiRoot;                          // parent for cards

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

    void Start()
    {
        // Check that all the required fields are present
        if (storyletsCsv == null || playerStats == null || mainCardPrefab == null || sideChoicePrefab == null || uiRoot == null || defaultSprite == null)
        {
            Debug.LogError("EventManager is not fully configured.");
            return;
        }

        db = CsvStoryletLoader.Load(storyletsCsv);
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
            if (e.conditions.minFame != null && playerStats.Fame < e.conditions.minFame) continue;
            if (e.conditions.maxFame != null && playerStats.Fame > e.conditions.maxFame) continue;
            if (e.conditions.minStress != null && playerStats.Stress < e.conditions.minStress) continue;
            if (e.conditions.maxStress != null && playerStats.Stress > e.conditions.maxStress) continue;

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

        currentMain = Instantiate(mainCardPrefab, uiRoot);

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
        OnEventOpened?.Invoke();
    }

    void ShowSideChoice(EventChoice c, bool left)
    {
        // If already showing, destroy and return
        if (left && sideLeft) { Destroy(sideLeft); return; }
        if (!left && sideRight) { Destroy(sideRight); return; }


        // Log choice info
        Debug.Log($"Showing side choice: {c.text}");
        Debug.Log($"Delta Fame: {c.deltaFame}, Delta Stress: {c.deltaStress}");

        var side = Instantiate(sideChoicePrefab, uiRoot);
        var img = side.transform.Find("Image").GetComponent<Image>();
        img.sprite = LoadSpriteOrDefault(c.spritePath);

        var txt = side.transform.Find("TextBox").GetChild(0).GetComponent<TMPro.TextMeshProUGUI>();
        txt.text = c.text;

        var chooseBtn = side.transform.Find("BtnChoose").GetComponent<Button>();
        chooseBtn.onClick.AddListener(() => Choose(c));

        // position left or right — keep it simple: anchor presets on your prefab
        if (left) { if (sideLeft) Destroy(sideLeft); sideLeft = side; }
        else { if (sideRight) Destroy(sideRight); sideRight = side; }
    }

    void Choose(EventChoice c)
    {
        // apply outcome
        playerStats.ApplyDelta(c.deltaFame, c.deltaStress);

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

        ClearUI();

        // Clear showing state and notify external systems
        IsShowingEvent = false;
        OnEventClosed?.Invoke();
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
    }

    public bool IsShowingEvent { get; private set; }   // true while a storylet is on screen
    public void ShowNextEvent() => NextTurn();
}
