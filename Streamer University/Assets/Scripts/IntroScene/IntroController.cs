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

    public string gameTitle = "Streamer University";
    public string introText =
        "“It all started one late night… scrolling through clips of Speed(the Streamer of the Year) shattering mics and records. Our hero—armed with a jittery webcam and louder dreams—decided: why not me? Welcome to Streamer University: where viral moments are made, and sanity sometimes goes live.”";

    public string hintText = "Press any key to continue…";

    // Make this a singleton
    public static IntroController Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    void Start() => StartCoroutine(Sequence());

    IEnumerator Sequence()
    {
        // Initial state
        readyToContinue = false;
        title.gameObject.SetActive(false);
        continueHint.gameObject.SetActive(false);
        typewriter.gameObject.SetActive(false);

        // Fade from black
        yield return fader.StartCoroutine(fader.Fade(1f, 0f, fadeDuration));

        // Title appear
        title.gameObject.SetActive(true);
        title.text = gameTitle;
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
        continueHint.text = hintText;
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
        AudioController.Instance.PlaySelect();
        yield return fader.StartCoroutine(fader.Fade(0f, 1f, fadeDuration));
        GameFlowController.Instance.TransitionToScene(nextSceneName);
    }
}
