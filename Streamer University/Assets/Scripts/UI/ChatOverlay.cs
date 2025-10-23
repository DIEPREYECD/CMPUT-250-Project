using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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


    public void Push(string user, string message)
    {
        if (gameObject.activeSelf == false)
        {
            return;
        }

        var scroll = GetComponent<UnityEngine.UI.ScrollRect>();

        ChatMessageItem item = Instantiate(itemPrefab, content);
        item.Set(user, message);

        active.Enqueue(item);

        bool autoScroll = scroll.verticalNormalizedPosition < 0.001f;
        if (autoScroll) StartCoroutine(AutoScrollChat());

    }

    private IEnumerator AutoScrollChat() {
        var scroll = GetComponent<UnityEngine.UI.ScrollRect>();
        yield return new WaitForEndOfFrame();
        scroll.verticalNormalizedPosition = 0f;
    }


    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }
}