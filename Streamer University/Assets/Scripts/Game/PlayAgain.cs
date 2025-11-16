using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayAgain : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void playAgain() {
        for (int i = 0; i < 100; i++) {
            Debug.Log("playagain clicked");
        }
        GameFlowController.Instance.TransitionToScene("IntroScene");
    }
}
