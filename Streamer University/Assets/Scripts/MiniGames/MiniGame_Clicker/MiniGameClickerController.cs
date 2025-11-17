using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MiniGameClickerController : MiniGameController
{
    private int score;
    private int targetScore;

    [Header("UI References")]
    public Text scoreText;
    public Text instructionsText;
    public Text timerText;
    public GameObject losePanel;
    public GameObject winPanel;

    [Header("Intro UI")]
    public GameObject introPanel;      // Fullscreen panel with intro text + start button
    public GameObject gameUIRoot;      // Parent for the actual gameplay UI (score, timer, button, etc.)

    private bool gameStarted = false;
    private float soundEffectVolReduce = 0.4f; // How much quieter should the clicking sound effect be in this minigame

    // The player has to reach the target score by clicking before time runs out
    private const float timeLimit = 40f;
    private float timer = 0f;

    public void Start()
    {
        this.delta = new Dictionary<string, int>();
        // Generate a random target score between 50 and 200
        score = 0;
        timer = 0f;
        finished = false;
        successDeclared = false;

        if (scoreText != null)
            scoreText.text = $"Score: {score}";

        targetScore = Random.Range(150, 300);

        if (instructionsText != null)
            instructionsText.text =
                $"Keep petting Mr. Kitty to win!\n" +
                $"Reach at least {targetScore} points in {timeLimit} seconds.";

        if (scoreText != null)
            scoreText.text = $"Score: {score}";

        if (timerText != null)
            timerText.text = $"Time Left: {timeLimit.ToString("F2")} seconds";

        // Result panels hidden at start
        if (losePanel != null) losePanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);

        // Show intro, hide gameplay until player presses Start
        if (introPanel != null) introPanel.SetActive(true);
        if (gameUIRoot != null) gameUIRoot.SetActive(false);

        gameStarted = false;
    }

    public void Update()
    {
        // Don't run the game loop until player has started
        if (!gameStarted || finished) return;

        if (scoreText != null)
            scoreText.text = $"Score: {score}";

        if (timerText != null && timer <= timeLimit)
            timerText.text = $"Time Left: {(timeLimit - timer).ToString("F2")} seconds";

        if (timeLimit - timer <= 10f && score < targetScore)
        {
            float t = Mathf.PingPong(Time.time * 5f, 1f);
            timerText.color = Color.Lerp(Color.white, Color.red, t);
        }

        timer += Time.deltaTime;

        if (timer >= timeLimit)
        {
            successDeclared = score >= targetScore;
            AudioController.Instance.toggleBGM(); // stop the BGM 
            AudioController.Instance.SFXSource.volume += soundEffectVolReduce; // Reverse the modified sound volume
            if (successDeclared == false)
            {
                AudioController.Instance.PlayLoseMinigame();
                losePanel.SetActive(true);
            }
            else
            {
                AudioController.Instance.PlayWinMinigame();
                winPanel.SetActive(true);
            }
            // Change the score text to show final score and target and whether they won or lost
            // then wait a moment before finishing so the player can see the result
            if (scoreText != null)
                scoreText.text = $"Final Score: {score} / Target: {targetScore} - " + (successDeclared ? "You Win!" : "You Lose!");

            Invoke("FinishMiniGame", 4f);
            finished = true;
        }
        else if (score >= targetScore)
        {
            successDeclared = true;
            winPanel.SetActive(true);
            AudioController.Instance.toggleBGM(); // stop the BGM
            AudioController.Instance.SFXSource.volume += soundEffectVolReduce; // Reverse the modified sound volume
            AudioController.Instance.PlayWinMinigame();
            // Change the score text to show final score and target and whether they won or lost
            // then wait a moment before finishing so the player can see the result
            if (scoreText != null)
                scoreText.text = $"Final Score: {score} / Target: {targetScore} - " + (successDeclared ? "You Win!" : "You Lose!");

            Invoke("FinishMiniGame", 4f);
            finished = true;
        }
    }

    // Call this from a Button in the min-game UI to simulate "click to score"
    public void Click()
    {
        if (!gameStarted || finished) return;

        AudioController.Instance.PlayCatMeow();
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
            this.delta.Add("stress", 4);
            setFlags.Add("clickerLose");
            EventManager.Instance.addToQueue("EVT003_LOSE");
        }

        var result = new MiniGameResult
        {
            success = success,
            delta = delta
        };

        EventManager.Instance.setFlags(setFlags);

        if (resultChannel != null)
            resultChannel.Raise(result);
    }

    public void StartGame()
    {
        if (gameStarted) return;

        AudioController.Instance.PlaySelect();
        AudioController.Instance.checkScene(); // play the minigame soundtrack
        AudioController.Instance.SFXSource.volume -= soundEffectVolReduce;

        gameStarted = true;
        timer = 0f;

        if (introPanel != null)
            introPanel.SetActive(false);

        if (gameUIRoot != null)
            gameUIRoot.SetActive(true);

        // Reset timer display once more at the actual start
        if (timerText != null)
            timerText.text = $"Time Left: {timeLimit.ToString("F2")} seconds";
    }

}
