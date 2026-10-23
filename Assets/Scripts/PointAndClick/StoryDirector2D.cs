using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Plays the whole Script through the 2D views: title → prologue → Q1–Q20 → ending. Lines come only
// from AssessmentScript2D.json (generated from the Script); "visual" lines are cues (view switches,
// show/hide, effects, flags, evidence) run by RunCue. Records answers, flags and evidence, autosaves,
// and resumes. Endings follow EndingRules2D.json only. Mirrors AssessmentDirector (3D), untouched.
public class StoryDirector2D : MonoBehaviour
{
    public const string SaveFile = "assessment2d_save.json";
    const string Complete = "complete";

    [SerializeField] TextAsset scriptAsset;
    [SerializeField] TextAsset endingRulesAsset;
    [SerializeField] StoryView[] views;
    [SerializeField] GameMenus menus;
    [SerializeField] RectTransform stage;
    [SerializeField] EndingScreens endingScreens;
    [SerializeField] TextAsset endingScreensAsset;
    [SerializeField] Sprite[] endingBackgrounds;

    [Header("Overlays")]
    [SerializeField] CanvasGroup fade;
    [SerializeField] CanvasGroup whiteFade;
    [SerializeField] CanvasGroup notice;
    [SerializeField] TMP_Text noticeText;
    [SerializeField] CanvasGroup actCard;
    [SerializeField] TMP_Text actCardText;
    [SerializeField] float fadeSeconds = 1.5f;
    [SerializeField] float crossFadeSeconds = 0.8f;

    public AssessmentState State { get; private set; }
    public StoryView CurrentView { get; private set; }

    AssessmentData data;
    EndingRules rules;
    float dimAmount;
    string shownAct;
    bool revealPending; // an act card left the screen black; the next view switch fades in

    // QA hooks (command line): -autoplay, -answers q1=A,…, -default B, -resume, -ending endingN, -shots <dir>
    bool autoplay, autoResume;
    readonly Dictionary<string, string> autoAnswers = new();
    string defaultAnswer = "A", forcedEnding, shotsDir;
    int shotIndex;

    IEnumerator Start()
    {
        AssessmentState.SaveFileName = SaveFile;
        ReadArgs();
        data = AssessmentData.Parse(scriptAsset.text);
        foreach (var problem in data.Validate(4, IsKnownCue)) Debug.LogError($"AssessmentScript2D: {problem}");
        rules = JsonUtility.FromJson<EndingRules>(endingRulesAsset.text);
        menus.Init(data.evidence, () => State);

        foreach (var view in views) { view.SetAlpha(0f); view.ArmHotspots(false); }
        SetGroup(notice, 0f); SetGroup(actCard, 0f); SetGroup(whiteFade, 0f);
        fade.alpha = 1f;

        var saved = AssessmentState.LoadResumable(data);
        bool resume;
        if (StartNewOnLoad)
        {
            StartNewOnLoad = false;
            resume = false;
            Trace("START from NEW RUN");
            if (autoplay) { Application.Quit(); yield break; } // QA: NEW RUN reached a fresh start
        }
        else if (autoplay) resume = autoResume && saved != null;
        else
        {
            bool? choice = null;
            StartCoroutine(FadeGroup(fade, 0f, fadeSeconds));
            yield return menus.RunTitle(saved != null, AssessmentState.SaveExists, r => choice = r);
            resume = choice.Value;
            yield return FadeGroup(fade, 1f, 0.6f);
        }
        if (!resume) AssessmentState.DeleteSave();
        State = resume ? saved : new AssessmentState { currentNode = data.startNode };
        Trace($"START node={State.currentNode} resumed={resume} save={AssessmentState.SavePath}");
        menus.InStory = true;
        yield return Run(State.currentNode);
    }

    IEnumerator Run(string nodeId)
    {
        while (data.TryGetNode(nodeId, out var node))
        {
            State.currentNode = node.id;
            State.Save();
            if (!string.IsNullOrEmpty(node.actTitle) && node.actTitle != shownAct) yield return ActCard(node.actTitle);

            yield return PlayLines(node.pre);
            if (node.HasQuestion)
            {
                CurrentView.dialogue.Hide();
                ScriptChoice chosen = null;
                var asking = StartCoroutine(CurrentView.questionPanel.Ask(node, autoplay ? AutoAnswer(node.id) : null, c => chosen = c));
                if (shotsDir != null) yield return Shot("question");
                yield return asking;
                State.RecordAnswer(node.id, chosen.id);
                if (chosen.setFlags != null) foreach (var flag in chosen.setFlags) State.SetFlag(flag);
                State.Save();
                Trace($"ANSWER {node.id}={chosen.id}");
                foreach (var branch in node.branches ?? Array.Empty<ScriptBranch>())
                    if (branch.choices.Contains(chosen.id)) yield return PlayLines(branch.lines);
                yield return PlayLines(node.post);
            }
            nodeId = node.next;
            if (string.IsNullOrEmpty(nodeId) && node.id == "q20") nodeId = ChooseEnding();
        }
        yield return Finish();
    }

