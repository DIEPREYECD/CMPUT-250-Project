using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ChatBarkSystem;

public class ChatBarkSystem : MonoBehaviour
{

    public TextAsset ChatBarkCSV;

    public static ChatBarkSystem Instance {get; private set; }

    [System.Serializable]
    public class BarkEntry
    {
        public string id;
        public string user;
        public string text;
        public bool def;
        public int stress;
        public int fame;

        public int lastSpawnedTurn = -1;
    }

    [System.Serializable]
    public class BarkEntryList
    {
        public BarkEntry[] entries;
    }

    private List<BarkEntry> barkList = new List<BarkEntry>();
    private int currentTurn = 0;

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
                    stress = int.Parse(splitRow[4].Trim()),
                    fame = int.Parse(splitRow[5].Trim())
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

    
        if (entry.def) return true; // Automatically eligible if default

        bool stressCond = true;
        bool fameCond = true;

        // 0 = ignore, 1 = under half, 2 = over half
        stressCond = entry.stress == 0 ||
                        (entry.stress == 1 && PlayerController.Instance.Stress < 50) ||
                        (entry.stress == 2 && PlayerController.Instance.Stress >= 50);
        
        fameCond = entry.fame == 0 ||
                        (entry.fame == 1 && PlayerController.Instance.Fame < 50) ||
                        (entry.fame == 2 && PlayerController.Instance.Fame >= 50);
        
        return stressCond && fameCond;
    }

    private float GetProbWeight(BarkEntry entry)
    {
        if (entry.lastSpawnedTurn == -1)
        {
            return 1.0f;
        }

        int turnsSinceSpawn = currentTurn - entry.lastSpawnedTurn;

        if (turnsSinceSpawn <= 3)
        {
            return 0f;
        }

        // Increase prob over time
        float normalizedTime = (turnsSinceSpawn - 3f) / barkList.Count;

        // Cap at 1.0 for full weight
        return Mathf.Min(1.0f, normalizedTime);
    }

    public BarkEntry GetBark() {

        /*
        Go through each entry in bark list
        if eligible, add to eligible List.
        Return a random bark eligible bark from the list.
        */
        
        List<BarkEntry> eligibleList = new List<BarkEntry>();
        List<float> weights = new List<float>();
        
        foreach (var entry in barkList) {
            if (Eligibility(entry)) {
                float weight = GetProbWeight(entry);
                if (weight > 0f)
                {
                    eligibleList.Add(entry);
                    weights.Add(weight);
                }
            }
        }

        return WeightedRandomSelect(eligibleList, weights);
    }

    private BarkEntry WeightedRandomSelect(List<BarkEntry> entries, List<float> weights)
    {
        float totalWeight = 0f;
        foreach (float weight in weights)
        {
            totalWeight += weight;
        }

        float randomValue = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        for (int i = 0; i < entries.Count; i++)
        {
            cumulative += weights[i];
            if (randomValue <= cumulative)
            {
                return entries[i];
            }
        }

        return entries[entries.Count - 1];
    }

    public void PushBark() {
        var bark = GetBark();
        bark.lastSpawnedTurn = currentTurn;
        currentTurn++;
        ChatOverlay.Instance.Push(bark.user, bark.text); 
    }
    


}
