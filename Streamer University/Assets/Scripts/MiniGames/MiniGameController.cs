using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MiniGameController : MonoBehaviour
{
    [Header("Channel to report result on")]
    public MiniGameResultChannel resultChannel;
    protected bool successDeclared = false;


    [Header("Scene name (for unload)")]
    public string mySceneName;

    protected Dictionary<string, int> delta;

    public abstract void FinishMiniGame();

}
