using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum LetterState { Unknown, Absent, Present, Correct }


public class MiniGameWordleController : MiniGameController
{
    [Header("Board / Tiles")]
    public RectTransform boardRoot;
    public List<LetterTilePreview> tileBackgrounds = new List<LetterTilePreview>();
    public List<TMP_Text> tileLabels = new List<TMP_Text>();
    public List<Animator> tileAnimators = new List<Animator>();

    [Header("Keyboard")]
    public RectTransform keyboardRoot;
    public List<Button> letterKeys = new List<Button>();
    public Button enterKey;
    public Button backspaceKey;
    public List<Image> keyBackgrounds = new List<Image>();

    [Header("Colors / Visuals")]
    public Color unknownColor = new Color32(30, 30, 30, 255);
    public Color absentColor = new Color32(58, 58, 60, 255);
    public Color presentColor = new Color32(181, 159, 59, 255);
    public Color correctColor = new Color32(83, 141, 78, 255);
    
    Color keyUnknownColor, keyAbsentColor, keyPresentColor, keyCorrectColor;

    [Header("Audio")]
    public AudioSource sfx;
    public AudioClip keyClick, invalid, reveal, win, lose;

    [Header("Word Lists")]
    public TextAsset allowedWordsFile;
    public TextAsset answersFile;

    [Header("UI / Feedback")]
    public TMP_Text titleLabel;
    public TMP_Text statusLabel;
    public CanvasGroup statusToastGroup;

    // Data containers
    private HashSet<string> allowedWords;
    private List<string> answerWords;

    // Minigame deltas
    public int fameDeltaOnWin = 5;
    public int stressDeltaOnWin = -2;
    public int fameDeltaOnLose = -2;
    public int stressDeltaOnLose = 3;

    // Current game state
    private string currentAnswer;
    private int currentRow = 0;
    private int currentCol = 0;
    private char[,] board = new char[6, 5];

    private const float finishDelay = 1.25f;

    private Coroutine toastCo;
    private IEnumerator ShowToast(string msg, float seconds = 1.2f)
    {
        if (statusLabel) statusLabel.text = msg;
        if (!statusToastGroup) yield break;

        statusToastGroup.alpha = 1f;
        yield return new WaitForSeconds(seconds);
        statusToastGroup.alpha = 0f;
    }


    private void LoadWordLists()
    {
        allowedWords = new HashSet<string>();
        answerWords = new List<string>();

        if (allowedWordsFile != null)
        {
            string[] lines = allowedWordsFile.text.Split('\n');
            foreach (string line in lines)
            {
                string w = line.Trim().ToUpper();
                if (w.Length == 5)
                    allowedWords.Add(w);
            }
        }

        if (answersFile != null)
        {
            string[] lines = answersFile.text.Split('\n');
            foreach (string line in lines)
            {
                string w = line.Trim().ToUpper();
                if (w.Length == 5)
                    answerWords.Add(w);
            }
        }

        // Pick a random answer
        if (answerWords.Count > 0)
        {
            currentAnswer = answerWords[Random.Range(0, answerWords.Count)];
            Debug.Log("Today's answer: " + currentAnswer);
        }
    }

    private void OnLetterKey(string letter)
    {
        if (finished) return;
        if (currentCol >= 5 || currentRow >= 6) return;

        board[currentRow, currentCol] = letter[0];
        int tileIndex = currentRow * 5 + currentCol;
        tileLabels[tileIndex].text = letter;
        currentCol++;
    }

    private void OnBackspaceKey()
    {
        if (finished) return;
        if (currentCol <= 0 || currentRow >= 6) return;

        currentCol--;
        board[currentRow, currentCol] = '\0';
        int tileIndex = currentRow * 5 + currentCol;
        tileLabels[tileIndex].text = "";
    }

