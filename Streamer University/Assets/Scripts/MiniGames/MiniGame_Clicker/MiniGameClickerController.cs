using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MiniGameClickerController : MiniGameController
{
    private int score;
    public int targetScore = 5;

    public Text scoreText;

    public void Start()
    {
        this.delta = new Dictionary<string, int>();
    }

    public void Update()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";
    }

    // Call this from a Button in the min-game UI to simulate "click to score"
    public void Click()
    {
        score++;
        if (score >=  targetScore)
        {
            FinishMiniGame(success: true);
        }
    }

    public override void Fail()
    {
        FinishMiniGame(success: false);
    }

    public override void FinishMiniGame(bool success)
    {
        if (success)
        {
            this.delta.Add("fame", 5);
            this.delta.Add("stress", -3);
        }
        else
        {
            this.delta.Add("fame", -2);
            this.delta.Add("stress", 4);
        }

        var result = new MiniGameResult
        {
            success = success,
            delta = delta
        };

        if (resultChannel != null)
            resultChannel.Raise(result);

        MiniGameLoader.UnloadMiniGame(mySceneName);
        var mainScene = SceneManager.GetSceneByName("StreamScene");
        if (mainScene.IsValid())
            SceneManager.SetActiveScene(mainScene);
    }
}
