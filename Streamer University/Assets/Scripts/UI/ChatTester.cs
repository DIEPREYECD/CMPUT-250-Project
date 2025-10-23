using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatTester : MonoBehaviour
{
    [SerializeField] private float chatRate = 1f;

    void Start()
    {
        StartCoroutine(SimulateChat());
    }

    private IEnumerator SimulateChat() {
        while (true) {
            yield return new WaitForSeconds(chatRate);


            ChatBarkSystem.Instance.PushBark();
        }
    }

}
