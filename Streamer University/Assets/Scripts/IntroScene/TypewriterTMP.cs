using System.Collections;
using System.Text;
using UnityEngine;
using TMPro;

public class TypewriterTMP : MonoBehaviour
{
    public TMP_Text text;
    [TextArea(3, 10)]
    public string fullText =
        "“It all started one late night… scrolling through clips of Speed(the Streamer of the Year) shattering mics and records. Our hero—armed with a jittery webcam and louder dreams—decided: why not me? Welcome to Streamer University: where viral moments are made, and sanity sometimes goes live.”";

    public float charsPerSecond = 40f;
    public bool isTyping { get; private set; }

    Coroutine routine;
    string bakedText;

    void OnEnable()
    {
        // Bake the final line breaks based on the current box, font, size, etc.
        bakedText = BakeLineBreaks(fullText);

        // Lock wrapping so lines won’t reflow while typing.
        text.enableWordWrapping = false;
        text.text = bakedText;

        // Start with nothing visible; reveal via maxVisibleCharacters.
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
            // AudioController.Instance.PlayBeep();
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
        int srcIndex = 0;
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
