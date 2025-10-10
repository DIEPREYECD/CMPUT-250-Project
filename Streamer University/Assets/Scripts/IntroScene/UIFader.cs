using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIFader : MonoBehaviour
{
    public Image overlay;

    public IEnumerator Fade(float from, float to, float dur)
    {
        Color c = overlay.color;
        for (float t = 0; t < dur; t += Time.deltaTime)
        {
            float a = Mathf.Lerp(from, to, t / dur);
            overlay.color = new Color(c.r, c.g, c.b, a);
            yield return null;
        }
        overlay.color = new Color(c.r, c.g, c.b, to);
    }
}
