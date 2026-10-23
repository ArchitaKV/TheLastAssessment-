using System;
using System.Linq;

// Ending selection rules, loaded from Assets/Narrative/Data/EndingRules2D.json. The Script does not
// define eligibility, so the shipped file contains no rules and "approved": false; the game then
// stops at the Script's own "OUTCOME PENDING." until approved rules are entered (no invented logic).
[Serializable]
public class EndingRules
{
    [Serializable]
    public class Condition
    {
        public string answer;   // node id, e.g. "q20"; matches when the recorded answer is in values
        public string[] values;
        public string flag;     // matches when the flag is set
        public string notFlag;  // matches when the flag is not set
        public string evidence; // matches when the evidence id was recorded
    }

    [Serializable]
    public class Rule
    {
        public string ending;     // "ending1" … "ending6"
        public Condition[] all;   // every condition must match
        public Condition[] any;   // at least one must match (ignored when empty)
    }

    public bool approved;
    public string note;
    public Rule[] rules;
    public string fallback; // ending when no rule matches; empty = none

    // First matching rule wins, in file order. Returns null when unapproved or nothing applies.
    public string Evaluate(AssessmentState state)
    {
        if (!approved) return null;
        foreach (var rule in rules ?? Array.Empty<Rule>())
            if ((rule.all ?? Array.Empty<Condition>()).All(c => Matches(c, state)) &&
                (rule.any == null || rule.any.Length == 0 || rule.any.Any(c => Matches(c, state))))
                return rule.ending;
        return string.IsNullOrEmpty(fallback) ? null : fallback;
    }

    static bool Matches(Condition c, AssessmentState state) =>
        (string.IsNullOrEmpty(c.answer) || (c.values ?? Array.Empty<string>()).Contains(state.AnswerFor(c.answer))) &&
        (string.IsNullOrEmpty(c.flag) || state.HasFlag(c.flag)) &&
        (string.IsNullOrEmpty(c.notFlag) || !state.HasFlag(c.notFlag)) &&
        (string.IsNullOrEmpty(c.evidence) || state.evidence.Contains(c.evidence));
}
