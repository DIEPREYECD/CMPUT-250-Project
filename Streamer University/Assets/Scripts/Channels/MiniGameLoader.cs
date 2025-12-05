using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class MiniGameLoader : MonoBehaviour
{
    private static MiniGameLoader _instance;

    public static MiniGameLoader Instance { get { return _instance; } }

    [Tooltip("Name of the MiniGame scene (must be added in Build Settings)")]
    private string miniGameSceneName = null;

    [Header("Wire the shared channel asset here")]
    [SerializeField] private MiniGameResultChannel resultChannel;

    private bool _waiting;
    private MiniGameResult _result;

    public void Awake()
    {
        _instance = this;
    }

    public Coroutine RunMiniGame(string sceneName)
    {
        return StartCoroutine(RunMiniGameRoutine(sceneName));
    }

    private IEnumerator RunMiniGameRoutine(string sceneName)
    {
        // enter modal state
        GameFlowController.Instance.SetState(GameState.Minigame); // needs GameFlowController in Intro scene
        LaunchMiniGame(sceneName); // existing method

        _waiting = true;
        UnityAction<MiniGameResult> handler = (MiniGameResult r) =>
        {
            Debug.Log($"MiniGameLoader: Received minigame result: success={r.success}");
            _result = r;
            _waiting = false;
        };
        resultChannel.OnRaised += handler;

        // wait until minigame sends result
        while (_waiting) yield return null;

        // stop listening BEFORE unload to avoid leaks
        resultChannel.OnRaised -= handler;

        // unload
        UnloadMiniGame(miniGameSceneName);

        // apply deltas (centralized here)
        int deltaFame = _result.delta != null && _result.delta.ContainsKey("fame") ? _result.delta["fame"] : 0;
        int deltaStress = _result.delta != null && _result.delta.ContainsKey("stress") ? _result.delta["stress"] : 0;
        PlayerController.Instance.ApplyDelta(deltaFame, deltaStress);

        // back to main gameplay
        var mainScene = SceneManager.GetSceneByName("StreamScene");
        if (mainScene.IsValid())
        {
            SceneManager.SetActiveScene(mainScene);
            AudioController.Instance.checkScene(); // resume main gameplay music
        }

        GameFlowController.Instance.SetState(GameState.MainGameplay);
    }

    public void LaunchMiniGame(string mgsceneName)
    {
        if (!doesMiniGameExist(mgsceneName))
        {
            Debug.LogError($"MiniGame scene '{mgsceneName}' does not exist or is not added to Build Settings.");
            return;
        }
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

    public bool doesMiniGameExist(string mgsceneName)
    {
        return Application.CanStreamedLevelBeLoaded(mgsceneName);
    }

}
