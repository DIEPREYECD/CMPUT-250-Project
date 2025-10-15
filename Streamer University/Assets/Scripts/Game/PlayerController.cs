using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
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

    //assign at least 2 stress sprites in the editor
    [Header("Stress Sprites (min 2)")]
    public List<Sprite> stressSprites;

    private Image playerImage;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }


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
        if (stressSprites == null || stressSprites.Count < 2 || playerImage == null)
            return;

        int spriteCount = stressSprites.Count;
        float stressPerSprite = 100f / spriteCount;
        int spriteIndex = Mathf.FloorToInt(stress / stressPerSprite);
        spriteIndex = Mathf.Clamp(spriteIndex, 0, spriteCount - 1);
        playerImage.sprite = stressSprites[spriteIndex];
    }

    // method to return stress value
    public int Stress { get { return stress; } }
    // method to return fame value
    public int Fame { get { return fame; } }

    // method to apply changes to fame and stress
    public void ApplyDelta(int dFame, int dStress)
    {
        Debug.Log($"Applying stats delta: Fame {dFame}, Stress {dStress}");
        fame += dFame;
        stress += dStress;
        Debug.Log($"New stats: Fame {fame}, Stress {stress}");
        fame = Mathf.Max(fame, 0);
        stress = Mathf.Clamp(stress, 0, 100);
    }

    // method to reset stats to default values
    public void ResetStats()
    {
        fame = 20;
        stress = 10;
    }
}