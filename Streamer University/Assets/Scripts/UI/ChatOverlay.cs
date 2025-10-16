using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatOverlay : MonoBehaviour
{
    public static ChatOverlay Instance { get; private set; }

    [SerializeField] private Transform content;
    [SerializeField] private ChatMessageItem itemPrefab;
    [SerializeField] private ScrollRect scroll;
    public int maxItems = 15;
    private bool autoScroll = true;

    private readonly Queue<ChatMessageItem> active = new Queue<ChatMessageItem>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void Update()
    {
        // Make the scroll rect go to bottom when new messages come in
        if (scroll.verticalNormalizedPosition <= 0.15f)
            autoScroll = true;
        else {
            autoScroll = false;
        }
    }

    public void Push(string user, string message)
    {
        if (gameObject.activeSelf == false)
        {
            return;
        }

        ChatMessageItem item = Instantiate(itemPrefab, content);
        item.Set(user, message);

        active.Enqueue(item);
        if (autoScroll) {
            StartCoroutine(ScrollBottom());
        }
    }

    private IEnumerator ScrollBottom() {
        yield return new WaitForEndOfFrame();

        Canvas.ForceUpdateCanvases();
        scroll.verticalNormalizedPosition = 0f;
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }
}
