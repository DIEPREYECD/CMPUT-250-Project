using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BarUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;

    public void SetFill(float value)
    {
        fillImage.fillAmount = Mathf.Clamp01(value);
    }
}
