using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class IntroController : MonoBehaviour
{
    [Header("Refs")]
    public UIFader fader;
    public TypewriterTMP typewriter;
    public TMP_Text title;
    public TMP_Text continueHint;

    [Header("Config")]
    public string nextSceneName = "MainMenu";
    public float fadeDuration = 0.6f;
    public float hintPulseSpeed = 3f;

    bool readyToContinue;

    void Start() => StartCoroutine(Sequence());

    IEnumerator Sequence()
    {
        // Initial state
        readyToContinue = false;
        title.gameObject.SetActive(false);
        continueHint.gameObject.SetActive(false);
        typewriter.gameObject.SetActive(false);
        continueHint.text = "Press any key to continue…";

        // Fade from black
        yield return fader.StartCoroutine(fader.Fade(1f, 0f, fadeDuration));

        // Title appear
        title.gameObject.SetActive(true);
        title.text = "Streamer University";
        title.alpha = 0;
        for (float t = 0; t < 0.4f; t += Time.deltaTime)
        {
            title.alpha = Mathf.SmoothStep(0, 1, t / 0.4f);
            yield return null;
        }
        title.alpha = 1;

        // Typewriter begins
        typewriter.gameObject.SetActive(true);

        // Wait for typing or skip
        while (typewriter.isTyping)
        {
            if (Input.anyKeyDown) typewriter.Skip();
            yield return null;
        }

        // Show continue hint
        continueHint.gameObject.SetActive(true);
        readyToContinue = true;

        // Wait a bit before accepting input
        yield return new WaitForSeconds(0.5f);
        // Pulse hint while waiting
        while (true)
        {
            continueHint.alpha = 0.6f + 0.4f * Mathf.Sin(Time.unscaledTime * hintPulseSpeed);
            if (Input.anyKeyDown) break;
            yield return null;
        }

        // Fade to black and load next scene
        yield return fader.StartCoroutine(fader.Fade(0f, 1f, fadeDuration));
        SceneManager.LoadScene(nextSceneName);
    }
}
