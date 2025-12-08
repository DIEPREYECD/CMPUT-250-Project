using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : AnimatedEntity
{
    //Singleton pattern
    private static PlayerController _instance;
    public static PlayerController Instance
    {
        get
        {
            // Check if instance exists
            if (_instance == null)
            {
                // Try to find an existing one in the scene first (optional check, should exist)
                _instance = FindObjectOfType<PlayerController>();

                // If none found, log an error
                if (_instance == null)
                {
                    Debug.LogError("No Player instance found in the scene!");
                }
            }

            return _instance;
        }
    }

    // define internal fame and stress variables
    [Header("Player Stats")]
    private int stress = 0;
    private int fame = 0;

    // Event data for stat changes
    public struct StatsDelta
    {
        public int deltaFame;
        public int deltaStress;
        public int oldFame;
        public int newFame;
        public int oldStress;
        public int newStress;
        public string source;
        public float timestamp;
    }

    // Raised whenever fame/stress are changed via ApplyDelta
    public event System.Action<StatsDelta> OnStatsChanged;

    [SerializeField]
    private List<Sprite> playerOnEvents;

    private void Subscribe()
    {
        // Subscribe to any events here if needed
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnEventOpened += PlayPlayerOnEvent;
            EventManager.Instance.OnEventClosed += PlayPlayerOffEvent;
        }
    }

    private void Unsubscribe()
    {
        // Unsubscribe from events here to prevent memory leaks
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnEventOpened -= PlayPlayerOnEvent;
            EventManager.Instance.OnEventClosed -= PlayPlayerOffEvent;
        }
    }
    private void OnDestroy()
    {
        Unsubscribe();
    }

    void Start()
    {
        AnimationSetup();
        Subscribe();

        // Make sure player on events are set
        if (playerOnEvents == null || playerOnEvents.Count == 0)
        {
            Debug.LogWarning("Player On Events not set in PlayerController.");
        }
    }

    void Update()
    {
        // Update the effective animation slice based on stress level
        int numSlices = DefaultAnimationCycle.Count / sliceBy;
        effectiveSlicePortion = Mathf.Clamp(stress / (100 / sliceBy), 0, numSlices - 1);
        AnimationUpdate();
    }

    // Public callback to trigger player "on" event animation
    public void PlayPlayerOnEvent()
    {
        if (playerOnEvents == null || playerOnEvents.Count == 0)
        {
            Debug.LogWarning("PlayerController: No playerOnEvents assigned for on-event animation.");
            return;
        }

        SetAnimationOverride(playerOnEvents);
    }

    // Public callback to trigger player "off" event animation
    public void PlayPlayerOffEvent()
    {
        ClearAnimationOverride();
    }

    // method to return stress value
    public int Stress { get { return stress; } }
    // method to return fame value
    public int Fame { get { return fame; } }

    // method to apply changes to fame and stress
    // Optional `source` parameter can be provided by callers for debugging/analytics
    public void ApplyDelta(int dFame, int dStress, string source = null)
    {
        int oldFame = fame;
        int oldStress = stress;

        fame += dFame;
        stress += dStress;
        fame = Mathf.Max(fame, 0);
        stress = Mathf.Clamp(stress, 0, 100);

        // Build and raise stats delta event
        StatsDelta sd = new StatsDelta
        {
            deltaFame = dFame,
            deltaStress = dStress,
            oldFame = oldFame,
            newFame = fame,
            oldStress = oldStress,
            newStress = stress,
            source = source,
            timestamp = Time.realtimeSinceStartup
        };

        OnStatsChanged?.Invoke(sd);
    }

    // method to reset stats to default values
    public void ResetStats()
    {
        fame = 20;
        stress = 10;
    }
}