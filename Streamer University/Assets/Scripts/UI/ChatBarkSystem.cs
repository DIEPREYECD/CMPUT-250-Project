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

        /*
        Checks for the eligibility of a bark at a time.
        If default is 1, return true.
        Checks if stress and fame meet requirements.
        Return stress and fame conditions at the end.
        */

        // Automatically eligible if default
        if (entry.def) return true;

        bool stressCond;
        if (entry.stress) {
            stressCond = playerStats.Stress >= 50;
        }
        else {
            stressCond = playerStats.Stress < 50;
        }

        bool fameCond;
        if (entry.fame) {
            fameCond = playerStats.Fame >= 50;
        } else {
            fameCond = playerStats.Fame < 50;
        }

        return stressCond && fameCond;
    }

    public BarkEntry GetBark() {

        /*
        Go through each entry in bark list
        if eligible, add to eligible List.
        Return a random bark eligible bark from the list.
        */
        
        List<BarkEntry> eligibleList = new List<BarkEntry>();
        
        foreach (var entry in barkList) {
            if (Eligibility(entry)) {
                eligibleList.Add(entry);
            }
        }

        return eligibleList[Random.Range(0, eligibleList.Count)];
    }

    public void PushBark() {
        var bark = GetBark();
        ChatOverlay.Instance.Push(bark.user, bark.text); 
    }
    


}
