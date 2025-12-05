using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class CsvStoryletLoader
{
    // Parse semicolon-separated list into List<string>
    static List<string> ParseList(string s) =>
        string.IsNullOrWhiteSpace(s) ? new List<string>() :
        s.Split(';').Select(x => x.Trim()).Where(x => x.Length > 0).ToList();

    static int? ParseNullableInt(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        if (int.TryParse(s, out var v)) return v;
        return null;
    }

    public static Dictionary<string, EventDef> Load(TextAsset csv)
    {
        var db = new Dictionary<string, EventDef>(StringComparer.OrdinalIgnoreCase);
        var lines = csv.text.Replace("\r", "").Split('\n');
        if (lines.Length <= 1) return db;

        // Check whether to skip first column if the header is 'Timestamp'
        var headerCols = lines[0].Split(',').Select(c => c.Trim()).ToList();
        bool skipFirstCol = headerCols.Count > 0 && headerCols[0] == "Timestamp";

        // Expected headers in order (when Timestamp is omitted)
        var expected = new[] {
            "id",
            "title",
            "spritePath",
            "situation",
            "weight",
            "oncePerRun",
            "cooldownTurns",
            "minFame",
            "maxFame",
            "minStress",
            "maxStress",
            "requiresAllFlags",
            "forbidsAnyFlags",
            "choiceA_text",
            "choiceA_spritePath",
            "choiceA_deltaFame",
            "choiceA_deltaStress",
            "choiceA_setFlags",
            "choiceA_clearFlags",
            "choiceA_nextEventId",
            "choiceA_miniGame",
            "choiceB_text",
            "choiceB_spritePath",
            "choiceB_deltaFame",
            "choiceB_deltaStress",
            "choiceB_setFlags",
            "choiceB_clearFlags",
            "choiceB_nextEventId",
            "choiceB_miniGame"
        };

        // Build the header list that should match 'expected'
        var actual = headerCols;
        if (skipFirstCol) actual = headerCols.Skip(1).ToList();

        // Normalize to lower-case for comparison
        var actualNorm = actual.Select(s => s.Trim().ToLowerInvariant()).ToList();
        var expectedNorm = expected.Select(s => s.Trim().ToLowerInvariant()).ToList();

        if (actualNorm.Count < expectedNorm.Count)
        {
            Debug.LogError($"CSV header has too few columns ({actualNorm.Count}). Expected at least {expectedNorm.Count} headers.");
            return db;
        }

        for (int hi = 0; hi < expectedNorm.Count; hi++)
        {
            if (actualNorm[hi] != expectedNorm[hi])
            {
                Debug.LogError($"CSV header mismatch at column {hi + 1}: expected '{expectedNorm[hi]}', found '{actualNorm[hi]}'. Full headers: {string.Join(",", headerCols)}");
                return db;
            }
        }

        // header assumed exactly as defined above
        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line)) continue;
            var cols = line.Split(',').ToList();
            if (skipFirstCol) cols = cols.Skip(1).ToList();

            int k = 0;
            var e = new EventDef();
            e.id = cols[k++].Trim();
            e.title = cols[k++].Trim();
            e.spritePath = cols[k++].Trim();
            e.situation = cols[k++].Trim();
            e.weight = int.TryParse(cols[k++], out var w) ? Math.Max(1, w) : 1;
            e.oncePerRun = bool.TryParse(cols[k++], out var opr) && opr;
            e.cooldownTurns = int.TryParse(cols[k++], out var cd) ? Math.Max(0, cd) : 0;

            e.conditions.minFame = ParseNullableInt(cols[k++]);
            e.conditions.maxFame = ParseNullableInt(cols[k++]);
            e.conditions.minStress = ParseNullableInt(cols[k++]);
            e.conditions.maxStress = ParseNullableInt(cols[k++]);
            e.conditions.requiresAllFlags = ParseList(cols[k++]);
            e.conditions.forbidsAnyFlags = ParseList(cols[k++]);

            // Choice A
            var ca = new EventChoice { key = "A" };
            ca.text = cols[k++].Trim();
            ca.spritePath = cols[k++].Trim();
            ca.deltaFame = int.TryParse(cols[k++], out var aF) ? aF : 0;
            ca.deltaStress = int.TryParse(cols[k++], out var aS) ? aS : 0;
            ca.setFlags = ParseList(cols[k++]);
            ca.clearFlags = ParseList(cols[k++]);
            ca.nextEventId = cols[k++].Trim();
            ca.miniGame = cols[k++].Trim();

            // Choice B
            var cb = new EventChoice { key = "B" };
            cb.text = cols[k++].Trim();
            cb.spritePath = cols[k++].Trim();
            cb.deltaFame = int.TryParse(cols[k++], out var bF) ? bF : 0;
            cb.deltaStress = int.TryParse(cols[k++], out var bS) ? bS : 0;
            cb.setFlags = ParseList(cols[k++]);
            cb.clearFlags = ParseList(cols[k++]);
            cb.nextEventId = cols[k++].Trim();
            cb.miniGame = cols[k++].Trim();

            e.choices = new List<EventChoice> { ca, cb };
            if (!string.IsNullOrWhiteSpace(e.id)) db[e.id] = e;
        }
        return db;
    }

    public static string PrintOutDB(Dictionary<string, EventDef> db)
    {
        var lines = new List<string>();
        foreach (var kvp in db)
        {
            var e = kvp.Value;
            lines.Add($"ID: {e.id}, Title: {e.title}, Weight: {e.weight}, OncePerRun: {e.oncePerRun}, Cooldown: {e.cooldownTurns}");
            lines.Add($"  Conditions: Fame[{e.conditions.minFame}-{e.conditions.maxFame}], Stress[{e.conditions.minStress}-{e.conditions.maxStress}], Requires[{string.Join(";", e.conditions.requiresAllFlags)}], Forbids[{string.Join(";", e.conditions.forbidsAnyFlags)}]");
            foreach (var choice in e.choices)
            {
                lines.Add($"  Choice {choice.key}: Text: {choice.text}, DeltaFame: {choice.deltaFame}, DeltaStress: {choice.deltaStress}, NextEventId: {choice.nextEventId}, MiniGame: {choice.miniGame}");
                lines.Add($"    SetFlags: {string.Join(";", choice.setFlags)}, ClearFlags: {string.Join(";", choice.clearFlags)}");
            }
        }
        return string.Join("\n", lines);
    }
}
