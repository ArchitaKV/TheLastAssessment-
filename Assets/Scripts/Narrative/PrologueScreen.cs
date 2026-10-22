using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Prologue: each section fades in and its text types out letter by letter, then fade to black into
// the assessment chamber. First press reveals everything at once; the next press continues.
public class PrologueScreen : MonoBehaviour
{
    [Serializable]
    public class Stage
    {
        public CanvasGroup group;
        public TMP_Text[] texts;
        public float charsPerSecond = 40f;
    }

    [SerializeField] Stage[] stages;
    [SerializeField] CanvasGroup continuePrompt;
    [SerializeField] Graphic continueIndicator;
    [SerializeField] CanvasGroup exitFade;
    [SerializeField] string nextScene = "AssessmentChamber";
    [SerializeField] float groupFadeSeconds = 0.6f;
    [SerializeField] float stageGapSeconds = 0.5f;
    [SerializeField] float sentencePause = 0.25f;
    [SerializeField] float commaPause = 0.08f;
    [SerializeField] float exitFadeSeconds = 1.5f;

    bool revealed, leaving;

    IEnumerator Start()
    {
        foreach (var stage in stages)
        {
            stage.group.alpha = 0f;
            foreach (var text in stage.texts) text.maxVisibleCharacters = 0;
        }
        continuePrompt.alpha = 0f;
        exitFade.alpha = 0f;

        foreach (var stage in stages)
        {
            yield return Fade(stage.group, 1f, groupFadeSeconds);
            foreach (var text in stage.texts)
                yield return Type(text, stage.charsPerSecond);
            yield return Wait(stageGapSeconds);
            if (revealed) yield break;
        }
        yield return Fade(continuePrompt, 1f, groupFadeSeconds);
        revealed = true;
    }

    void Update()
    {
        if (leaving) return;
        if (revealed && continueIndicator)
        {
            var c = continueIndicator.color;
            c.a = 0.55f + 0.45f * Mathf.Sin(Time.time * 2f);
            continueIndicator.color = c;
        }
        if (!AdvanceInput.Pressed()) return;

        if (!revealed) RevealAll();
        else StartCoroutine(Leave());
    }

    void RevealAll()
    {
        revealed = true;
        foreach (var stage in stages)
        {
            stage.group.alpha = 1f;
            foreach (var text in stage.texts) text.maxVisibleCharacters = int.MaxValue;
        }
        continuePrompt.alpha = 1f;
    }

    // Letter-by-letter reveal with short beats after punctuation. Time-based, so speed is
    // independent of frame rate (several letters can appear in one frame).
    IEnumerator Type(TMP_Text text, float charsPerSecond)
    {
        text.ForceMeshUpdate();
        var info = text.textInfo;
        int shown = 0;
        float budget = 0f;
        while (shown < info.characterCount)
        {
            if (revealed) yield break;
            budget += Time.deltaTime;
            while (shown < info.characterCount && budget >= 0f)
            {
                char c = info.characterInfo[shown].character;
                shown++;
                budget -= 1f / charsPerSecond;
                if (c == '.' || c == '—' || c == ':') budget -= sentencePause;
                else if (c == ',') budget -= commaPause;
            }
            text.maxVisibleCharacters = shown;
            yield return null;
        }
    }

    IEnumerator Wait(float seconds)
    {
        for (float t = 0f; t < seconds; t += Time.deltaTime)
        {
            if (revealed) yield break;
            yield return null;
        }
    }

    IEnumerator Leave()
    {
        leaving = true;
        exitFade.blocksRaycasts = true;
        yield return Fade(exitFade, 1f, exitFadeSeconds, skippable: false);
        SceneManager.LoadScene(nextScene);
    }

    IEnumerator Fade(CanvasGroup group, float to, float seconds, bool skippable = true)
    {
        float from = group.alpha;
        for (float t = 0f; t < seconds; t += Time.deltaTime)
        {
            if (skippable && revealed) yield break;
            group.alpha = Mathf.Lerp(from, to, t / seconds);
            yield return null;
        }
        group.alpha = to;
    }
}
