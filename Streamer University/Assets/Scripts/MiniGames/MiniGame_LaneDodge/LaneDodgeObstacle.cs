using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LaneDodgeObstacle : MonoBehaviour
{
    [Header("References")]
    [Tooltip("RectTransform of this obstacle UI element.")]
    public RectTransform rectTransform;

    [Tooltip("Image component used to display the obstacle sprite.")]
    public Image obstacleImage;

    [Header("Movement")]
    [Tooltip("Horizontal speed in UI units per second (moving left).")]
    public float moveSpeed = 600f;

    [Tooltip("X position (anchored) at which this obstacle destroys itself (off-screen to the left).")]
    public float destroyX = -1200f;

    private void Awake()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        if (obstacleImage == null)
            obstacleImage = GetComponent<Image>();
    }

    private void Update()
    {
        // If game is over, destroy self
        if (LaneDodgeGameController.Instance != null && LaneDodgeGameController.Instance.IsGameFinished)
        {
            Destroy(gameObject);
            return;
        }

        if (rectTransform == null) return;

        // Move left
        rectTransform.anchoredPosition += Vector2.left * moveSpeed * Time.deltaTime;

        // Destroy when far enough off-screen
        if (rectTransform.anchoredPosition.x < destroyX)
        {
            Destroy(gameObject);
        }
    }

    // Called by the spawner to set which sprite this obstacle uses.
    public void SetSprite(Sprite sprite)
    {
        if (obstacleImage != null && sprite != null)
        {
            obstacleImage.sprite = sprite;
        }
    }

    // Optional: allow spawner to override speed per obstacle
    public void SetSpeed(float speed)
    {
        moveSpeed = speed;
    }
    private void ResizeColliderToMatchRect()
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null && rectTransform != null)
        {
            col.size = rectTransform.sizeDelta;
            col.offset = Vector2.zero;
        }
    }
    public void RefreshCollider()
    {
        ResizeColliderToMatchRect();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        // Tell GameController that player crashed
        if (LaneDodgeGameController.Instance != null)
        {
            LaneDodgeGameController.Instance.OnPlayerHitObstacle();
        }

        Destroy(gameObject);
    }


}
