using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// One designed scene (a Figma frame): background, dialogue / question panels, hotspots and named
// elements (documents, overlays) that Script cues show and hide. Built from Scenes2D.json.
public class StoryView : MonoBehaviour
{
    public string viewId;
    public CanvasGroup group;
    public Image dim;              // darkens the background for "dim:" cues
    public DialoguePanel dialogue;
    public QuestionPanel questionPanel;
    public Hotspot[] hotspots;

    [Header("Caption display (optional)")]
    public TMP_Text captionLabel;  // e.g. "PRELIMINARY VALUATION"
    public TMP_Text captionValue;  // e.g. "143 CREDITS"

    Dictionary<string, GameObject> elements;
    CaptionTarget[] captionTargets;
    string captionInitial;

    void Awake() => captionInitial = captionValue ? captionValue.text : null;

    // A reused caption display (e.g. the black view) starts each visit without the previous caption.
    public void ResetCaption()
    {
        if (captionValue && string.IsNullOrEmpty(captionInitial)) captionValue.text = string.Empty;
    }

    public GameObject Find(string elementName)
    {
        if (elements == null)
        {
            elements = new Dictionary<string, GameObject>();
            foreach (var t in GetComponentsInChildren<Transform>(true)) elements.TryAdd(t.name, t.gameObject);
        }
        return elements.TryGetValue(elementName, out var go) ? go : null;
    }

    public Hotspot FindHotspot(string hotspotName) => System.Array.Find(hotspots, h => h.name == hotspotName);

    // Returns false when this view has nowhere to show the caption (the global notice bar takes it).
    public bool ShowCaption(string caption)
    {
        captionTargets ??= GetComponentsInChildren<CaptionTarget>(true);
        bool handled = false;
        foreach (var target in captionTargets)
            if (target.caption == caption) { target.Show(caption); handled = true; }
        if (handled || !captionValue) return handled;
        // Script captions read "LABEL: VALUE"; Figma shows the two parts on separate lines.
        int split = caption.IndexOf(':');
        if (captionLabel) captionLabel.text = split < 0 ? string.Empty : caption[..split].Trim();
        captionValue.text = split < 0 ? caption : caption[(split + 1)..].Trim();
        captionValue.gameObject.SetActive(true);
        return true;
    }

    public void SetAlpha(float alpha) => group.alpha = alpha;

    // Idle camera drift: the whole view (art, hotspots, panels together, so nothing misaligns) eases in by
    // 1.5% and back over 30 s. Starts and ends at scale 1, so it hands over cleanly to the transition push.
    const float DriftAmount = 0.015f, DriftPeriod = 30f;
    bool drifting;
    float driftTime;

    public void Drift(bool on)
    {
        drifting = on && !Motion2D.Instant;
        driftTime = 0f;
        if (!on) transform.localScale = Vector3.one;
    }

    void Update()
    {
        if (!drifting) return;
        driftTime += Time.deltaTime;
        transform.localScale = Vector3.one * (1f + DriftAmount * 0.5f * (1f - Mathf.Cos(driftTime * 2f * Mathf.PI / DriftPeriod)));
    }

    public void SetDim(float amount)
    {
        if (dim) dim.color = new Color(0f, 0f, 0f, amount);
    }

    public void ArmHotspots(bool armed)
    {
        foreach (var h in hotspots) h.SetArmed(armed);
    }

    // Visual-only hotspots are live while the view is on screen and no question is open.
    public void SetIdle(bool idle)
    {
        foreach (var h in hotspots) if (h.IsVisual) h.SetArmed(idle);
    }
}