    private void OnEnterKey()
    {
        if (finished) return;
        if (currentCol < 5) return; // not enough letters

        string guess = new string(Enumerable.Range(0, 5)
                                  .Select(i => board[currentRow, i])
                                  .ToArray());

        guess = guess.ToUpper();

        if (!allowedWords.Contains(guess))
        {
            Debug.Log("Not in word list");
            if (toastCo != null) StopCoroutine(toastCo);
            toastCo = StartCoroutine(ShowToast("Not in word list"));
            // TODO: optional shake row here
            return;
        }

        var states = EvaluateGuess(guess);
        RevealRowColors(states);
        UpdateKeyboardStates(guess, states);

        if (guess == currentAnswer)
        {
            // Play bounce animation on tiles

            successDeclared = true;
            finished = true;
            Debug.Log("You guessed it!");

            StartCoroutine(PlayWinRowAnimation(currentRow));

            Invoke(nameof(FinishMiniGame), finishDelay);
            return;
        }

        currentRow++;
        currentCol = 0;

        if (currentRow >= 6)
        {
            successDeclared = false;
            finished = true;
            Debug.Log("Out of rows! Answer was: " + currentAnswer);
            Invoke(nameof(FinishMiniGame), finishDelay);
        }
    }

    private IEnumerator PlayWinRowAnimation(int row)
    {
        // Safety check
        if (tileAnimators == null || tileAnimators.Count < 5) yield break;

        for (int i = 0; i < 5; i++)
        {
            int tileIndex = row * 5 + i;
            if (tileIndex >= 0 && tileIndex < tileAnimators.Count)
            {
                var anim = tileAnimators[tileIndex];
                if (anim != null)
                {
                    anim.ResetTrigger("Win"); // safety
                    anim.SetTrigger("Win");
                }
            }

            // small time wait for wave effect
            yield return new WaitForSeconds(0.06f);
        }
    }

    private void UpdateKeyboardStates(string guess, LetterState[] states)
    {
        // Update keyboard key colors based on the latest guess
        for (int i = 0; i < 5; i++)
        {
            string letter = guess[i].ToString();
            LetterState state = states[i];
            // Find the corresponding key button
            var keyBtn = letterKeys.Find(btn => btn.GetComponentInChildren<TMP_Text>().text.ToUpper() == letter);
            if (keyBtn != null)
            {
                var keyBg = keyBtn.GetComponent<Image>();
                if (keyBg != null)
                {
                    Color targetColor = keyUnknownColor;
                    switch (state)
                    {
                        case LetterState.Correct:
                            targetColor = keyCorrectColor;
                            break;
                        case LetterState.Present:
                            targetColor = keyPresentColor;
                            break;
                        case LetterState.Absent:
                            targetColor = keyAbsentColor;
                            break;
                    }
                    // Only upgrade color if it's a higher state
                    Color currentColor = keyBg.color;
                    if (state == LetterState.Correct ||
                        (state == LetterState.Present && currentColor != keyCorrectColor) ||
                        (state == LetterState.Absent && currentColor != keyCorrectColor && currentColor != keyPresentColor))
                    {
                        keyBg.color = targetColor;
                    }
                }
            }
        }
    }

    private void SetupKeyboard()
    {

        foreach (var btn in letterKeys)
        {
            string letter = btn.GetComponentInChildren<TMP_Text>().text.ToUpper();
            btn.onClick.AddListener(() => OnLetterKey(letter));
        }

        if (enterKey) enterKey.onClick.AddListener(OnEnterKey);
        if (backspaceKey) backspaceKey.onClick.AddListener(OnBackspaceKey);
    }

    private LetterState[] EvaluateGuess(string guess)
    {
        LetterState[] result = new LetterState[5];
        bool[] answerUsed = new bool[5];
        guess = guess.ToUpper();

        // Pass 1 – mark correct letters
        for (int i = 0; i < 5; i++)
        {
            if (guess[i] == currentAnswer[i])
            {
                result[i] = LetterState.Correct;
                answerUsed[i] = true;
            }
            else
            {
                result[i] = LetterState.Unknown; // temp
            }
        }

        // Pass 2 – mark presents
        for (int i = 0; i < 5; i++)
        {
            if (result[i] == LetterState.Correct) continue;

            bool found = false;
            for (int j = 0; j < 5; j++)
            {
                if (!answerUsed[j] && guess[i] == currentAnswer[j])
                {
                    found = true;
                    answerUsed[j] = true;
                    break;
                }
            }
            result[i] = found ? LetterState.Present : LetterState.Absent;
        }

        // Log for debugging
        string debugStr = "Evaluating guess '" + guess + "': ";
        for (int i = 0; i < 5; i++)
        {
            debugStr += result[i].ToString() + " ";
        }

        return result;
    }

