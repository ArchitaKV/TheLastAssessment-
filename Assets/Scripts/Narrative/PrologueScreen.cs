using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Title / prologue: each section fades in and its text types out letter by letter, then the
// NEW GAME / CONTINUE menu appears. First press during the reveal shows everything at once.
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
    [SerializeField] CanvasGroup exitFade;
    [SerializeField] string nextScene = "AssessmentChamber";
    [SerializeField] float groupFadeSeconds = 0.6f;
    [SerializeField] float stageGapSeconds = 0.5f;
    [SerializeField] float sentencePause = 0.25f;
    [SerializeField] float commaPause = 0.08f;
    [SerializeField] float exitFadeSeconds = 1.5f;

    [Header("Menu")]
    [SerializeField] TextAsset scriptAsset;
    [SerializeField] CanvasGroup menu;
    [SerializeField] ChoiceButton newGameButton;
    [SerializeField] ChoiceButton continueButton;

    [Header("Start-over confirmation")]
    [SerializeField] CanvasGroup confirm;
    [SerializeField] ChoiceButton confirmButton;
    [SerializeField] ChoiceButton cancelButton;

    bool revealed, leaving, canContinue;

    IEnumerator Start()
    {
        foreach (var stage in stages)
        {
            stage.group.alpha = 0f;
            foreach (var text in stage.texts) text.maxVisibleCharacters = 0;
        }
        exitFade.alpha = 0f;
        SetGroup(menu, false);
        SetGroup(confirm, false);
        WireMenu();

        foreach (var stage in stages)
        {
            yield return Fade(stage.group, 1f, groupFadeSeconds);
            foreach (var text in stage.texts)
                yield return Type(text, stage.charsPerSecond);
            yield return Wait(stageGapSeconds);
            if (revealed) yield break;
        }
        RevealAll();
    }

    void WireMenu()
    {
        // Continue only resumes a save that points at an authored question; a stale save never leads to a blank screen.
        canContinue = AssessmentState.LoadResumable(AssessmentData.Parse(scriptAsset.text)) != null;
        var continueUi = continueButton.GetComponent<Button>();
        continueUi.interactable = canContinue;
        if (!canContinue)
        {
            continueButton.label.alpha = 0.35f;
            continueButton.chevron.alpha = 0.35f;
        }

        newGameButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            if (AssessmentState.SaveExists) OpenConfirm();
            else StartNewAssessment();
        });
        continueUi.onClick.AddListener(() => StartCoroutine(Leave()));
        confirmButton.GetComponent<Button>().onClick.AddListener(StartNewAssessment);
        cancelButton.GetComponent<Button>().onClick.AddListener(CloseConfirm);
    }

    void Update()
    {
        if (leaving) return;
        if (!revealed)
        {
            if (AdvanceInput.Pressed()) RevealAll();
            return;
        }
        if (confirm.interactable && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            CloseConfirm();
    }

    void RevealAll()
    {
        if (revealed) return;
        revealed = true;
        foreach (var stage in stages)
        {
            stage.group.alpha = 1f;
            foreach (var text in stage.texts) text.maxVisibleCharacters = int.MaxValue;
        }
        SetGroup(menu, true);
        // Select next frame so the press that revealed the menu cannot also activate a button.
        StartCoroutine(SelectNextFrame(canContinue ? continueButton : newGameButton));
    }

    void OpenConfirm()
    {
        SetGroup(menu, false, keepVisible: true);
        SetGroup(confirm, true);
        StartCoroutine(SelectNextFrame(cancelButton)); // safe default
    }

    void CloseConfirm()
    {
        SetGroup(confirm, false);
        SetGroup(menu, true);
        StartCoroutine(SelectNextFrame(newGameButton));
    }

    void StartNewAssessment()
    {
        AssessmentState.DeleteSave();
        StartCoroutine(Leave());
    }

    IEnumerator SelectNextFrame(ChoiceButton button)
    {
        EventSystem.current.SetSelectedGameObject(null);
        yield return null;
        EventSystem.current.SetSelectedGameObject(button.gameObject);
    }

    static void SetGroup(CanvasGroup group, bool active, bool keepVisible = false)
    {
        group.alpha = active || keepVisible ? 1f : 0f;
        group.interactable = active;
        group.blocksRaycasts = active;
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
        if (leaving) yield break;
        leaving = true;
        SetGroup(menu, false, keepVisible: true);
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
