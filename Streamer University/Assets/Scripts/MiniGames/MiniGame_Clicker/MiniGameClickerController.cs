using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MiniGameClickerController : MiniGameController
{
    private int score;
    private int targetScore;

    public Text scoreText;
    public Text instructionsText;
    public Text timerText;
    public GameObject losePanel;
    public GameObject winPanel;

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

        targetScore = Random.Range(150, 300);

        instructionsText.text = $"Keep Clicking the Button to WIN!!!!!! Don't be a LOSER : ( Reach at least {targetScore} points in {timeLimit} seconds!";

        if (timerText != null)
            timerText.text = $"Time Left: {(timeLimit - timer).ToString("F2")} seconds";
        losePanel.SetActive(false);
        winPanel.SetActive(false);
    }

    public void Update()
    {
        if (finished) return;

        if (scoreText != null)
            scoreText.text = $"Score: {score}";

        if (timerText != null && timer <= timeLimit)
            timerText.text = $"Time Left: {(timeLimit - timer).ToString("F2")} seconds";

        // When time is less than 10 second, change timer text color to blinking betwenn white and black
        if (timeLimit - timer <= 10f && score < targetScore)
        {
            float t = Mathf.PingPong(Time.time * 5f, 1f);
            timerText.color = Color.Lerp(Color.white, Color.red, t);
        }

        timer += Time.deltaTime;

        if (timer >= timeLimit)
        {
            successDeclared = score >= targetScore;
            if (successDeclared == false)
            {
                losePanel.SetActive(true);
            }
            else
            {
                winPanel.SetActive(true);
            }
            // Change the score text to show final score and target and whether they won or lost
            // then wait a moment before finishing so the player can see the result
            if (scoreText != null)
                scoreText.text = $"Final Score: {score} / Target: {targetScore} - " + (successDeclared ? "You Win!" : "You Lose!");

            Invoke("FinishMiniGame", 2f);
            finished = true;
        }
        else if (score >= targetScore)
        {
            successDeclared = true;
            winPanel.SetActive(true) ;
            // Change the score text to show final score and target and whether they won or lost
            // then wait a moment before finishing so the player can see the result
            if (scoreText != null)
                scoreText.text = $"Final Score: {score} / Target: {targetScore} - " + (successDeclared ? "You Win!" : "You Lose!");

            Invoke("FinishMiniGame", 2f);
            finished = true;
        }
    }

    // Call this from a Button in the min-game UI to simulate "click to score"
    public void Click()
    {
        AudioController.Instance.PlaySelect();
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
