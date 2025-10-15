using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatsHUDController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Slider fameBar;
    [SerializeField] private Slider stressBar;
    [SerializeField] private TMP_Text fameText;
    [SerializeField] private TMP_Text stressText;

    private void Start()
    {
        // Make sure sliders are normalized (0-1)
        if (fameBar != null) fameBar.minValue = 0;
        if (fameBar != null) fameBar.maxValue = 1;

        if (stressBar != null) stressBar.minValue = 0;
        if (stressBar != null) stressBar.maxValue = 1;

        RefreshUI();
    }

    private void Update()
    {
        // For prototype, just update each frame
        RefreshUI();
    }

    private void RefreshUI()
    {

        // Fame: no upper cap for now, so just show raw value and clamp bar to [0,1]
        if (fameBar != null)
            fameBar.value = Mathf.Clamp01(player.Instance.Fame / 100f);

        if (fameText != null)
            fameText.text = $"Fame: {player.Instance.Fame}";

        // Stress: always 0–100
        if (stressBar != null)
            stressBar.value = player.Instance.Stress / 100f;

        if (stressText != null)
            stressText.text = $"Stress: {player.Instance.Stress}%";
    }
}
