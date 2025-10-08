using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatOverlay : MonoBehaviour
{
    [SerializeField] private Transform content;
    [SerializeField] private ChatMessageItem itemPrefab;
    public int maxItems = 15;

    private readonly Queue<ChatMessageItem> active = new Queue<ChatMessageItem>();

    public void Push(string user, string message) {
        ChatMessageItem item = Instantiate(itemPrefab, content);

        // if (active.Count >= maxItems) {
        //     item = active.Dequeue();
        // } else {
        //     item = Instantiate(itemPrefab, content);
        // }

        item.Set(user, message); 

        active.Enqueue(item);

        // if (active.Count > maxItems) {
        //     ChatMessageItem old = active.Dequeue();
        //     Destroy(old.gameObject);
        // }
    }
}
