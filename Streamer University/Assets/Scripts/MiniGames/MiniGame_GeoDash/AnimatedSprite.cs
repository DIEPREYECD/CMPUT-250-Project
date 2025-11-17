using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedSprite : MonoBehaviour
{
    public List<Sprite> AnimationCycle;
    public float Framerate = 12f;
    private SpriteRenderer spriteRenderer;
    
    private float animationTimer;
    private float animationTimerMax;
    private int index;
    
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError($"No SpriteRenderer found on {gameObject.name}");
        }
    }
    
    void Start()
    {
        animationTimerMax = 1.0f / Framerate;
        index = 0;
    }
    
    void Update()
    {
        if (AnimationCycle == null || AnimationCycle.Count == 0) return;
        
        animationTimer += Time.deltaTime;
        
        if (animationTimer > animationTimerMax)
        {
            animationTimer = 0;
            index++;
            
            if (index >= AnimationCycle.Count)
                index = 0;
            
            spriteRenderer.sprite = AnimationCycle[index];
        }
    }
}