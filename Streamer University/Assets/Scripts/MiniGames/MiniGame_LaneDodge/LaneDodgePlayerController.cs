using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LaneDodgePlayerController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The RectTransform of the player Image.")]
    public RectTransform playerRect;

    [Tooltip("UI Image component that displays the player sprite.")]
    public Image playerImage;

    [Tooltip("Lane anchors in order: bottom, middle, top (or however you want).")]
    public List<RectTransform> laneAnchors = new List<RectTransform>();

    [Header("Movement")]
    [Tooltip("Index of the lane the player starts in.")]
    public int startLaneIndex = 1;  // 0 = first lane, 1 = middle, 2 = top

    [Tooltip("Time it takes to move between lanes.")]
    public float laneMoveDuration = 0.15f;

    private int currentLaneIndex;
    private bool isMoving = false;

    [Header("Run Animation")]
    [Tooltip("Sprites for the running animation, played in a loop.")]
    public List<Sprite> runSprites = new List<Sprite>();

    [Tooltip("Frames per second for the running animation.")]
    public float animationFPS = 10f;

    private int currentFrameIndex = 0;
    private float animationTimer = 0f;

    public static LaneDodgePlayerController Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Try to auto-fill if not set from Inspector
        if (playerRect == null)
            playerRect = GetComponent<RectTransform>();

        if (playerImage == null)
            playerImage = GetComponent<Image>();
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
        // Clamp start lane index and snap to that lane
        if (laneAnchors.Count > 0)
        {
            startLaneIndex = Mathf.Clamp(startLaneIndex, 0, laneAnchors.Count - 1);
            currentLaneIndex = startLaneIndex;
            SnapToLane(currentLaneIndex);
        }

        ResizeColliderToMatchRect();
    }

    private void Update()
    {
        if (LaneDodgeGameController.Instance == null || LaneDodgeGameController.Instance.IsGameFinished)
            return;

        HandleInput();
        UpdateRunAnimation();
    }

    private void HandleInput()
    {
        if (isMoving || laneAnchors.Count == 0)
            return;

        int targetLane = currentLaneIndex;

        // Up
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            targetLane = currentLaneIndex - 1;
        }
        // Down
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            targetLane = currentLaneIndex + 1;
        }

        // If lane changed and still in range, start movement
        if (targetLane != currentLaneIndex &&
            targetLane >= 0 && targetLane < laneAnchors.Count)
        {
            StartCoroutine(MoveToLaneCoroutine(targetLane));

        }
    }

    private IEnumerator MoveToLaneCoroutine(int targetLaneIndex)
    {
        isMoving = true;

        Vector2 startPos = playerRect.anchoredPosition;
        Vector2 endPos = new Vector2(
            startPos.x, // keep x the same
            laneAnchors[targetLaneIndex].anchoredPosition.y
        );

        float elapsed = 0f;

        while (elapsed < laneMoveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / laneMoveDuration);
            playerRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        playerRect.anchoredPosition = endPos;
        currentLaneIndex = targetLaneIndex;
        isMoving = false;
    }

    private void SnapToLane(int laneIndex)
    {
        if (laneIndex < 0 || laneIndex >= laneAnchors.Count)
            return;

        Vector2 pos = playerRect.anchoredPosition;
        pos.y = laneAnchors[laneIndex].anchoredPosition.y;
        playerRect.anchoredPosition = pos;
    }

    private void UpdateRunAnimation()
    {
        if (runSprites == null || runSprites.Count == 0 || playerImage == null)
            return;

        animationTimer += Time.deltaTime;
        float frameTime = 1f / animationFPS;

        if (animationTimer >= frameTime)
        {
            animationTimer -= frameTime;
            currentFrameIndex = (currentFrameIndex + 1) % runSprites.Count;
            playerImage.sprite = runSprites[currentFrameIndex];
        }
    }

    private void ResizeColliderToMatchRect()
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null && playerRect != null)
        {
            col.size = playerRect.sizeDelta;
            col.offset = Vector2.zero;
        }
    }


}
