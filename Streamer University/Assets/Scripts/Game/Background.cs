using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;


public class Background : MonoBehaviour
{
    private static Background _instance;
    public static Background Instance { get { return _instance; } }

    public List<Sprite> backgrounds;

    private Image backgroundImage;

    public int animRate = 10; // Frames per second for animation
    private int currentFrame = 0;

    private void Awake()
    {
        _instance = this;
        backgroundImage = GetComponent<Image>();
        if (backgroundImage == null)
        {
            Debug.LogError("Background: No Image found on the GameObject.");
        }
    }

    private void Update()
    {
        if (backgrounds == null || backgrounds.Count == 0 || backgroundImage == null)
            return;

        // Simple animation by cycling through the backgrounds
        currentFrame = (currentFrame + 1) % (animRate * backgrounds.Count);
        int index = currentFrame / animRate;
        backgroundImage.sprite = backgrounds[index];
    }

}