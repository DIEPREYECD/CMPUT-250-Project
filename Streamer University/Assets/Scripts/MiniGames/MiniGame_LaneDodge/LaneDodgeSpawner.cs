using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LaneDodgeSpawner : MonoBehaviour
{
    [System.Serializable]
    public class ObstacleSpriteConfig
    {
        public Sprite sprite;
        [Tooltip("Preferred width for this obstacle (height is fixed to 100).")]
        public float preferredWidth = 150f;
    }

    [Header("References")]
    [Tooltip("UI lane anchors where obstacles/pickups will be centered.")]
    public List<RectTransform> laneAnchors = new List<RectTransform>();

    [Tooltip("Spawn point on the right side of the road (only X is used).")]
    public RectTransform spawnPoint;

    [Tooltip("Obstacle prefab (with LaneDodgeObstacle attached).")]
    public GameObject obstaclePrefab;

    [Tooltip("Pickup prefab (with LaneDodgePickup attached).")]
    public GameObject pickupPrefab;

    [Header("Obstacle Sprites")]
    [Tooltip("Different obstacle sprites, each with its own preferred width.")]
    public List<ObstacleSpriteConfig> obstacleOptions = new List<ObstacleSpriteConfig>();

    [Header("Size Settings")]
    [Tooltip("Fixed height for all obstacles.")]
    public float obstacleHeight = 100f;

    [Header("Spawn Timing")]
    [Tooltip("Minimum time between spawns.")]
    public float minSpawnInterval = 0.7f;

    [Tooltip("Maximum time between spawns.")]
    public float maxSpawnInterval = 1.3f;

    [Header("Pickup Settings")]
    [Tooltip("Chance (0–1) that a spawn will be a pickup instead of an obstacle.")]
    public float pickupSpawnChance = 0.2f;

    [Tooltip("Fixed size for pickups (width x height).")]
    public Vector2 pickupSize = new Vector2(80f, 100f);

    [Header("Difficulty Scaling")]
    [Tooltip("Spawn interval at the start of the game (easy).")]
    public float easyMinSpawnInterval = 1.0f;
    public float easyMaxSpawnInterval = 1.5f;

    [Tooltip("Spawn interval when the game is hardest.")]
    public float hardMinSpawnInterval = 0.4f;
    public float hardMaxSpawnInterval = 0.8f;

    [Tooltip("Multiplier for obstacle speed at max difficulty.")]
    public float maxSpeedMultiplier = 2.0f;

    private float currentSpeedMultiplier = 1.0f;

    private bool spawning = false;

    public static LaneDodgeSpawner Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
    private void Start()
    {
        // Initialize to easy values
        minSpawnInterval = easyMinSpawnInterval;
        maxSpawnInterval = easyMaxSpawnInterval;
    }
    public void UpdateDifficulty(float normalized)
    {
        // normalized should be 0..1 based on timeAlive/gameDuration
        normalized = Mathf.Clamp01(normalized);

        // Lerp spawn interval between easy and hard
        minSpawnInterval = Mathf.Lerp(easyMinSpawnInterval, hardMinSpawnInterval, normalized);
        maxSpawnInterval = Mathf.Lerp(easyMaxSpawnInterval, hardMaxSpawnInterval, normalized);

        // Speed multiplier starts at 1 and goes up
        currentSpeedMultiplier = Mathf.Lerp(1.0f, maxSpeedMultiplier, normalized);
    }

    public void StartSpawning()
    {
        if (spawning) return;
        spawning = true;
        StartCoroutine(SpawnLoop());
    }

    public void StopSpawning()
    {
        spawning = false;
        StopAllCoroutines();
    }

    private IEnumerator SpawnLoop()
    {
        // Safety checks
        if (laneAnchors.Count == 0 || spawnPoint == null)
        {
            Debug.LogWarning("LaneDodgeSpawner: Missing lane anchors or spawnPoint.");
            yield break;
        }

        while (spawning)
        {
            // Wait a random interval
            float delay = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(delay);

            // Decide if this spawn is a pickup or an obstacle
            bool spawnPickup = (pickupPrefab != null && Random.value < pickupSpawnChance);

            // Pick random lane
            int laneIndex = Random.Range(0, laneAnchors.Count);
            RectTransform lane = laneAnchors[laneIndex];

            if (spawnPickup)
            {
                SpawnPickupAtLane(lane);
            }
            else
            {
                SpawnObstacleAtLane(lane);
            }
        }
    }

    private void SpawnObstacleAtLane(RectTransform lane)
    {
        if (obstaclePrefab == null) return;

        // Instantiate as child of the same parent as spawnPoint (usually the Canvas)
        GameObject obj = Instantiate(obstaclePrefab, spawnPoint.parent);
        RectTransform rect = obj.GetComponent<RectTransform>();

        if (rect == null)
        {
            Debug.LogWarning("LaneDodgeSpawner: Obstacle prefab has no RectTransform.");
            Destroy(obj);
            return;
        }

        // Position it: X from spawnPoint, Y from lane
        Vector2 pos = rect.anchoredPosition;
        pos.x = spawnPoint.anchoredPosition.x;
        pos.y = lane.anchoredPosition.y;
        rect.anchoredPosition = pos;

        // Choose a sprite configuration
        Sprite chosenSprite = null;
        float chosenWidth = obstacleHeight; // fallback

        if (obstacleOptions != null && obstacleOptions.Count > 0)
        {
            int index = Random.Range(0, obstacleOptions.Count);
            ObstacleSpriteConfig config = obstacleOptions[index];
            chosenSprite = config.sprite;
            chosenWidth = config.preferredWidth;
        }

        LaneDodgeObstacle obstacle = obj.GetComponent<LaneDodgeObstacle>();
        if (obstacle != null)
        {
            obstacle.SetSprite(chosenSprite);

            // Take the prefab's base speed and scale it by difficulty
            float baseSpeed = obstacle.moveSpeed; // public in LaneDodgeObstacle
            obstacle.SetSpeed(baseSpeed * currentSpeedMultiplier);
        }

        // Apply size + collider
        rect.sizeDelta = new Vector2(chosenWidth, obstacleHeight);
        LaneDodgeObstacle obstacleComp = obj.GetComponent<LaneDodgeObstacle>();
        if (obstacleComp != null)
        {
            obstacleComp.RefreshCollider();
        }
    }

    private void SpawnPickupAtLane(RectTransform lane)
    {
        if (pickupPrefab == null) return;

        GameObject obj = Instantiate(pickupPrefab, spawnPoint.parent);
        RectTransform rect = obj.GetComponent<RectTransform>();

        if (rect == null)
        {
            Debug.LogWarning("LaneDodgeSpawner: Pickup prefab has no RectTransform.");
            Destroy(obj);
            return;
        }

        // Position it: X from spawnPoint, Y from lane
        Vector2 pos = rect.anchoredPosition;
        pos.x = spawnPoint.anchoredPosition.x;
        pos.y = lane.anchoredPosition.y;
        rect.anchoredPosition = pos;

        // Apply fixed pickup size
        rect.sizeDelta = pickupSize;

        // refresh collider
        LaneDodgePickup pickupComp = obj.GetComponent<LaneDodgePickup>();
        if (pickupComp != null)
        {
            pickupComp.RefreshCollider();
        }
    }
}
