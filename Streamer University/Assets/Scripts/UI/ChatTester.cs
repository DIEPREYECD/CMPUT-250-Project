using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatTester : MonoBehaviour
{
    [SerializeField] private ChatOverlay overlay;
    [SerializeField] private string[] users;
    [SerializeField] private string[] messages;
    [SerializeField] private float chatRate = 1f;

    void Start() {
        int maxChats = overlay.maxItems;
        for (int i = 0; i < maxChats; i++) {
            string user = users[Random.Range(0, users.Length)];
            string message = messages[Random.Range(0, messages.Length)];
            overlay.Push(user, message);
        }
        // StartCoroutine(ChatLoop());
        // overlay.Push("AlyE", "Mr. Fish Studiosasdddddddddddddddddddddddddddddddddddddddddd");
        // overlay.Push("Hello", "Mr. Man");
        // overlay.Push("Charles", "This sucks.");
    }

    // IEnumerator ChatLoop() {
    //     while (true) {
    //         string user = users[Random.Range(0, users.Length)];
    //         string message = messages[Random.Range(0, messages.Length)];
    //         overlay.Push(user, message);
    //         yield return new WaitForSeconds(chatRate);
    //     }
    // }
}
