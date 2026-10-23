using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Speaker label plus one Script line. The short cyan rule pulses while the panel waits for a press.
public class DialoguePanel : MonoBehaviour
{
    public CanvasGroup group;
    public TMP_Text speaker;
    public TMP_Text body;
    public Graphic continueIndicator;
    public string speakerSuffix; // Figma frame 3 labels the speaker "ASSESSOR / DIALOGUE"

    public bool Waiting { get; set; }

    void Awake() => Hide();

    public void Show(ScriptLine line)
    {
        speaker.text = string.IsNullOrEmpty(speakerSuffix) ? line.speaker : $"{line.speaker} / {speakerSuffix}";
        body.text = line.text;
        group.alpha = 1f;
    }

    public void Hide()
    {
        group.alpha = 0f;
        Waiting = false;
    }

    void Update()
    {
        continueIndicator.enabled = Waiting;
        if (!Waiting) return;
        var c = continueIndicator.color;
        c.a = 0.45f + 0.55f * Mathf.Abs(Mathf.Sin(Time.time * 2.2f));
        continueIndicator.color = c;
    }
}
