using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatOverlay : MonoBehaviour
{
    public static ChatOverlay Instance { get; private set; }

    [SerializeField] private Transform content;
    [SerializeField] private ChatMessageItem itemPrefab;
    public int maxItems = 15;

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
        var scroll = GetComponent<UnityEngine.UI.ScrollRect>();
        if (scroll.verticalNormalizedPosition <= 0.15f)
            scroll.verticalNormalizedPosition = 0f;
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
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }
}
