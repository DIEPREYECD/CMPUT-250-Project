using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Note that the SO stands for Scriptable Object
[CreateAssetMenu(fileName = "PlayerStats", menuName = "Player Stats")]
public class PlayerStatsSO : ScriptableObject
{
    public int Fame = 20;
    public int Stress = 10;

    public void ApplyDelta(int dFame, int dStress)
    {
        Debug.Log($"Applying stats delta: Fame {dFame}, Stress {dStress}");
        Fame += dFame;
        Stress += dStress;
        Debug.Log($"New stats: Fame {Fame}, Stress {Stress}");
        Fame = Mathf.Max(Fame, 0);
        Stress = Mathf.Clamp(Stress, 0, 100);
    }

    public void ResetStats()
    {
        Fame = 20;
        Stress = 10;
    }
}