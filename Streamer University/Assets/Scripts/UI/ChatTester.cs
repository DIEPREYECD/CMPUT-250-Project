using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatTester : MonoBehaviour
{
    [SerializeField] private float chatRate;

    void Start()
    {
        StartCoroutine(SimulateChat());
    }

    private IEnumerator SimulateChat() {
        while (true) {
            if (PlayerController.Instance.Fame != 0) {
                chatRate = 30f / Mathf.Pow(PlayerController.Instance.Fame * 0.5f, 1.1f); // NEW - Random Calculation - if prevents div by 0.
            } else {
                 chatRate = 1000f; // If fame is 0
            }
            yield return new WaitForSeconds(chatRate);


            ChatBarkSystem.Instance.PushBark();
        }
    }

}
