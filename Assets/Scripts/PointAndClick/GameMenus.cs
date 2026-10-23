using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Title (New / Continue / Quit, with start-over confirmation), pause menu (Escape) and evidence
// journal (J). System UI only: no story text except evidence copied from the Script.
public class GameMenus : MonoBehaviour
{
    public CanvasGroup title, confirm, pause, journal;
    public ChoiceButton newGame, continueGame, quitFromTitle, confirmYes, confirmNo;
    public ChoiceButton resume, openJournal, toTitle, quitGame, closeJournal;
    public TMP_Text journalHeader, journalBody;

    public bool InStory { get; set; }
    public bool Blocking => pause.alpha > 0f || journal.alpha > 0f;

    EvidenceDef[] evidenceDefs;
    Func<AssessmentState> state;

    void Awake()
    {
        foreach (var g in new[] { title, confirm, pause, journal }) Set(g, false);
        Wire(resume, ClosePause);
        Wire(openJournal, () => { Set(pause, false); OpenJournal(); });
        Wire(toTitle, () => { Time.timeScale = 1f; SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); });
        Wire(quitGame, Application.Quit);
        Wire(closeJournal, CloseJournal);
        Wire(quitFromTitle, Application.Quit);
    }

    public void Init(EvidenceDef[] defs, Func<AssessmentState> current)
    {
        evidenceDefs = defs ?? Array.Empty<EvidenceDef>();
        state = current;
    }

    // Resolves true for Continue, false for New Game (after confirming if a save would be lost).
    public IEnumerator RunTitle(bool canContinue, bool saveExists, Action<bool> onResult)
    {
        bool? result = null;
        continueGame.GetComponent<Button>().interactable = canContinue;
        continueGame.label.alpha = continueGame.chevron.alpha = canContinue ? 1f : 0.35f;
        Wire(continueGame, () => result = true);
        Wire(newGame, () =>
        {
            if (!saveExists) { result = false; return; }
            Set(title, false, keepVisible: true);
            Set(confirm, true);
            StartCoroutine(Select(confirmNo));
        });
        Wire(confirmYes, () => result = false);
        Wire(confirmNo, () => { Set(confirm, false); Set(title, true); StartCoroutine(Select(newGame)); });

        Set(title, true);
        yield return Select(canContinue ? continueGame : newGame);
        while (result == null)
        {
            if (confirm.alpha > 0f && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) confirmNo.GetComponent<Button>().onClick.Invoke();
            yield return null;
        }
        Set(confirm, false);
        Set(title, false);
        onResult(result.Value);
    }

    void Update()
    {
        if (!InStory || Keyboard.current == null) return;
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (journal.alpha > 0f) CloseJournal();
            else if (pause.alpha > 0f) ClosePause();
            else OpenPause();
        }
        else if (Keyboard.current.jKey.wasPressedThisFrame)
        {
            if (journal.alpha > 0f) CloseJournal(); else if (pause.alpha == 0f) OpenJournal();
        }
    }

    void OpenPause()
    {
        Time.timeScale = 0f;
        Set(pause, true);
        StartCoroutine(Select(resume));
    }

    void ClosePause()
    {
        Set(pause, false);
        Time.timeScale = 1f;
        EventSystem.current.SetSelectedGameObject(null);
    }

    void OpenJournal()
    {
        Time.timeScale = 0f;
        var current = state?.Invoke();
        var found = current?.evidence ?? new System.Collections.Generic.List<string>();
        var items = evidenceDefs.Where(d => found.Contains(d.id)).ToArray();
        // Kept-but-unheard evidence (Q12 answer C) shows its title only; the transcript appears once heard.
        bool Heard(EvidenceDef d) => string.IsNullOrEmpty(d.heardFlag) || (current != null && current.HasFlag(d.heardFlag));
        journalHeader.text = $"EVIDENCE / {items.Length} {(items.Length == 1 ? "ITEM" : "ITEMS")}";
        journalBody.text = string.Join("\n\n", items.Select(d => $"<b>{d.title} — {d.source}</b>" + (Heard(d) ? $"\n“{d.text}”" : string.Empty)));
        Set(journal, true);
        StartCoroutine(Select(closeJournal));
        StoryDirector2D.Trace($"JOURNAL {items.Length} | {journalBody.text.Replace("\n", " / ")}");
    }

    void CloseJournal()
    {
        Set(journal, false);
        Time.timeScale = 1f;
        EventSystem.current.SetSelectedGameObject(null);
    }

    static void Wire(ChoiceButton b, Action action)
    {
        var button = b.GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => action());
    }

    static IEnumerator Select(ChoiceButton b)
    {
        EventSystem.current.SetSelectedGameObject(null);
        yield return null; // the press that opened the menu must not also activate a button
        EventSystem.current.SetSelectedGameObject(b.gameObject);
    }

    static void Set(CanvasGroup g, bool on, bool keepVisible = false)
    {
        g.alpha = on || keepVisible ? 1f : 0f;
        g.interactable = g.blocksRaycasts = on;
    }
}
