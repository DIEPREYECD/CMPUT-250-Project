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
        int expectedCols = 27; // number of expected columns without timestamp
        bool skipFirstCol = headerCols[0] == "Timestamp";

        // header assumed exactly as defined above
        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line)) continue;
            var cols = line.Split(',').ToList();
            if (skipFirstCol) cols = cols.Skip(1).ToList();
            if (cols.Count < expectedCols) continue; // not enough columns

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

            // Choice B
            var cb = new EventChoice { key = "B" };
            cb.text = cols[k++].Trim();
            cb.spritePath = cols[k++].Trim();
            cb.deltaFame = int.TryParse(cols[k++], out var bF) ? bF : 0;
            cb.deltaStress = int.TryParse(cols[k++], out var bS) ? bS : 0;
            cb.setFlags = ParseList(cols[k++]);
            cb.clearFlags = ParseList(cols[k++]);
            cb.nextEventId = cols[k++].Trim();

            e.choices = new List<EventChoice> { ca, cb };
            if (!string.IsNullOrWhiteSpace(e.id)) db[e.id] = e;
        }
        return db;
    }
}
