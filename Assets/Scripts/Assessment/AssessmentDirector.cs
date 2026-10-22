using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Plays the assessment in the chamber: dialogue lines one press at a time, captions for on-screen
// system text, A–D choices for questions. Records answers and autosaves at each node.
// Story text comes only from the narrative data asset.
public class AssessmentDirector : MonoBehaviour
{
    [SerializeField] TextAsset scriptAsset;
    [SerializeField] bool resumeFromSave = true;

    [Header("Opening fade")]
    [SerializeField] CanvasGroup fade;
    [SerializeField] float fadeSeconds = 3f;

    [Header("Dialogue panel")]
    [SerializeField] TMP_Text speakerLabel;
    [SerializeField] TMP_Text body;
    [SerializeField] Graphic continueIndicator;

    [Header("Caption")]
    [SerializeField] CanvasGroup captionGroup;
    [SerializeField] TMP_Text caption;

    [Header("Choices")]
    [SerializeField] GameObject choicesPanel;
    [SerializeField] ChoiceButton[] buttons;
    [SerializeField] float minChoiceHeight = 54f;
    [SerializeField] float choicePadding = 26f;

    public AssessmentState State { get; private set; }
    public string CurrentNodeId => State?.currentNode;
    public bool Finished { get; private set; }

    AssessmentData data;
    string chosen;

    IEnumerator Start()
    {
        data = AssessmentData.Parse(scriptAsset.text);
        foreach (var problem in data.Validate(buttons.Length))
            Debug.LogError($"AssessmentScript: {problem}");

        State = (resumeFromSave ? AssessmentState.Load() : null) ?? new AssessmentState();
        if (string.IsNullOrEmpty(State.currentNode)) State.currentNode = data.startNode;

        body.text = string.Empty;
        speakerLabel.text = string.Empty;
        ShowChoices(null);
        SetCaption(null);
        continueIndicator.enabled = false;

        for (float t = 0f; t < fadeSeconds; t += Time.deltaTime)
        {
            fade.alpha = 1f - t / fadeSeconds;
            yield return null;
        }
        fade.alpha = 0f;
        fade.blocksRaycasts = false;

        yield return Run(State.currentNode);
    }

    IEnumerator Run(string nodeId)
    {
        while (data.TryGetNode(nodeId, out var node))
        {
            State.currentNode = node.id;
            State.Save();
            SetCaption(null);

            yield return PlayLines(node.pre);
            if (node.HasQuestion)
            {
                Show(node.question);
                continueIndicator.enabled = false;
                yield return AwaitChoice(node);
                State.RecordAnswer(node.id, chosen);
                yield return PlayLines(node.post);
            }
            nodeId = node.next;
        }

        // End of authored content: keep the last line on screen, nothing invented.
        State.currentNode = nodeId;
        State.Save();
        continueIndicator.enabled = false;
        Finished = true;
        if (!string.IsNullOrEmpty(nodeId))
            Debug.Log($"AssessmentDirector: node '{nodeId}' is not authored yet; stopping here.");
    }

    IEnumerator PlayLines(ScriptLine[] lines)
    {
        if (lines == null) yield break;
        foreach (var line in lines)
        {
            if (line.IsCaption)
            {
                SetCaption(line.caption); // stays up for the rest of this node
                continue;
            }
            Show(line);
            continueIndicator.enabled = true;
            yield return WaitForAdvance();
        }
    }

    void Show(ScriptLine line)
    {
        speakerLabel.text = line.speaker;
        body.text = line.text;
    }

    // Waits for a fresh press; the frame that ended the previous step is never reused.
    static IEnumerator WaitForAdvance()
    {
        yield return null;
        while (!AdvanceInput.Pressed()) yield return null;
    }

    IEnumerator AwaitChoice(ScriptNode node)
    {
        chosen = null;
        ShowChoices(node);
        while (chosen == null) yield return null;
        ShowChoices(null);
    }

    void ShowChoices(ScriptNode node)
    {
        var choices = node?.choices;
        choicesPanel.SetActive(choices != null);
        for (int i = 0; i < buttons.Length; i++)
        {
            var button = buttons[i];
            bool used = choices != null && i < choices.Length;
            button.gameObject.SetActive(used);
            if (!used) continue;

            var choice = choices[i];
            button.label.text = choice.text;
            // Long answers wrap; grow the button to fit rather than clipping.
            float textHeight = button.label.GetPreferredValues(choice.text, button.label.rectTransform.rect.width, 0f).y;
            button.GetComponent<LayoutElement>().preferredHeight = Mathf.Max(minChoiceHeight, textHeight + choicePadding);
            var uiButton = button.GetComponent<Button>();
            uiButton.onClick.RemoveAllListeners();
            uiButton.onClick.AddListener(() => chosen = choice.id);
        }
        if (choices != null && choices.Length > 0)
            EventSystem.current.SetSelectedGameObject(buttons[0].gameObject);
    }

    void SetCaption(string text)
    {
        caption.text = text ?? string.Empty;
        captionGroup.alpha = string.IsNullOrEmpty(text) ? 0f : 1f;
    }
}
