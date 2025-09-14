using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [Header("Stress Sprites (min 2)")]
    public List<Sprite> stressSprites;

    [Header("Reference to PlayerStats")]
    public PlayerStatsSO playerStats;

    private Image playerImage;

    void Start()
    {
        playerImage = GetComponent<Image>();
        if (stressSprites == null || stressSprites.Count < 2)
        {
            Debug.LogError("Player: Please assign at least 2 sprites to stressSprites.");
        }
    }

    void Update()
    {
        if (stressSprites == null || stressSprites.Count < 2 || playerStats == null || playerImage == null)
            return;

        int spriteCount = stressSprites.Count;
        float stress = playerStats.Stress; // Assuming stress is a float or int property
        float stressPerSprite = 100f / spriteCount;
        int spriteIndex = Mathf.FloorToInt(stress / stressPerSprite);
        spriteIndex = Mathf.Clamp(spriteIndex, 0, spriteCount - 1);
        playerImage.sprite = stressSprites[spriteIndex];
    }
}
