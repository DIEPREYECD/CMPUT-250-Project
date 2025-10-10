using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatTester : MonoBehaviour
{
    [SerializeField] private string[] users;
    [SerializeField] private string[] messages;
    [SerializeField] private float chatRate = 1f;

    void Start()
    {
        StartCoroutine(SimulateChat());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            string user = users[Random.Range(0, users.Length)];
            string message = messages[Random.Range(0, messages.Length)];
            ChatOverlay.Instance.Push(user, message);
        }
    }

    private IEnumerator SimulateChat()
    {
        while (true)
        {
            yield return new WaitForSeconds(chatRate);
            string user = users[Random.Range(0, users.Length)];
            string message = messages[Random.Range(0, messages.Length)];
            ChatOverlay.Instance.Push(user, message);
        }
    }
}
