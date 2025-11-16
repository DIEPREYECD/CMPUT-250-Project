using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlowController : MonoBehaviour
{
    public static GameFlowController Instance { get; private set; }
    public GameState CurrentState { get; private set; } = GameState.Title;
    private GameEndings currentEnding;

    private CanvasGroup canvasGroup;
    public float fadeDuration = 0.75f;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            Debug.LogError("GameFlowController is missing a CanvasGroup component!");
        }
    }

    public void SetState(GameState next) => CurrentState = next;

    public void SetEnding(GameEndings ending) => currentEnding = ending;
    public GameEndings GetEnding() => currentEnding;

    public void Start()
    {
        StartCoroutine(Fade(0f));
    }

    private void Update()
    {

    }

    public void TransitionToScene(string sceneName)
    {
        StartCoroutine(FadeAndLoadRoutine(sceneName));
    }

    private IEnumerator FadeAndLoadRoutine(string sceneName)
    {
        // 1. Fade Out (to Alpha 1)
        yield return StartCoroutine(Fade(1f));

        // 2. Load the Scene
        SceneManager.LoadScene(sceneName);

        // 3. Fade In (to Alpha 0)
        yield return StartCoroutine(Fade(0f));
    }

    private IEnumerator Fade(float targetAlpha)
    {
        // Block all mouse clicks during fade
        canvasGroup.blocksRaycasts = true;

        float time = 0;
        float startAlpha = canvasGroup.alpha;

        while (time < fadeDuration)
        {
            // Lerp (linearly interpolate) the alpha value
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);

            // Increment time
            time += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        // Ensure the target alpha is set
        canvasGroup.alpha = targetAlpha;

        // Unblock raycasts if we faded in (alpha is 0)
        if (targetAlpha == 0f)
        {
            canvasGroup.blocksRaycasts = false;
        }
    }
}

public enum GameState { Title, MainMenu, MainGameplay, Minigame, Paused }
public enum GameEndings { NoFame, MaxStress, LowStressMaxFame, HighStressMaxFame }
