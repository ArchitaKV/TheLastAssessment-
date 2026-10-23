using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Plays the assessment in the chamber: dialogue lines one press at a time, captions and scripted
// visual events via AssessmentPresentation, A–D choices for questions. Records answers and flags,
// and autosaves at each node. Story text comes only from the narrative data asset.
public class AssessmentDirector : MonoBehaviour
{
    [SerializeField] TextAsset scriptAsset;
    [SerializeField] AssessmentPresentation presentation;
    [SerializeField] bool resumeFromSave = true;
    [SerializeField] string titleScene = "Prologue";

    [Header("Fades")]
    [SerializeField] CanvasGroup fade;
    [SerializeField] float fadeSeconds = 3f;
    [SerializeField] float exitFadeSeconds = 1.5f;

    [Header("Dialogue panel")]
    [SerializeField] TMP_Text speakerLabel;
    [SerializeField] TMP_Text body;
    [SerializeField] Graphic continueIndicator;

    [Header("Choices")]
    [SerializeField] GameObject choicesPanel;
    [SerializeField] ChoiceButton[] buttons;
    [SerializeField] float minChoiceHeight = 54f;
    [SerializeField] float choicePadding = 26f;

    public AssessmentState State { get; private set; }
    public string CurrentNodeId => State?.currentNode;
    public bool Finished { get; private set; }

    AssessmentData data;
    ScriptChoice chosen;

    IEnumerator Start()
    {
        data = AssessmentData.Parse(scriptAsset.text);
        foreach (var problem in data.Validate(buttons.Length, presentation.IsKnown))
            Debug.LogError($"AssessmentScript: {problem}");

        // Only a save that points at an authored question is resumed; anything else starts fresh.
        State = (resumeFromSave ? AssessmentState.LoadResumable(data) : null) ?? new AssessmentState { currentNode = data.startNode };

        body.text = string.Empty;
        speakerLabel.text = string.Empty;
        ShowChoices(null);
        presentation.ResetForNode();
        presentation.Restore(State);
        continueIndicator.enabled = false;

        yield return Fade(1f, 0f, fadeSeconds);
        fade.blocksRaycasts = false;

        yield return Run(State.currentNode);
    }

    IEnumerator Run(string nodeId)
    {
        while (data.TryGetNode(nodeId, out var node))
        {
            State.currentNode = node.id;
            State.Save();
            presentation.ResetForNode();

            yield return PlayLines(node.pre);
            if (node.HasQuestion)
            {
                Show(node.question);
                continueIndicator.enabled = false;
                yield return AwaitChoice(node);
                State.RecordAnswer(node.id, chosen.id);
                if (chosen.setFlags != null)
                    foreach (var flag in chosen.setFlags) State.SetFlag(flag);
                State.Save();
                yield return PlayLines(node.post);
            }
            nodeId = node.next;
        }

        // End of authored content: save the position, then return to the title screen (no invented text).
        State.currentNode = nodeId;
        State.Save();
        continueIndicator.enabled = false;
        Finished = true;
        if (!string.IsNullOrEmpty(nodeId))
            Debug.Log($"AssessmentDirector: node '{nodeId}' is not authored yet; returning to the title screen.");
        fade.blocksRaycasts = true;
        yield return Fade(0f, 1f, exitFadeSeconds);
        SceneManager.LoadScene(titleScene);
    }

    IEnumerator PlayLines(ScriptLine[] lines)
    {
        if (lines == null) yield break;
        foreach (var line in lines)
        {
            if (!string.IsNullOrEmpty(line.setFlag)) State.SetFlag(line.setFlag);
            if (line.IsCaption)
            {
                presentation.SetCaption(line.caption); // stays up for the rest of this node
                continue;
            }
            if (line.IsVisual)
            {
                presentation.Play(line.visual);
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
            uiButton.onClick.AddListener(() => chosen = choice);
        }
        if (choices != null && choices.Length > 0)
            EventSystem.current.SetSelectedGameObject(buttons[0].gameObject);
    }

    IEnumerator Fade(float from, float to, float seconds)
    {
        for (float t = 0f; t < seconds; t += Time.deltaTime)
        {
            fade.alpha = Mathf.Lerp(from, to, t / seconds);
            yield return null;
        }
        fade.alpha = to;
    }
}