    string ChooseEnding()
    {
        string ending = forcedEnding ?? rules.Evaluate(State);
        Trace($"ENDING {ending ?? "pending"} rulesApproved={rules.approved}{(forcedEnding != null ? " (forced by -ending)" : "")}");
        if (ending != null) State.SetFlag($"ending:{ending}");
        return ending ?? string.Empty;
    }

    // After an ending: hold for a press, then the Figma ending screens (Reveal / Epilogue / Assessment).
    // NEW RUN starts a fresh 2D game; otherwise (OUTCOME PENDING) fade back to the title.
    IEnumerator Finish()
    {
        State.currentNode = Complete;
        State.Save();
        Trace($"END save={AssessmentState.SavePath}");
        yield return WaitForAdvance();
        if (CurrentView) CurrentView.dialogue.Hide();
        yield return FadeGroup(fade, 1f, fadeSeconds);

        string ending = State.flags.FirstOrDefault(f => f.StartsWith("ending:"))?.Substring(7);
        var screens = ending != null ? endingScreens.Find(endingScreensAsset, ending) : null;
        bool newRun = false;
        if (screens != null && data.TryGetNode(ending, out var endingNode))
        {
            if (CurrentView) CurrentView.SetAlpha(0f);
            SetGroup(whiteFade, 0f); // Ending 1 ends on "The screen fades to white"; the black fade now covers it
            StartCoroutine(FadeGroup(fade, 0f, 0.8f));
            yield return endingScreens.Play(screens, endingBackground(screens.background), endingNode.summary, autoplay, r => newRun = r);
            yield return FadeGroup(fade, 1f, 0.8f);
        }
        menus.InStory = false;
        if (newRun)
        {
            AssessmentState.DeleteSave(); // the 2D save only (SaveFileName is the 2D file)
            StartNewOnLoad = true;
            Trace("NEW RUN");
        }
        else if (autoplay) { Application.Quit(); yield break; }
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // NEW RUN: the next load skips the title and starts a fresh assessment.
    static bool StartNewOnLoad;

    Sprite endingBackground(string path) => System.Array.Find(endingBackgrounds, s => s && path.EndsWith(s.name));

    IEnumerator PlayLines(ScriptLine[] lines)
    {
        foreach (var line in lines ?? Array.Empty<ScriptLine>())
        {
            if (!string.IsNullOrEmpty(line.setFlag)) State.SetFlag(line.setFlag);
            if (line.IsVisual) { yield return RunCue(line.visual); continue; }
            if (line.IsCaption)
            {
                if (!CurrentView.ShowCaption(line.caption)) ShowNotice(line.caption);
                Trace($"CAPTION {line.caption}");
                State.Save();
                continue;
            }
            CurrentView.dialogue.Show(line);
            Trace($"LINE {CurrentView.dialogue.speaker.text} | {CurrentView.dialogue.body.text}");
            CurrentView.dialogue.Waiting = true;
            yield return WaitForAdvance();
            CurrentView.dialogue.Waiting = false;
        }
    }

    // ---------- Cues ----------

    static readonly HashSet<string> CueVerbs = new() { "view", "await", "show", "hide", "flash", "flicker", "shrink", "wave", "dim", "fade", "cut", "glitch", "wait", "flag", "evidence", "notice" };

    bool IsKnownCue(string cue)
    {
        var p = cue.Split(':');
        return CueVerbs.Contains(p[0]) && (p[0] != "view" || views.Any(v => v.viewId == p[1]));
    }

    IEnumerator RunCue(string cue)
    {
        var p = cue.Split(':');
        string arg = p.Length > 1 ? p[1] : null;
        float Num(int i, float fallback) => p.Length > i ? float.Parse(p[i], CultureInfo.InvariantCulture) : fallback;
        Trace($"CUE {cue}");
        switch (p[0])
        {
            case "view": yield return Enter(views.First(v => v.viewId == arg)); break;
            case "await": yield return Await(arg); break;
            case "show": Element(arg)?.SetActive(true); break;
            case "hide": Element(arg)?.SetActive(false); break;
            case "flash":
                var flashed = Element(arg);
                if (!flashed) break;
                flashed.SetActive(true);
                yield return Wait(Num(2, 0.35f));
                flashed.SetActive(false);
                break;
            case "flicker":
                var flickered = Element(arg);
                if (!flickered) break;
                for (int i = 0; i < 3; i++) { flickered.SetActive(true); yield return Wait(0.07f); flickered.SetActive(false); yield return Wait(0.12f); }
                break;
            case "shrink": yield return Shrink(Element(arg)); break;
            case "wave":
                var wave = CurrentView.GetComponentInChildren<Waveform>(true);
                if (!wave) { Debug.LogError($"StoryDirector2D: view {CurrentView.viewId} has no waveform"); break; }
                if (arg == "play") wave.Play(); else if (arg == "faint") wave.Faint(); else if (arg == "cut") wave.Cut(); else wave.Stop();
                break;
            case "dim": dimAmount = Num(1, 0f); CurrentView.SetDim(dimAmount); break;
            case "fade":
                float seconds = Num(2, 1f);
                if (arg == "clear") { StartCoroutine(FadeGroup(whiteFade, 0f, seconds)); yield return FadeGroup(fade, 0f, seconds); }
                else yield return FadeGroup(arg == "white" ? whiteFade : fade, 1f, seconds);
                break;
            case "cut": fade.alpha = 1f; yield return Wait(0.4f); break;
            case "glitch": yield return Glitch(); break;
            case "wait": yield return Wait(Num(1, 1f)); break;
            case "flag": State.SetFlag(arg); break;
            case "evidence":
                State.AddEvidence(arg);
                State.SetFlag($"evidence:{arg}");
                State.Save();
                Trace($"EVIDENCE {arg}");
                break;
            case "notice": SetGroup(notice, 0f); break;
            default: Debug.LogError($"StoryDirector2D: unknown cue '{cue}'"); break;
        }
    }

    GameObject Element(string elementName)
    {
        var go = CurrentView.Find(elementName);
        if (!go) Debug.LogError($"StoryDirector2D: view {CurrentView.viewId} has no element '{elementName}'");
        return go;
    }

    // Cross-fades to the view (or reveals it from black on first entry / after an act card).
    IEnumerator Enter(StoryView view)
    {
        var previous = CurrentView;
        if (previous == view)
        {
            // Same view after an act card (or a cut to black): reveal it again.
            if (revealPending) { revealPending = false; if (whiteFade.alpha == 0f) yield return FadeGroup(fade, 0f, 0.8f); }
            yield break;
        }
        CurrentView = view;
        SetGroup(notice, 0f);
        view.SetDim(dimAmount);
        view.ResetCaption();
        if (previous)
        {
            previous.ArmHotspots(false);
            previous.dialogue.Hide();
        }
        if (!previous || revealPending || fade.alpha > 0.99f)
        {
            if (previous) previous.SetAlpha(0f);
            view.SetAlpha(1f);
            revealPending = false;
            if (fade.alpha > 0f && whiteFade.alpha == 0f) yield return FadeGroup(fade, 0f, previous ? 0.8f : fadeSeconds);
        }
        else
        {
            for (float t = 0f; t < crossFadeSeconds; t += Time.deltaTime)
            {
                float k = t / crossFadeSeconds;
                previous.SetAlpha(1f - k);
                view.SetAlpha(k);
                yield return null;
            }
            previous.SetAlpha(0f);
            view.SetAlpha(1f);
        }
        Trace($"VIEW {view.viewId}");
        yield return Shot($"view_{view.viewId}");
    }

    // Arms one hotspot (e.g. the tablet) and waits for its click; keyboard: any key focuses it.
    IEnumerator Await(string hotspotName)
    {
        var hotspot = CurrentView.FindHotspot(hotspotName);
        if (!hotspot) { Debug.LogError($"StoryDirector2D: view {CurrentView.viewId} has no hotspot '{hotspotName}'"); yield break; }
        bool clicked = false;
        void OnClick() => clicked = true;
        hotspot.Clicked += OnClick;
        hotspot.SetArmed(true);
        EventSystem.current.SetSelectedGameObject(null);
        while (!clicked)
        {
            if (autoplay) { yield return null; break; }
            if (!menus.Blocking && Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame && !EventSystem.current.currentSelectedGameObject)
                EventSystem.current.SetSelectedGameObject(hotspot.gameObject);
            yield return null;
        }
        hotspot.SetArmed(false);
        hotspot.Clicked -= OnClick;
        Trace($"HOTSPOT {hotspotName}");
        yield return null; // the click that used the hotspot must not also advance the next line
    }

    IEnumerator ActCard(string actTitle)
    {
        shownAct = actTitle;
        Trace($"ACT {actTitle}");
        if (CurrentView) CurrentView.dialogue.Hide();
        yield return FadeGroup(fade, 1f, 0.6f);
        actCardText.text = actTitle;
        yield return FadeGroup(actCard, 1f, 0.5f);
        yield return Shot("act");
        yield return Wait(1.8f);
        yield return FadeGroup(actCard, 0f, 0.5f);
        revealPending = true; // the node's first view cue fades in from black
    }

    IEnumerator Shrink(GameObject go)
    {
        if (!go) yield break;
        // Elements are anchored and pivoted top-left; land the scaled element's centre at the top-right corner.
        var rt = (RectTransform)go.transform;
        Vector2 from = rt.anchoredPosition, to = new(1720f - rt.rect.width * 0.15f, -90f + rt.rect.height * 0.15f);
        for (float t = 0f; t < 0.8f; t += Time.deltaTime)
        {
            float k = Mathf.SmoothStep(0f, 1f, t / 0.8f);
            rt.anchoredPosition = Vector2.Lerp(from, to, k);
            rt.localScale = Vector3.one * Mathf.Lerp(1f, 0.3f, k);
            yield return null;
        }
        rt.anchoredPosition = to;
        rt.localScale = Vector3.one * 0.3f;
    }

    // Brief, unexplained visual disturbance: the stage jitters and blinks.
    IEnumerator Glitch()
    {
        var home = stage.anchoredPosition;
        for (int i = 0; i < 7; i++)
        {
            stage.anchoredPosition = home + new Vector2(UnityEngine.Random.Range(-14f, 14f), UnityEngine.Random.Range(-6f, 6f));
            if (CurrentView) CurrentView.SetAlpha(i % 2 == 0 ? 0.6f : 1f);
            yield return Wait(0.045f);
        }
        stage.anchoredPosition = home;
        if (CurrentView) CurrentView.SetAlpha(1f);
    }

    void ShowNotice(string text)
    {
        noticeText.text = text;
        SetGroup(notice, 1f);
    }

    // ---------- Input and timing ----------

    // Waits for a fresh press; the frame that ended the previous step is never reused.
    IEnumerator WaitForAdvance()
    {
        yield return null;
        if (autoplay) { yield return null; yield break; }
        while (menus.Blocking || !AdvanceInput.Pressed()) yield return null;
    }

    IEnumerator Wait(float seconds)
    {
        if (autoplay) seconds *= 0.25f;
        for (float t = 0f; t < seconds; t += Time.deltaTime) yield return null;
    }

    IEnumerator FadeGroup(CanvasGroup group, float to, float seconds)
    {
        if (autoplay) seconds *= 0.25f;
        float from = group.alpha;
        for (float t = 0f; t < seconds; t += Time.deltaTime)
        {
            group.alpha = Mathf.Lerp(from, to, t / seconds);
            yield return null;
        }
        group.alpha = to;
        group.blocksRaycasts = group == fade && to > 0f;
    }

    static void SetGroup(CanvasGroup g, float alpha) { g.alpha = alpha; g.blocksRaycasts = false; }

    // ---------- QA hooks ----------

    void ReadArgs()
    {
        var args = Environment.GetCommandLineArgs();
        string Arg(string name) { int i = Array.IndexOf(args, name); return i >= 0 && i + 1 < args.Length ? args[i + 1] : null; }
        autoplay = args.Contains("-autoplay");
        autoResume = args.Contains("-resume");
        EndingScreens.NewRunInAutoplay = args.Contains("-newrun");
        if (Arg("-shots") != null) EndingScreens.ShotHook = Shot;
        forcedEnding = Arg("-ending");
        shotsDir = Arg("-shots");
        defaultAnswer = Arg("-default") ?? "A";
        foreach (var pair in (Arg("-answers") ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            var kv = pair.Split('=');
            if (kv.Length == 2) autoAnswers[kv[0]] = kv[1];
        }
        if (!autoplay) return;
        // Unattended QA runs must not pause when the window loses focus (players keep the project setting).
        Application.runInBackground = true;
        Trace($"AUTOPLAY default={defaultAnswer} answers={Arg("-answers")} ending={forcedEnding}");
    }

    string AutoAnswer(string nodeId) => autoAnswers.TryGetValue(nodeId, out var a) ? a : defaultAnswer;

    IEnumerator Shot(string tag)
    {
        if (shotsDir == null) yield break;
        yield return new WaitForEndOfFrame();
        ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(shotsDir, $"{shotIndex++:000}_{State?.currentNode}_{tag}.png"));
    }

    // One Player.log line per displayed element, so on-screen text can be checked against the Script.
    public static void Trace(string message) => Debug.Log($"[Slice2D] {message}");
}
