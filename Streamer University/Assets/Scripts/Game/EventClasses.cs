using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EventConditions
{
    public int? minFame, maxFame, minStress, maxStress;
    public List<string> requiresAllFlags = new List<string>();
    public List<string> forbidsAnyFlags = new List<string>();
}

[Serializable]
public class EventChoice
{
    public string key;                // "A" or "B"
    public string text;
    public string spritePath = "";    // Resources path (optional)
    public int deltaFame;
    public int deltaStress;
    public List<string> setFlags = new List<string>();
    public List<string> clearFlags = new List<string>();
    public string nextEventId = "";
}

[Serializable]
public class EventDef
{
    public string id;
    public string title;
    public string spritePath = "";    // Resources path (optional)
    public string situation;
    public int weight = 1;
    public bool oncePerRun = false;
    public int cooldownTurns = 0;
    public EventConditions conditions = new EventConditions();
    public List<EventChoice> choices = new List<EventChoice> { new EventChoice { key = "A" }, new EventChoice { key = "B" } };

    [NonSerialized] public int cooldownLeft = 0;
    [NonSerialized] public bool consumedThisRun = false;
}
