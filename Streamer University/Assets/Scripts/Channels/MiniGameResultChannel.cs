using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Channels/MiniGame Result Channel")]
public class MiniGameResultChannel : ScriptableObject
{
    public UnityAction<MiniGameResult> OnRaised;
    
    public void Raise(MiniGameResult result)
    {
        if (OnRaised != null) OnRaised.Invoke(result);
    }
}
