using System.Collections;
using System.Text;
using UnityEngine;
using TMPro;

public class TypewriterTMP : MonoBehaviour
{
    public TMP_Text text;
    [TextArea(3, 10)]
    public string fullText;
    public float charsPerSecond = 40f;
    public bool isTyping { get; private set; }

    Coroutine routine;
    Coroutine initRoutine;
    string bakedText;
    void OnEnable()
    {
        // Try to auto-assign TMP_Text if not set in inspector
        if (text == null)
            text = GetComponent<TMP_Text>();

        if (text == null)
        {
            Debug.LogError("TypewriterTMP requires a TMP_Text assigned or present on the same GameObject.", this);
            return;
        }

        // Start an init coroutine that waits for IntroController to be ready before starting the typewriter.
        if (initRoutine != null) StopCoroutine(initRoutine);
        initRoutine = StartCoroutine(InitializeAndRun());
    }

    void OnDisable()
    {
        if (initRoutine != null) StopCoroutine(initRoutine);
        if (routine != null) StopCoroutine(routine);
        isTyping = false;
    }

    IEnumerator InitializeAndRun()
    {
        // Wait briefly for IntroController.Instance to be assigned, but don't wait forever.
        float wait = 0f;
        const float timeout = 5f; // seconds
        while (IntroController.Instance == null && wait < timeout)
        {
            wait += Time.deltaTime;
            yield return null;
        }

        if (IntroController.Instance != null)
            fullText = IntroController.Instance.introText;
        else
            Debug.LogWarning("IntroController.Instance not found after waiting; using inspector 'fullText'.", this);

        // Bake and start typing
        bakedText = BakeLineBreaks(fullText);

        text.enableWordWrapping = false;
        text.text = bakedText;
        text.maxVisibleCharacters = 0;

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        isTyping = true;
        float t = 0f;
        int visible = 0;
        int total = text.textInfo.characterCount; // counts visible glyphs

        while (visible < total)
        {
            if (Input.anyKeyDown) break;

            t += Time.deltaTime * charsPerSecond;
            int next = Mathf.Clamp(Mathf.FloorToInt(t), 0, total);
            if (next != visible)
            {
                visible = next;
                text.maxVisibleCharacters = visible;
            }
            yield return null;
        }

        text.maxVisibleCharacters = int.MaxValue;
        isTyping = false;
    }

    public void Skip()
    {
        if (routine != null) StopCoroutine(routine);
        text.maxVisibleCharacters = int.MaxValue;
        isTyping = false;
    }

    // —— Bake TMP's chosen wraps into the string so they don't change during typing ——
    string BakeLineBreaks(string src)
    {
        // Lay out once using normal wrapping
        bool prevWrap = text.enableWordWrapping;
        var prevText = text.text;

        text.enableWordWrapping = true;
        text.text = src;
        text.ForceMeshUpdate();

        var ti = text.textInfo;
        if (ti.lineCount <= 1)
        {
            // Restore and return original if no wrap needed
            text.text = prevText;
            text.enableWordWrapping = prevWrap;
            return src;
        }

        // Mark positions right after each line’s last visible character
        var sb = new StringBuilder(src.Length + ti.lineCount * 2);
        int breakIdx = 0;

        // Collect break positions in source-string indices
        // (Use lastVisibleCharacterIndex so we don't break mid-whitespace/punctuation)
        var breaks = new int[ti.lineCount - 1];
        for (int i = 0; i < ti.lineCount - 1; i++)
        {
            breaks[i] = ti.lineInfo[i].lastVisibleCharacterIndex + 1;
        }

        // Rebuild string, inserting '\n' at wrap points and trimming a single preceding space
        for (int i = 0; i < src.Length; i++)
        {
            // If this is a wrap insertion point, inject newline *before* copying src[i]
            while (breakIdx < breaks.Length && i == breaks[breakIdx])
            {
                // If we just added a space, remove it so we don't start the next line with space
                if (sb.Length > 0 && sb[sb.Length - 1] == ' ')
                    sb.Length -= 1;

                sb.Append('\n');
                breakIdx++;
            }

            sb.Append(src[i]);
        }

        // Restore original TMP state
        text.text = prevText;
        text.enableWordWrapping = prevWrap;

        return sb.ToString();
    }
}
