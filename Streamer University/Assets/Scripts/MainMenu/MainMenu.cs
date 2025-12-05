using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button quitButton; // assign in inspector
    // Start is called before the first frame update
    void Start()
    {
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void StartGame()
    {
        AudioController.Instance.PlaySelect();
        GameFlowController.Instance.TransitionToScene("StreamScene");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
