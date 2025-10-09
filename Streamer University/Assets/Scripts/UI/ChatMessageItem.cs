using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class ChatMessageItem : MonoBehaviour
{

    TextMeshProUGUI text;

    void Awake() {
        text = GetComponent<TextMeshProUGUI>();
    }
    public void Set(string user, string message) {
        text.text = ChatFormat.FormatLine(user, message);
    }
}
