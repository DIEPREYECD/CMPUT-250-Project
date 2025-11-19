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

    [System.Serializable]
    public class TargetConfig
    {
        public int targetScore = 150;
        public int fameSuccess = 5;
        public int stressSuccess = -3;
        public int fameFail = 0;
        public int stressFail = 4;
    }

    // Combined per-difficulty configuration (score + success/fail deltas)
    public List<TargetConfig> targetConfigs;

    // Selected difficulty index for this run (-1 = random/fallback)
    private int chosenDifficultyIndex = -1;

    private bool gameStarted = false;
    private float soundEffectVolReduce = 0.4f; // How much quieter should the clicking sound effect be in this minigame

    [SerializeField]
    private const float timeLimit = 30f;
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

        // Determine targetScore based on difficulty flags managed by EventManager.
        // Flags are named "ClickerMini_Game_{n}" where n is the difficulty index.
        if (targetConfigs != null && targetConfigs.Count > 0)
        {
            int currentIndex = 0; // default when no flag exists
            if (EventManager.Instance != null)
            {
                try
                {
                    var allFlags = EventManager.Instance.GetFlags();
                    int found = -1;
                    const string prefix = "ClickerMini_Game_";
                    foreach (var f in allFlags)
                    {
                        if (f != null && f.StartsWith(prefix))
                        {
                            var suffix = f.Substring(prefix.Length);
                            if (int.TryParse(suffix, out var v))
                            {
                                if (found == -1 || v < found) // choose lowest if multiple present
                                    found = v;
                            }
                        }
                    }
                    if (found >= 0)
                        currentIndex = found;
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"MiniGameClickerController: Error reading flags: {ex.Message}");
                    currentIndex = 0;
                }
            }
            if (currentIndex < 0 || currentIndex >= targetConfigs.Count)
                currentIndex = 0;

            targetScore = targetConfigs[currentIndex].targetScore;
            chosenDifficultyIndex = currentIndex;

            // Advance difficulty flag (wrap around)
            if (EventManager.Instance != null)
            {
                int nextIndex = (currentIndex + 1) % targetConfigs.Count;
                // clear any existing ClickerMini_Game_* flags to keep state consistent
                var toClear = new List<string>();
                for (int i = 0; i < targetConfigs.Count; i++)
                    toClear.Add($"ClickerMini_Game_{i}");

                EventManager.Instance.clearFlags(toClear);
                EventManager.Instance.setFlags(new List<string> { $"ClickerMini_Game_{nextIndex}" });
                Debug.Log($"MiniGameClickerController: selected difficulty {currentIndex}, target {targetScore}, advanced to {nextIndex}");
            }
        }
        else
        {
            // fallback to previous random behavior when no explicit targets set
            targetScore = Random.Range(150, 300);
            chosenDifficultyIndex = -1;
        }

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
            if (chosenDifficultyIndex >= 0 && targetConfigs != null && chosenDifficultyIndex < targetConfigs.Count)
            {
                var cfg = targetConfigs[chosenDifficultyIndex];
                this.delta.Add("fame", cfg.fameSuccess);
                this.delta.Add("stress", cfg.stressSuccess);
            }
            else
            {
                // fallback to previous defaults
                this.delta.Add("fame", 5);
                this.delta.Add("stress", -3);
            }
            setFlags.Add("clickerWin");
            EventManager.Instance.addToQueue("EVT003_WIN");
        }
        else
        {
            if (chosenDifficultyIndex >= 0 && targetConfigs != null && chosenDifficultyIndex < targetConfigs.Count)
            {
                var cfg = targetConfigs[chosenDifficultyIndex];
                this.delta.Add("fame", cfg.fameFail);
                this.delta.Add("stress", cfg.stressFail);
            }
            else
            {
                // fallback to previous defaults
                this.delta.Add("stress", 4);
            }
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
