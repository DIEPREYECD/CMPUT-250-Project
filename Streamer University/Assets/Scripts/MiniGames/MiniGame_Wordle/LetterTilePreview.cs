using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
[ExecuteAlways]
public class LetterTilePreview : MonoBehaviour
{
    public enum LetterState { Unknown, Absent, Present, Correct }
    public LetterState previewState = LetterState.Unknown;

    public Image background;

    public Color unknownColor = new Color32(30, 30, 30, 255);
    public Color absentColor = new Color32(58, 58, 60, 255);
    public Color presentColor = new Color32(181, 159, 59, 255);
    public Color correctColor = new Color32(83, 141, 78, 255);

    void Update()
    {
        if (background == null) background = GetComponent<Image>();
        if (background == null) return;

        switch (previewState)
        {
            case LetterState.Absent: background.color = absentColor; break;
            case LetterState.Present: background.color = presentColor; break;
            case LetterState.Correct: background.color = correctColor; break;
            default: background.color = unknownColor; break;
        }
    }
}
#endif
