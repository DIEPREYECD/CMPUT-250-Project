using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinTrigger : MonoBehaviour
{
    private MiniGameGeoDashController controller;

    private void Start()
    {
        // Find the controller in the scene
        controller = FindObjectOfType<MiniGameGeoDashController>();
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            controller.TriggerWin();
        }
    }
}