using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MiniGameClickerController : MiniGameController
{
    private int score;
    private int targetScore;

    public Text scoreText;

    // The player has to reach the target score by clicking before time runs out
    private const float timeLimit = 40f;
    private float timer = 0f;

    public void Start()
    {
        this.delta = new Dictionary<string, int>();
        // Generate a random target score between 50 and 200
        score = 0;

        if (scoreText != null)
            scoreText.text = $"Score: {score}";

        targetScore = Random.Range(50, 200);
    }

    public void Update()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";

        timer += Time.deltaTime;
        if (timer >= timeLimit)
        {
            // Time's up, check if the player met the target score
            if (score >= targetScore)
            {
                successDeclared = true;
            }
            else
            {
                successDeclared = false;
            }

            // Change the score text to show final score and target and whether they won or lost
            // then wait a moment before finishing so the player can see the result
            if (scoreText != null)
                scoreText.text = $"Final Score: {score} / Target: {targetScore} - " + (successDeclared ? "You Win!" : "You Lose!");

            Invoke("FinishMiniGame", 3f);
        }
    }

    // Call this from a Button in the min-game UI to simulate "click to score"
    public void Click()
    {
        score++;
    }

    public override void FinishMiniGame()
    {
        bool success = successDeclared;
        Debug.Log($"MiniGameClickerController: Finishing minigame with success = {success}");
        List<string> setFlags = new List<string>();
        if (success)
        {
            this.delta.Add("fame", 5);
            this.delta.Add("stress", -3);
            setFlags.Add("clickerWin");
            EventManager.Instance.addToQueue("EVT003_WIN");
        }
        else
        {
            this.delta.Add("fame", -2);
            this.delta.Add("stress", 4);
            setFlags.Add("clickerLose");
            EventManager.Instance.addToQueue("EVT003_LOSE");
        }

        var result = new MiniGameResult
        {
            success = success,
            delta = delta
        };

        if (resultChannel != null)
            resultChannel.Raise(result);

        EventManager.Instance.setFlags(setFlags);
        MiniGameLoader.UnloadMiniGame(mySceneName);
        var mainScene = SceneManager.GetSceneByName("StreamScene");
        if (mainScene.IsValid())
            SceneManager.SetActiveScene(mainScene);
    }
}
