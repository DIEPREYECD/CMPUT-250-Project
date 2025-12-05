using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Note that the SO stands for Scriptable Object
[CreateAssetMenu(fileName = "New Stream Event", menuName = "Stream Events/Stream Event")]
public class StreamEventSO : ScriptableObject
{
    public string title;
    [TextArea(3, 10)]
    public string description;
    public int dStress;
    public int dFame;
}
