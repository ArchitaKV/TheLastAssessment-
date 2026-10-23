using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Post-ending sequence from Untitled.fig: 01 Reveal → 02 Epilogue (the .fig's Assessment screen is not used), with ← / NEXT →
// navigation and NEW RUN on the Epilogue. Content comes from EndingScreens2D.json (Figma labels) and the
// ending's own Script description; nothing here adds story text.
public class EndingScreens : MonoBehaviour
{
    [Serializable]
    public class Data
    {
        public string id, number, title, background, revealStyle, revealOverline, revealTitle, revealSubline, revealFoot;
        public string tabletLabel, tabletValue, status;
        public float revealVeil;
        public string[] records;
    }

    [Serializable] class DataFile { public Data[] endings; }

    public CanvasGroup root;
    public CanvasGroup[] screens = new CanvasGroup[2];
    public Image[] backgrounds = new Image[2];
    public Image revealVeil;

    [Header("01 Reveal")]
    public GameObject centerBlock, tabletBlock;
    public TMP_Text revealOverline, revealTitle, revealSubline, revealFoot, tabletOverline, tabletTitle, tabletLabel, tabletValue;

    [Header("02 Epilogue")]
    public TMP_Text endingId;
    public TMP_Text epilogueHeading, epilogueText, epilogueStatus;
    public RectTransform epilogueColumn;
    public GameObject recordsPanel;
    public TMP_Text recordsText;

    [Header("Navigation")]
    public Button back, next;
    public TMP_Text nextLabel, sequence;
    public Color nextColor = Color.white, newRunColor = Color.cyan;

    Data[] all;
    const int Last = 1; // 0 Reveal, 1 Epilogue

    public Data Find(TextAsset json, string id)
    {
        all ??= JsonUtility.FromJson<DataFile>(json.text).endings;
        return all.FirstOrDefault(d => d.id == id);
    }

    // Plays the two screens; resolves true when the player chooses NEW RUN.
    public IEnumerator Play(Data d, Sprite background, string summary, bool autoplay, Action<bool> onDone)
    {
        foreach (var bg in backgrounds) bg.sprite = background;
        revealVeil.color = new Color(0.008f, 0.016f, 0.02f, d.revealVeil);

        bool tablet = d.revealStyle == "tablet";
        centerBlock.SetActive(!tablet);
        tabletBlock.SetActive(tablet);
        revealOverline.text = d.revealOverline;
        revealTitle.text = d.revealTitle;
        revealSubline.text = d.revealSubline;
        revealFoot.text = d.revealFoot;
        tabletOverline.text = d.revealOverline;
        tabletTitle.text = d.revealTitle;
        tabletLabel.text = d.tabletLabel;
        tabletValue.text = d.tabletValue;

        endingId.text = $"ENDING {d.number}  /  {d.title}";
        epilogueHeading.text = d.title;
        epilogueText.text = summary;
        epilogueStatus.text = d.status;
        bool records = d.records != null && d.records.Length > 0;
        recordsPanel.SetActive(records);
        recordsText.text = records ? string.Join("\n\n", d.records) : string.Empty;
        // Figma moves the epilogue column left when the records panel is shown (Ending 5).
        epilogueColumn.anchoredPosition = new Vector2(records ? -310f : 0f, epilogueColumn.anchoredPosition.y);
        // Figma: two-line titles at 72 px; keep a full line of height per line so the subline never overlaps.
        bool twoLines = d.revealTitle.Contains("\n");
        revealTitle.fontSize = twoLines ? 72f : 96f;
        revealTitle.lineSpacing = twoLines ? -18f : 0f;
        var titleSize = revealTitle.GetComponent<LayoutElement>() ?? revealTitle.gameObject.AddComponent<LayoutElement>();
        titleSize.minHeight = revealTitle.fontSize * (twoLines ? 2.1f : 1.25f);
        centerBlock.GetComponent<VerticalLayoutGroup>().spacing = 28f;

        int index = 0, shown = -1, step = 0;
        bool done = false, newRun = false;
        back.onClick.RemoveAllListeners();
        next.onClick.RemoveAllListeners();
        back.onClick.AddListener(() => step = -1);
        next.onClick.AddListener(() => step = 1);

        foreach (var s in screens) { s.alpha = 0f; s.blocksRaycasts = false; }
        root.alpha = 1f;
        root.blocksRaycasts = true;
        while (!done)
        {
            if (shown != index)
            {
                yield return Show(index, shown);
                shown = index;
                Trace(d, index);
                if (ShotHook != null) yield return ShotHook($"end_{d.id}_{index + 1}");
            }
            // Pause/evidence menu open, or closed this frame by the key that chose Resume: it owns input.
            if (Time.timeScale == 0f || Time.frameCount == GameMenus.ClosedFrame) { step = 0; yield return null; continue; }
            // Keys are read here, not via UI Submit: a mouse click on empty space clears the selection and would kill Enter.
            if (EventSystem.current.currentSelectedGameObject != null) EventSystem.current.SetSelectedGameObject(null);
            if (autoplay) { step = 1; yield return new WaitForSecondsRealtime(0.3f); }
            var kb = Keyboard.current;
            if (kb != null && (kb.rightArrowKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame
                || kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame)) step = 1;
            if (kb != null && (kb.leftArrowKey.wasPressedThisFrame || kb.backspaceKey.wasPressedThisFrame)) step = -1;
            if (step == 1 && index == Last) { done = true; newRun = !autoplay || NewRunInAutoplay; }
            else if (step != 0) index = Mathf.Clamp(index + step, 0, Last);
            step = 0;
            yield return null;
        }
        onDone(newRun);
    }

    public static bool NewRunInAutoplay;
    public static Func<string, IEnumerator> ShotHook; // QA screenshots (-shots); null for players

    // Cross-dissolve with a slow push on the incoming background.
    IEnumerator Show(int index, int previous)
    {
        back.interactable = index > 0;
        nextLabel.text = index == Last ? "NEW RUN" : "NEXT →";
        nextLabel.color = index == Last ? newRunColor : nextColor;
        sequence.text = $"0{index + 1} / 0{Last + 1}";
        var incoming = screens[index];
        var outgoing = previous >= 0 ? screens[previous] : null;
        var bg = backgrounds[index].rectTransform;
        incoming.blocksRaycasts = true;
        if (outgoing) outgoing.blocksRaycasts = false;
        const float seconds = 0.9f;
        for (float t = 0f; t < seconds; t += Time.unscaledDeltaTime)
        {
            float k = Mathf.SmoothStep(0f, 1f, t / seconds);
            incoming.alpha = k;
            if (outgoing) outgoing.alpha = 1f - k;
            bg.localScale = Vector3.one * Mathf.Lerp(1.04f, 1f, k);
            yield return null;
        }
        incoming.alpha = 1f;
        if (outgoing) outgoing.alpha = 0f;
        bg.localScale = Vector3.one;
    }

    void Trace(Data d, int index)
    {
        string text = index switch
        {
            0 => d.revealStyle == "tablet"
                ? $"{tabletOverline.text} | {tabletTitle.text} | {tabletLabel.text} | {tabletValue.text}"
                : $"{revealOverline.text} | {revealTitle.text.Replace("\n", " ")} | {revealSubline.text} | {revealFoot.text}",
            _ => $"{endingId.text} | {epilogueHeading.text} | {epilogueText.text} | {epilogueStatus.text} | records={recordsPanel.activeSelf}",
        };
        StoryDirector2D.Trace($"ENDSCREEN {d.id} {index + 1}/{Last + 1} | {text}");
        StoryDirector2D.Trace($"ENDSCREEN_BG {d.id} {index + 1}/{Last + 1} {(backgrounds[index].sprite ? backgrounds[index].sprite.name : "none")}");
    }
}
