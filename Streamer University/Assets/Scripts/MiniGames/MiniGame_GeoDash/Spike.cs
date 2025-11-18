using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Spike : MonoBehaviour
{
    private MiniGameGeoDashController controller;

    private void Start()
    {
        controller = FindObjectOfType<MiniGameGeoDashController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            controller.TriggerLose();
        }
    }
}
