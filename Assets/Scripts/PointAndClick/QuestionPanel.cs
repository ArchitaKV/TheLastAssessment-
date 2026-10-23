using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Question display plus A–D options, in one of the Figma styles (Q1 tablet box, Q2 image tiles,
// bottom card + option column). Question and option text come only from the Script node.
public class QuestionPanel : MonoBehaviour
{
    public CanvasGroup group;
    public TMP_Text label;          // optional, filled from labelFormat
    public string labelFormat;      // tokens: {ACT} {nn} {TITLE} {SPEAKER}
    public TMP_Text question;
    public ChoiceButton[] options;  // chevron field holds the A–D letter
    public bool hideOnAnswer = true;
    public bool autoHeight = true;  // grow rows to fit wrapped text (not for fixed image tiles)
    public float minOptionHeight = 48f;
    public float optionPadding = 22f;

    Vector2 home;

    void Awake()
    {
        home = ((RectTransform)transform).anchoredPosition;
        SetVisible(false);
    }

    public IEnumerator Ask(ScriptNode node, string autoplayChoice, System.Action<ScriptChoice> onChosen)
    {
        ScriptChoice chosen = null;
        question.text = node.question.text;
        if (label) label.text = FormatLabel(node);
        for (int i = 0; i < options.Length; i++)
        {
            var option = options[i];
            bool used = i < node.choices.Length;
            option.gameObject.SetActive(used);
            if (!used) continue;

            var choice = node.choices[i];
            option.label.text = choice.text;
            option.chevron.text = choice.id;
            if (autoHeight)
            {
                float textHeight = option.label.GetPreferredValues(choice.text, option.label.rectTransform.rect.width, 0f).y;
                option.GetComponent<LayoutElement>().preferredHeight = Mathf.Max(minOptionHeight, textHeight + optionPadding);
            }
            var button = option.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => chosen = choice);
        }
        SetVisible(true);
        StoryDirector2D.Trace($"QUESTION {question.text}");
        for (int i = 0; i < node.choices.Length; i++)
            StoryDirector2D.Trace($"OPTION {options[i].chevron.text} | {options[i].label.text}");
        // Select next frame so the press that opened the question cannot also pick an answer.
        EventSystem.current.SetSelectedGameObject(null);
        yield return null;
        EventSystem.current.SetSelectedGameObject(options[0].gameObject);

        var last = options[0].gameObject;
        while (chosen == null)
        {
            if (autoplayChoice != null) chosen = System.Array.Find(node.choices, c => c.id == autoplayChoice) ?? node.choices[0];
            // Keep keyboard focus on an answer: closing the pause menu (or a click on empty space) clears the
            // selection, so restore the last highlighted answer once no menu owns input (timeScale is 0 while one is open).
            var selected = EventSystem.current.currentSelectedGameObject;
            if (selected && System.Array.Exists(options, o => o.gameObject == selected)) last = selected;
            else if (!selected && Time.timeScale > 0f) EventSystem.current.SetSelectedGameObject(last);
            yield return null;
        }
        group.interactable = group.blocksRaycasts = false;
        EventSystem.current.SetSelectedGameObject(null);
        if (hideOnAnswer) SetVisible(false);
        onChosen(chosen);
    }

    string FormatLabel(ScriptNode node)
    {
        string act = string.IsNullOrEmpty(node.actTitle) ? string.Empty : node.actTitle[(node.actTitle.IndexOf('—') + 1)..].Trim();
        return labelFormat.Replace("{ACT}", act).Replace("{nn}", node.number.ToString("00"))
            .Replace("{TITLE}", node.title.ToUpperInvariant()).Replace("{SPEAKER}", node.question.speaker);
    }

    bool shown;

    public void SetVisible(bool visible)
    {
        if (visible != shown) { shown = visible; Ambience.TextShown(visible); }
        StopAllCoroutines();
        group.alpha = visible ? 1f : 0f;
        ((RectTransform)transform).anchoredPosition = home;
        group.interactable = group.blocksRaycasts = visible;
        // Options are selectable from the first frame; the slide-in is cosmetic.
        if (visible) StartCoroutine(Motion2D.Appear(group, (RectTransform)transform, home));
    }
}
