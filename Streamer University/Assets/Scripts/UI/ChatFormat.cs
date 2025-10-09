using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ChatFormat
{   
    private static readonly Color[] Palette = {
        new Color(1.0f, 0.0f, 0.0f), //Red
        new Color(0.0f, 1.0f, 0.0f), //Green
        new Color(0.0f, 0.0f, 1.0f), //Blue
    };

    public static string FormatLine(string user, string message) {
        var col = ColorFor(user);
        var hex = ColorUtility.ToHtmlStringRGB(col);
        return $"<color=#{hex}>{user}</color>: {message}";  
    }

    public static Color ColorFor(string user) {
        int hash = 23;
        foreach (char c in user) {
            hash = hash * 31 + c;
        }
        var index = Mathf.Abs(hash) % Palette.Length;
        return Palette[index];
    }
}
