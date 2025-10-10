using System.Collections;
using UnityEngine;
using TMPro;

public class TypewriterTMP : MonoBehaviour
{
    public TMP_Text text;
    [TextArea(3, 10)] public string fullText = "“It all started one late night… scrolling through clips of Speed(the Streamer of the Year) shattering mics and records. Our hero—armed with a jittery webcam and louder dreams—decided: why not me? Welcome to Streamer University: where viral moments are made, and sanity sometimes goes live.”";
    public float charsPerSecond = 40f;
    public bool isTyping { get; private set; }

    Coroutine routine;

    void OnEnable()
    {
        text.text = "";
        routine = StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        isTyping = true;
        float t = 0f;
        int len = 0;
        while (len < fullText.Length)
        {
            // Skip on input
            if (Input.anyKeyDown) break;

            t += Time.deltaTime * charsPerSecond;
            int nextLen = Mathf.Clamp(Mathf.FloorToInt(t), 0, fullText.Length);
            if (nextLen != len)
            {
                len = nextLen;
                text.text = fullText.Substring(0, len);
            }
            yield return null;
        }
        text.text = fullText;
        isTyping = false;
    }

    public void Skip()
    {
        if (routine != null) StopCoroutine(routine);
        text.text = fullText;
        isTyping = false;
    }
}
