using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGameGeoDashController : MiniGameController
{
    [Header("Prefabs")]
    public GameObject spikePrefab;
    public GameObject portalPrefab;
    public GameObject groundTilePrefab;

    [Header("Game State")]
    public GameObject player;
    public GameObject losePanel;
    public GameObject winPanel;
    // private float gameTimer = 0f;
    // private float winTime = 30f;

    void SpawnGround(Vector3 startPosition, float length, bool isCeiling = false)
    {
        // Assuming each tile is 1 unit wide - adjust if different
        float tileWidth = 1f;
        
        int numTiles = Mathf.CeilToInt(length / tileWidth);

        
        for (int i = 0; i < numTiles; i++)
        {
            Vector3 position = startPosition + Vector3.right * (i * tileWidth);
            GameObject tile = Instantiate(groundTilePrefab, position, Quaternion.identity);

            tile.layer = LayerMask.NameToLayer("Ground");
            
            if (isCeiling)
            {
                tile.transform.rotation = Quaternion.Euler(0, 0, 180);
            }
        }
    }


    void Start()
    {
        this.delta = new Dictionary<string, int>();
        finished = false;
        successDeclared = false;

        if (losePanel) losePanel.SetActive(false);
        if (winPanel) winPanel.SetActive(false);

        SpawnLevel();
    }

    void SpawnLevel()
    { 

        SpawnGround(new Vector3(-100f, -1f, 0f), 1100f, false);
        SpawnGround(new Vector3(-100f, 6f, 0f), 1100f, true);

        // false = floor, true = ceiling
        SpawnSpike(new Vector3(18f, -0.1f, 0f), false); 
        SpawnSpike(new Vector3(40f, 5f, 0f), true);
        SpawnSpike(new Vector3(60f, 5f, 0f), true);

        SpawnSpeedPortal(new Vector3(5f, 1f, 0f), Speeds.Fastest, false, Color.red);
        SpawnGravityPortal(new Vector3(70f, 4f, 0f), true, false);
    }

    // Spawn a spike at position, optionally upside down
    void SpawnSpike(Vector3 position, bool upsideDown)
    {
        GameObject spike = Instantiate(spikePrefab, position, Quaternion.identity);
        
        if (upsideDown)
        {
            spike.transform.rotation = Quaternion.Euler(0, 0, 180);
        }
    }

    // Spawn a speed portal
    void SpawnSpeedPortal(Vector3 position, Speeds speed, bool upsideDown, Color color)
    {
        GameObject portalObj = Instantiate(portalPrefab, position, Quaternion.identity);
        Portal portal = portalObj.GetComponent<Portal>();
        
        if (portal != null)
        {
            portal.Speed = speed;
            portal.gravity = false; // Not a gravity portal
            portal.State = 0; // 0 = speed portal
            
            // Set color
            SpriteRenderer sr = portalObj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = color;
            }
            
            // Rotate if on ceiling
            if (upsideDown)
            {
                portalObj.transform.rotation = Quaternion.Euler(0, 0, 180);
            }
        }
    }

    // Spawn a gravity portal
    void SpawnGravityPortal(Vector3 position, bool gravityDown, bool upsideDown, Color? color = null)
    {
        GameObject portalObj = Instantiate(portalPrefab, position, Quaternion.identity);
        Portal portal = portalObj.GetComponent<Portal>();
        
        if (portal != null)
        {
            portal.gravity = gravityDown;
            portal.State = 1;
            portal.Speed = Speeds.Normal;
            
            // Only set color if one was provided
            if (color.HasValue)
            {
                SpriteRenderer sr = portalObj.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = color.Value;
                }
            }
            
            if (upsideDown)
            {
                portalObj.transform.rotation = Quaternion.Euler(0, 0, 180);
            }
        }
    }

    void Update()
    {
        if (finished) return;

        // gameTimer += Time.deltaTime;
    }
    
    public void TriggerLose()
    {
        if (finished) return;

        successDeclared = false;
        finished = true;

        AudioController.Instance.toggleBGM();
        AudioController.Instance.PlayLoseMinigame();

        if (losePanel) losePanel.SetActive(true);

        Invoke("FinishMiniGame", 4f);
    }

    public void TriggerWin()
    {
        if (finished) return;

        successDeclared = true;
        finished = true;

        AudioController.Instance.toggleBGM();
        AudioController.Instance.PlayWinMinigame();

        if (winPanel) winPanel.SetActive(true);

        Invoke("FinishMiniGame", 4f);
    }

    public override void FinishMiniGame()
    {
        bool success = successDeclared;
        Debug.Log($"MiniGameGeoDashController: Finished Minigame!!");

        List<string> setFlags = new List<string>();
        if (success)
        {
            this.delta.Add("fame", 5);
            this.delta.Add("stress", -5);
            setFlags.Add("geoDashWin");
            EventManager.Instance.addToQueue("EVT_GEODASH_WIN");
        } else 
        {
            this.delta.Add("stress", 5);
            setFlags.Add("geoDashLose");
            EventManager.Instance.addToQueue("EVT_GEODASH_LOSE");
        }

        var result = new MiniGameResult
        {
            success = success,
            delta = delta
        };

        EventManager.Instance.setFlags(setFlags);

        if (resultChannel) resultChannel.Raise(result);
    }
}
