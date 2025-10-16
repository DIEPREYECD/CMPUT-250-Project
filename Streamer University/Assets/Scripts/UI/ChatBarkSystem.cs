using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ChatBarkSystem;

public class ChatBarkSystem : MonoBehaviour
{

    public TextAsset ChatBarkCSV;
    public PlayerStatsSO playerStats;

    public static ChatBarkSystem Instance {get; private set; }

    [System.Serializable]
    public class BarkEntry
    {
        public string id;
        public string user;
        public string text;
        public bool def;
        public bool stress;
        public bool fame;
    }

    [System.Serializable]
    public class BarkEntryList
    {
        public BarkEntry[] entries;
    }

    private List<BarkEntry> barkList = new List<BarkEntry>();

    // Awake
    void Awake() {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        ParseDialogueText();
    }

    void ParseDialogueText()
    {
        string[] lines = ChatBarkCSV.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] splitRow = lines[i].Split(',');

            if (splitRow.Length >= 6)
            {
                BarkEntry entry = new BarkEntry
                {
                    id = splitRow[0].Trim(),
                    user = splitRow[1].Trim(),
                    text = splitRow[2].Trim(),
                    def = splitRow[3].Trim() == "1",
                    stress = splitRow[4].Trim() == "1",
                    fame = splitRow[5].Trim() == "1"
                };

                barkList.Add(entry);
            }
        }
    }

    // Determines eligiblity of text being in 
    public bool Eligibility(BarkEntry entry) {
        // Automatically eligible if default
        if (entry.def) return true;

        bool stressOverHalf;
        if (entry.stress) {
            stressOverHalf = playerStats.Stress >= 50;
        }
        else {
            stressOverHalf = playerStats.Stress < 50;
        }

        bool fameOverHalf;
        if (entry.fame) {
            fameOverHalf = playerStats.Fame >= 50;
        } else {
            fameOverHalf = playerStats.Fame < 50;
        }

        return stressOverHalf && fameOverHalf;
    }


}
