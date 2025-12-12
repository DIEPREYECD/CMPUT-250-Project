using UnityEngine;
using UnityEngine.EventSystems;

public class Node : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private bool isDragging = false;

    public int nodeIndex; // Which node number this is

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Don't allow dragging if game is finished
        if (DetangleController.Instance != null && DetangleController.Instance.IsGameFinished)
            return;
        isDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Don't allow dragging if game is finished
        if (DetangleController.Instance != null && DetangleController.Instance.IsGameFinished)
            return;

        if (isDragging)
        {
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;

            // Tell the game controller that a node moved
            if (DetangleController.Instance != null)
            {
                DetangleController.Instance.OnNodeMoved();
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
    }
}