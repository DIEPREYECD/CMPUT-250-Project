using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BarUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    private float currentFill;

    [SerializeField]
    private TMP_Text deltaText; // This Text object will display the change in value specified

    private void Start()
    {
        if (deltaText != null)
        {
            deltaText.gameObject.SetActive(false);
        }
    }

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

    public void ShowDelta(int deltaValue, float displayDuration = 1f)
    {
        StartCoroutine(ShowDeltaCoroutine(deltaValue, displayDuration));
    }

    private IEnumerator ShowDeltaCoroutine(int deltaValue, float displayDuration)
    {
        deltaText.text = (deltaValue > 0 ? "+" : "-") + deltaValue.ToString();
        deltaText.gameObject.SetActive(true);
        yield return new WaitForSeconds(displayDuration);
        deltaText.gameObject.SetActive(false);
    }
}