using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BarUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    private float currentFill;

    public void SetFill(float value)
    {
        if (currentFill == value)
            return;
        fillImage.fillAmount = Mathf.Clamp01(value);

        // Map intensity between 2 and 5 based on how big the change is
        // The min value of the slider is 0, max is 1
        float intensity = Mathf.Abs(currentFill - value) * 3f + 2f;
        ShakeBar(intensity, 0.2f);
        currentFill = value;
    }

    // Each bar has a rect transform
    // So when the setFill is called we can shake the bar to give feedback to the player
    public void ShakeBar(float intensity = 1f, float duration = 0.2f)
    {
        StartCoroutine(ShakeCoroutine(intensity, duration));
    }
    
    private IEnumerator ShakeCoroutine(float intensity, float duration)
    {
        RectTransform rt = GetComponent<RectTransform>();
        Vector2 originalPos = rt.anchoredPosition;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float xOffset = Random.Range(-intensity, intensity);
            float yOffset = Random.Range(-intensity, intensity);
            rt.anchoredPosition = originalPos + new Vector2(xOffset, yOffset);
            yield return null;
        }
        rt.anchoredPosition = originalPos;
    }
}
