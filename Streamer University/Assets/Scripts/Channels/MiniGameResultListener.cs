using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGameResultListener : MonoBehaviour
{
    [Header("Wiring")]
    public MiniGameResultChannel resultChannel;
    public PlayerStatsSO playerStats;

    private void OnEnable()
    {
        if (resultChannel != null)
            resultChannel.OnRaised += OnMiniGameResult;
    }

    private void OnDisable()
    {
        if (resultChannel != null)
            resultChannel.OnRaised -= OnMiniGameResult;
    }

    private void OnMiniGameResult(MiniGameResult result)
    {
        int deltaFame = result.delta.ContainsKey("fame") ? result.delta["fame"] : 0;
        int deltaStress = result.delta.ContainsKey("stress") ? result.delta["stress"] : 0;


        if (playerStats != null)
            playerStats.ApplyDelta(deltaFame, deltaStress);

        Debug.Log($"MiniGame finished. Success={result.success} | deltaFame: {deltaFame}, deltaStress: {deltaStress}");
    }
}
