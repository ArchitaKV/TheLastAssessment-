using System;
using System.Collections.Generic;
using UnityEngine;

// Narrative data for the assessment, loaded from Assets/Narrative/Data/AssessmentScript.json.
// All story text lives in that file; this model only describes its shape.
[Serializable]
public class ScriptLine
{
    public string speaker; // ASSESSOR, PROTAGONIST, VOICE, ...; empty for captions
    public string text;
    public string caption; // on-screen system text, e.g. "PRELIMINARY VALUATION: 143 CREDITS"

    public bool IsCaption => !string.IsNullOrEmpty(caption);
}

[Serializable]
public class ScriptChoice
{
    public string id; // A–D
    public string text;
}

[Serializable]
public class ScriptNode
{
    public string id;
    public int number;
    public string act;
    public string title;
    public ScriptLine[] pre;
    public ScriptLine question;
    public ScriptChoice[] choices;
    public ScriptLine[] post;
    public string next;

    public bool HasQuestion => question != null && !string.IsNullOrEmpty(question.text) && choices != null && choices.Length > 0;
}

[Serializable]
public class AssessmentData
{
    public string source;
    public string startNode;
    public ScriptNode[] nodes;

    Dictionary<string, ScriptNode> byId;

    public static AssessmentData Parse(string json)
    {
        var data = JsonUtility.FromJson<AssessmentData>(json);
        data.byId = new Dictionary<string, ScriptNode>();
        foreach (var node in data.nodes)
            data.byId[node.id] = node;
        return data;
    }

    public bool TryGetNode(string id, out ScriptNode node)
    {
        node = null;
        return !string.IsNullOrEmpty(id) && byId.TryGetValue(id, out node);
    }

    // Structural checks only; returns human-readable problems (empty list = valid).
    public List<string> Validate(int maxChoices)
    {
        var problems = new List<string>();
        var seen = new HashSet<string>();
        if (!byId.ContainsKey(startNode ?? "")) problems.Add($"startNode '{startNode}' does not exist");
        foreach (var node in nodes)
        {
            if (string.IsNullOrEmpty(node.id)) { problems.Add("node without id"); continue; }
            if (!seen.Add(node.id)) problems.Add($"duplicate node id '{node.id}'");
            if (node.choices != null && node.choices.Length > maxChoices)
                problems.Add($"'{node.id}' has {node.choices.Length} choices; UI supports {maxChoices}");
            foreach (var line in Lines(node))
                if (!line.IsCaption && (string.IsNullOrEmpty(line.speaker) || string.IsNullOrEmpty(line.text)))
                    problems.Add($"'{node.id}' has a line without speaker or text");
        }
        return problems;
    }

    static IEnumerable<ScriptLine> Lines(ScriptNode node)
    {
        if (node.pre != null) foreach (var l in node.pre) yield return l;
        if (node.question != null && !string.IsNullOrEmpty(node.question.text)) yield return node.question;
        if (node.post != null) foreach (var l in node.post) yield return l;
    }
}
