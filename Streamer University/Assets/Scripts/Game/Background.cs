using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Background : MonoBehaviour
{
    private static Background _instance;
    public static Background Instance { get { return _instance; } }

    public List<Sprite> backgrounds;

    private SpriteRenderer spriteRenderer;

    public int animRate = 10; // Frames per second for animation
    private int currentFrame = 0;

    private void Awake()
    {
        _instance = this;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("Background: No SpriteRenderer found on the GameObject.");
        }
    }

    private void Update()
    {
        if (backgrounds == null || backgrounds.Count == 0 || spriteRenderer == null)
            return;

        // Simple animation by cycling through the backgrounds
        currentFrame = (currentFrame + 1) % (animRate * backgrounds.Count);
        int index = currentFrame / animRate;
        spriteRenderer.sprite = backgrounds[index];
    }

    public void SetBackground(int index)
    {
        if (backgrounds == null || backgrounds.Count == 0)
        {
            Debug.LogError("Background: No backgrounds assigned.");
            return;
        }

        if (index < 0 || index >= backgrounds.Count)
        {
            Debug.LogError($"Background: Index {index} is out of range. Valid range is 0 to {backgrounds.Count - 1}.");
            return;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = backgrounds[index];
        }
    }

}