// MiniGameResult.cs
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct MiniGameResult
{
    public bool success;
    public Dictionary<string, int> delta;
}
