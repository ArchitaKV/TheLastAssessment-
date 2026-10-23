using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Player progress through the assessment. Plain data: no UI, no story text.
// Answers are recorded raw (question id + A–D); nothing here is ever shown as a score.
[Serializable]
public class AssessmentState
{
    [Serializable]
    public struct Answer
    {
        public string node;
        public string choice;
    }

    public string currentNode;
    public List<Answer> answers = new();
    public List<string> flags = new();

    public void RecordAnswer(string node, string choice)
    {
        answers.RemoveAll(a => a.node == node); // replaying a question replaces its answer
        answers.Add(new Answer { node = node, choice = choice });
    }

    public string AnswerFor(string node) => answers.Find(a => a.node == node).choice;

    public void SetFlag(string flag)
    {
        if (!flags.Contains(flag)) flags.Add(flag);
    }

    public bool HasFlag(string flag) => flags.Contains(flag);

    // ---------- Single-slot save ----------

    public static string SavePath => Path.Combine(Application.persistentDataPath, "assessment_save.json");

    public static bool SaveExists => File.Exists(SavePath);

    public void Save() => File.WriteAllText(SavePath, JsonUtility.ToJson(this, true));

    // A save can only be continued if it points at a question that is authored in the current data.
    public static AssessmentState LoadResumable(AssessmentData data)
    {
        var state = Load();
        return state != null && data.TryGetNode(state.currentNode, out _) ? state : null;
    }

    public static AssessmentState Load()
    {
        try
        {
            return File.Exists(SavePath) ? JsonUtility.FromJson<AssessmentState>(File.ReadAllText(SavePath)) : null;
        }
        catch (Exception e)
        {
            // A corrupt save must not block the game; start fresh but keep the file for inspection.
            Debug.LogWarning($"AssessmentState: could not read save ({e.Message}); starting a new assessment.");
            return null;
        }
    }

    public static void DeleteSave()
    {
        if (File.Exists(SavePath)) File.Delete(SavePath);
    }
}
