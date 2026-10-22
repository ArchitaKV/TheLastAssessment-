using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

// Scene 1 — Consciousness Recovery: fade in from black, opening line, intake questions.
public class Scene01Dialogue : MonoBehaviour
{
    [Serializable]
    public class Exchange
    {
        public string question;
        [TextArea] public string answer;
    }

    [SerializeField] CanvasGroup fade;
    [SerializeField] float fadeSeconds = 3f;
    [SerializeField] AudioSource hum;
    [SerializeField] TMP_Text body;
    [SerializeField] ChoiceButton[] buttons;

    [SerializeField, TextArea]
    string opening = "Consciousness recovery successful.\nWelcome to Eterna Continuity Processing.";

    // "Where am I?" is canon (Story Bible Q1). The other two answers are placeholders
    // derived from the Bible's Q1 summary and still need writer approval.
    [SerializeField]
    List<Exchange> exchanges = new()
    {
        new() { question = "Where am I?", answer = "Your previous life has concluded.\nYour next-life allocation has not yet been finalised." },
        new() { question = "Who are you?", answer = "I am the Assessor responsible for your evaluation." },
        new() { question = "What is this place?", answer = "This is an assessment chamber.\nThis evaluation will determine which of your memories may be restored." },
    };

    IEnumerator Start()
    {
        body.text = string.Empty;
        ShowChoices(false);
        if (hum) hum.Play();

        for (float t = 0f; t < fadeSeconds; t += Time.deltaTime)
        {
            fade.alpha = 1f - t / fadeSeconds;
            yield return null;
        }
        fade.alpha = 0f;
        fade.blocksRaycasts = false;

        body.text = opening;
        ShowChoices(true);
    }

    void ShowChoices(bool visible)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            var button = buttons[i];
            bool used = visible && i < exchanges.Count;
            button.gameObject.SetActive(used);
            if (!used) continue;

            int index = i;
            button.label.text = exchanges[i].question;
            var uiButton = button.GetComponent<UnityEngine.UI.Button>();
            uiButton.onClick.RemoveAllListeners();
            uiButton.onClick.AddListener(() => Choose(index));
        }

        if (visible && exchanges.Count > 0)
            EventSystem.current.SetSelectedGameObject(buttons[0].gameObject);
    }

    void Choose(int index)
    {
        var exchange = exchanges[index];
        exchanges.RemoveAt(index);
        body.text = exchange.answer;
        ShowChoices(true);
    }
}
