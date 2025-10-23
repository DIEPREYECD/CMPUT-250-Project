using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatTester : MonoBehaviour
{
    public PlayerStatsSO playerStats;
    [SerializeField] private float chatRate;

    void Start()
    {
        StartCoroutine(SimulateChat());
    }


    private IEnumerator SimulateChat() {
        while (true) {
            chatRate = 30f / Mathf.Max(1f, Mathf.Pow((playerStats.Fame * 0.4f), 1.5f));
            yield return new WaitForSeconds(chatRate);

            ChatBarkSystem.Instance.PushBark();
        }
    }

}