    private void RevealRowColors(LetterState[] states)
    {
        for (int i = 0; i < 5; i++)
        {
            int tileIndex = currentRow * 5 + i;
            var letterTilePreview = tileBackgrounds[tileIndex];
            var label = tileLabels[tileIndex];

            switch (states[i])
            {
                case LetterState.Correct:
                    letterTilePreview.previewState = LetterTilePreview.LetterState.Correct;
                    break;
                case LetterState.Present:
                    letterTilePreview.previewState = LetterTilePreview.LetterState.Present;
                    break;
                case LetterState.Absent:
                    letterTilePreview.previewState = LetterTilePreview.LetterState.Absent;
                    break;
                default:
                    letterTilePreview.previewState = LetterTilePreview.LetterState.Unknown;
                    break;
            }

            // make text pop a bit for readability
            label.color = Color.white;

            // Log for debugging
            Debug.Log($"Tile {tileIndex} '{label.text}' set to {states[i]}");
        }
    }

    private void ClearBoardVisuals()
    {
        for (int i = 0; i < tileLabels.Count; i++)
        {
            tileLabels[i].text = "";
            if (i < tileBackgrounds.Count)
                tileBackgrounds[i].previewState = LetterTilePreview.LetterState.Unknown;
        }

        System.Array.Clear(board, 0, board.Length);
        currentRow = 0;
        currentCol = 0;
        finished = false;
        successDeclared = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        keyUnknownColor = unknownColor;
        keyAbsentColor = absentColor;
        keyPresentColor = presentColor;
        keyCorrectColor = correctColor;
        LoadWordLists();
        SetupKeyboard();
        ClearBoardVisuals();

        // required by MiniGameController pattern
        this.delta = new Dictionary<string, int>();

        // If you don’t want to set mySceneName by hand:
        if (string.IsNullOrEmpty(mySceneName))
            mySceneName = gameObject.scene.name;

        Debug.Log($"[Wordle] Answer selected: {currentAnswer}");
    }

    // Update is called once per frame
    void Update()
    {
        // Accept keyboard input also
        if (Input.anyKeyDown)
        {
            foreach (char c in Input.inputString)
            {
                if (char.IsLetter(c))
                {
                    OnLetterKey(char.ToUpper(c).ToString());
                }
                else if (c == '\b') // backspace
                {
                    OnBackspaceKey();
                }
                else if (c == '\n' || c == '\r') // enter
                {
                    OnEnterKey();
                }
            }
        }
    }

    // ==== Integration back into main game ====

    public override void FinishMiniGame()
    {
        bool success = successDeclared;
        Debug.Log($"[Wordle] FinishMiniGame success={success}");

        // Your fame/stress deltas; tweak to taste
        var setFlags = new List<string>();
        if (success)
        {
            this.delta["fame"] = fameDeltaOnWin;
            this.delta["stress"] = stressDeltaOnWin;
            setFlags.Add("wordleWin");
            EventManager.Instance.addToQueue("EVT_WORDLE_WIN"); // use IDs you have
        }
        else
        {
            this.delta["fame"] = fameDeltaOnLose;
            this.delta["stress"] = stressDeltaOnLose;
            setFlags.Add("wordleLose");
            EventManager.Instance.addToQueue("EVT_WORDLE_LOSE");
        }

        // Report result through your SO channel
        var result = new MiniGameResult { success = success, delta = this.delta };
        if (resultChannel != null) resultChannel.Raise(result);

        // Flags and cleanup
        EventManager.Instance.setFlags(setFlags);

        // Unload this additive scene and focus the main one (same as Clicker)
        MiniGameLoader.UnloadMiniGame(mySceneName);
        var main = SceneManager.GetSceneByName("StreamScene");
        if (main.IsValid()) SceneManager.SetActiveScene(main);
    }
}
