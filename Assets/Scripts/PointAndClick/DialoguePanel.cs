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

    Vector2 home;

    void Awake()
    {
        home = ((RectTransform)transform).anchoredPosition;
        Hide();
    }

    public void Show(ScriptLine line)
    {
        speaker.text = string.IsNullOrEmpty(speakerSuffix) ? line.speaker : $"{line.speaker} / {speakerSuffix}";
        body.text = line.text;
        // Slides in when it appears; later lines replace the text in place.
        if (group.alpha < 0.01f) StartCoroutine(Motion2D.Appear(group, (RectTransform)transform, home, 0.22f));
        if (!shown) { shown = true; Ambience.TextShown(true); }
    }

    bool shown;

    public void Hide()
    {
        if (shown) { shown = false; Ambience.TextShown(false); }
        StopAllCoroutines();
        group.alpha = 0f;
        ((RectTransform)transform).anchoredPosition = home;
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
