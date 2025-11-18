using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGameGeoDashController : MiniGameController
{
    [Header("Spike Setup")]
    public GameObject spikePrefab;
    public Transform[] spikeSpawnPoints; // Set in editor

    private Vector3[] spikePositions = new Vector3[]
    {
        // new Vector3(18f, -0.1f, 0f)
    };

    [Header("Game State")]
    public GameObject player;
    public GameObject losePanel;
    public GameObject winPanel;
    // private float gameTimer = 0f;
    // private float winTime = 30f;

    void Start()
    {
        this.delta = new Dictionary<string, int>();
        finished = false;
        successDeclared = false;

        if (losePanel) losePanel.SetActive(false);
        if (winPanel) winPanel.SetActive(false);

        SpawnSpikes();
    }

    void SpawnSpikes()
    {
        if (spikeSpawnPoints != null && spikeSpawnPoints.Length > 0)
        {
            foreach (Transform spawnPoint in spikeSpawnPoints)
            {
                Instantiate(spikePrefab, spawnPoint.position, Quaternion.identity);
            }
        } else
        {
            foreach (Vector3 position in spikePositions)
            {
                Instantiate(spikePrefab, position, Quaternion.identity);
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
