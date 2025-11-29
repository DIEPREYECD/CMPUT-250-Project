using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LaneDodgePickup : MonoBehaviour
{
    [Header("References")]
    public RectTransform rectTransform;
    public Image pickupImage;

    [Header("Movement")]
    public float moveSpeed = 600f;
    public float destroyX = -1200f;

    [Header("Pickup Settings")]
    [Tooltip("How many seconds of extra time this pickup should eventually give.")]
    public float timeBonus = 3f;

    private void Awake()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        if (pickupImage == null)
            pickupImage = GetComponent<Image>();
    }

    private void Update()
    {
        // If game is finished, destroy self
        if (LaneDodgeGameController.Instance != null && LaneDodgeGameController.Instance.IsGameFinished)
        {
            Destroy(gameObject);
            return;
        }

        if (rectTransform == null) return;

        rectTransform.anchoredPosition += Vector2.left * moveSpeed * Time.deltaTime;

        if (rectTransform.anchoredPosition.x < destroyX)
        {
            Destroy(gameObject);
        }
    }

    public void SetSprite(Sprite sprite)
    {
        if (pickupImage != null && sprite != null)
        {
            pickupImage.sprite = sprite;
        }
    }

    public void SetSpeed(float speed)
    {
        moveSpeed = speed;
    }

    // We'll wire this up to actually add time once the GameController is written.
    // For now we just destroy on collision with the player.
    private void OnTriggerEnter2D(Collider2D other)
    {
        // We'll tag the player as "Player" later.
        if (!other.CompareTag("Player"))
            return;

        if (LaneDodgeGameController.Instance != null)
        {
            LaneDodgeGameController.Instance.AddTime(timeBonus);
        }

        Destroy(gameObject);
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


}
