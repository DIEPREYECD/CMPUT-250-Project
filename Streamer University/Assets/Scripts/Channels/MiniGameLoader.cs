using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MiniGameLoader : MonoBehaviour
{
    private static MiniGameLoader _instance;

    public static MiniGameLoader Instance { get { return _instance; } }

    [Tooltip("Name of the MiniGame scene (must be added in Build Settings)")]
    private string miniGameSceneName = null;

    public void Awake()
    {
        _instance = this;
    }

    public void LaunchMiniGame(string mgsceneName)
    {
        miniGameSceneName = mgsceneName;
        StartCoroutine(LoadMiniGameAdditive());
    }

    private IEnumerator LoadMiniGameAdditive()
    {
        // Loading the minigame scene, we can do a blur animation on the main scene here
        var op = SceneManager.LoadSceneAsync(miniGameSceneName, LoadSceneMode.Additive);
        while (!op.isDone) yield return null;

        // Make the mini game scene active
        Scene mini = SceneManager.GetSceneByName(miniGameSceneName);
        if (mini.IsValid()) SceneManager.SetActiveScene(mini);
    }

    public static void UnloadMiniGame(string miniGameSceneName)
    {
        if (SceneManager.GetSceneByName(miniGameSceneName).IsValid())
            SceneManager.UnloadSceneAsync(miniGameSceneName);

        miniGameSceneName = null;
    }

    public bool isRunningGame() => miniGameSceneName != null;

}
