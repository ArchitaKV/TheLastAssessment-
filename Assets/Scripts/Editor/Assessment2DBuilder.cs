using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// Builds the complete 2D game scene from Assets/Narrative/Data/Scenes2D.json (view layouts at Figma
// pixel positions on the 1920×1080 stage) plus the shared UI (title, pause, journal, notice bar, act
// card, fades). Rerunnable; does not touch the 3D scenes or Build Settings.
public static class Assessment2DBuilder
{
    const string ScenePath = "Assets/Scenes/Assessment2D.unity";
    const string ScriptPath = "Assets/Narrative/Data/AssessmentScript2D.json";
    const string RulesPath = "Assets/Narrative/Data/EndingRules2D.json";
    const string LayoutPath = "Assets/Narrative/Data/Scenes2D.json";
    const string EndingScreensPath = "Assets/Narrative/Data/EndingScreens2D.json";
    const string ArtRoot = "Assets/Art/";
    const string PlayerPath = "D:/game dev/Builds/Game2D/TheLastAssessment.exe";

    // ---------- Layout data (Scenes2D.json) ----------

    [Serializable] class Layout { public Style style; public ViewSpec[] views; }
    [Serializable] class Style { public string font; public string cyan, white, grey, border, panel, dark; public float panelAlpha; }
    [Serializable] class ViewSpec
    {
        public string id, bg, dialogue, hud, captionLabel, captionValue;
        public QuestionSpec question;
        public HotspotSpec[] hotspots;
        public ElementSpec[] elements;
    }
    [Serializable] class QuestionSpec { public string style, label; public bool hideOnAnswer = true; public TileSpec[] tiles; }
    [Serializable] class TileSpec { public float[] rect; public string sprite; }
    [Serializable] class HotspotSpec { public string name, action; public float[] rect; }
    [Serializable] class ElementSpec
    {
        public string type, name, parent, text, color, align, sprite, caption;
        public float x, baseline, w, h, size, spacing;
        public float[] rect;
        public bool hidden, applyCaption;
    }

    static Color Cyan, White, Grey, Border, PanelFill, Dark;
    static TMP_FontAsset font;

    [MenuItem("The Last Assessment/Build 2D Game Scene")]
    public static void Build()
    {
        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            Debug.LogError("Assessment2DBuilder: save the open scene first; building opens a new scene.");
            return;
        }
        var layout = JsonUtility.FromJson<Layout>(File.ReadAllText(LayoutPath));
        ApplyStyle(layout.style ?? new Style());
        foreach (var path in new[] { "Backgrounds", "Clues", "Endings" }.SelectMany(d => Directory.GetFiles(ArtRoot + d, "*.png")))
            ImportSprite(path.Replace('\\', '/'));

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var cam = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)) { tag = "MainCamera" }.GetComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black; // letterbox bars
        new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));

        var canvasGo = new GameObject("Game2D_UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        var stage = ChamberBuilder.Rect("Stage_1920x1080", canvasGo.transform, new(0.5f, 0.5f), new(0.5f, 0.5f), Vector2.zero, StageFit.Size);
        stage.gameObject.AddComponent<StageFit>();
        stage.gameObject.AddComponent<RectMask2D>(); // camera push / drift stay inside the frame on letterboxed screens

        var views = layout.views.Select(v => BuildView(v, stage)).ToArray();
        var endingScreens = BuildEndingScreens(stage);

        // Shared overlays, above every view.
        var notice = PanelAt("Notice", stage, 64, 770, 1180, 48, Border);
        var noticeText = Text("Notice_Text", notice, "", 16, Cyan, 24, 32, 1130, spacing: 3);
        var noticeGroup = notice.gameObject.AddComponent<CanvasGroup>();
        var menus = BuildMenus(stage);

        var white = ChamberBuilder.Stretch("Fade_White", canvasGo.transform);
        white.gameObject.AddComponent<Image>().color = Color.white;
        var whiteGroup = white.gameObject.AddComponent<CanvasGroup>();
        var fadeRt = ChamberBuilder.Stretch("Fade", canvasGo.transform);
        fadeRt.gameObject.AddComponent<Image>().color = Color.black;
        var fade = fadeRt.gameObject.AddComponent<CanvasGroup>();
        // Act cards and menus sit above the fades: act titles show on black, the title over a black story.
        var actStage = ChamberBuilder.Rect("ActCard_Stage", canvasGo.transform, new(0.5f, 0.5f), new(0.5f, 0.5f), Vector2.zero, StageFit.Size);
        actStage.gameObject.AddComponent<StageFit>();
        var actGroup = actStage.gameObject.AddComponent<CanvasGroup>();
        actGroup.blocksRaycasts = false;
        var actText = Text("ActCard_Text", actStage, "", 40, White, 160, 560, 1600, spacing: 12, align: TextAlignmentOptions.Top);
        var menuStage = ChamberBuilder.Rect("Menu_Stage", canvasGo.transform, new(0.5f, 0.5f), new(0.5f, 0.5f), Vector2.zero, StageFit.Size);
        menuStage.gameObject.AddComponent<StageFit>();
        menus.transform.SetParent(menuStage, false);

        var director = canvasGo.AddComponent<StoryDirector2D>();
        var so = new SerializedObject(director);
        so.FindProperty("scriptAsset").objectReferenceValue = AssetDatabase.LoadAssetAtPath<TextAsset>(ScriptPath);
        so.FindProperty("endingRulesAsset").objectReferenceValue = AssetDatabase.LoadAssetAtPath<TextAsset>(RulesPath);
        so.FindProperty("menus").objectReferenceValue = menus;
        so.FindProperty("stage").objectReferenceValue = stage;
        so.FindProperty("fade").objectReferenceValue = fade;
        so.FindProperty("whiteFade").objectReferenceValue = whiteGroup;
        so.FindProperty("notice").objectReferenceValue = noticeGroup;
        so.FindProperty("noticeText").objectReferenceValue = noticeText;
        so.FindProperty("actCard").objectReferenceValue = actGroup;
        so.FindProperty("actCardText").objectReferenceValue = actText;
        so.FindProperty("endingScreens").objectReferenceValue = endingScreens;
        so.FindProperty("endingScreensAsset").objectReferenceValue = AssetDatabase.LoadAssetAtPath<TextAsset>(EndingScreensPath);
        so.FindProperty("ambience").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(AmbiencePath);
        var endingSprites = Directory.GetFiles(ArtRoot + "Endings", "*.png").Select(p => AssetDatabase.LoadAssetAtPath<Sprite>(p.Replace('\\', '/'))).ToArray();
        var spritesProp = so.FindProperty("endingBackgrounds");
        spritesProp.arraySize = endingSprites.Length;
        for (int i = 0; i < endingSprites.Length; i++) spritesProp.GetArrayElementAtIndex(i).objectReferenceValue = endingSprites[i];
        var viewsProp = so.FindProperty("views");
        viewsProp.arraySize = views.Length;
        for (int i = 0; i < views.Length; i++) viewsProp.GetArrayElementAtIndex(i).objectReferenceValue = views[i];
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.SaveScene(scene, ScenePath);
        Debug.Log($"Assessment2DBuilder: built {ScenePath} with {views.Length} views.");
    }

    // Batch entry point: a Windows player containing only the 2D scene (Build Settings untouched).
    public static void BuildPlayer() => BuildPlayer(BuildOptions.None);

    // Same player, rebuilt without the incremental build cache (release / validation builds).
    public static void CleanBuildPlayer() => BuildPlayer(BuildOptions.CleanBuildCache);

    static void BuildPlayer(BuildOptions options)
    {
        if (!File.Exists(ScenePath)) { Debug.LogError($"Assessment2DBuilder: {ScenePath} not built yet."); EditorApplication.Exit(1); return; }
        var report = BuildPipeline.BuildPlayer(new[] { ScenePath }, PlayerPath, BuildTarget.StandaloneWindows64, options);
        Debug.Log($"Assessment2DBuilder: player build {report.summary.result}, {report.summary.totalErrors} errors, {report.summary.totalSize / (1024 * 1024)} MB at {PlayerPath}");
        if (Application.isBatchMode) EditorApplication.Exit(report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded ? 0 : 1);
    }

    public static void BuildAll()
    {
        Build();
        BuildPlayer();
    }

    // Additive: opens the saved scene, adds only the EndingScreens object and wires the director's three
    // ending fields. Every other object stays exactly as saved. Refuses to run twice.
    [MenuItem("The Last Assessment/Add Ending Screens To Saved Scene")]
    public static void AddEndingScreensToSavedScene()
    {
        var layout = JsonUtility.FromJson<Layout>(File.ReadAllText(LayoutPath));
        ApplyStyle(layout.style ?? new Style());
        foreach (var path in Directory.GetFiles(ArtRoot + "Endings", "*.png")) ImportSprite(path.Replace('\\', '/'));

        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var director = UnityEngine.Object.FindFirstObjectByType<StoryDirector2D>(FindObjectsInactive.Include);
        var so = new SerializedObject(director);
        var stage = (RectTransform)so.FindProperty("stage").objectReferenceValue;
        if (stage.Find("EndingScreens"))
        {
            Debug.LogError("Assessment2DBuilder: EndingScreens already exists in the saved scene; nothing changed.");
            if (Application.isBatchMode) EditorApplication.Exit(1);
            return;
        }
        var endingScreens = BuildEndingScreens(stage);
        // After the story views, before the notice bar and the menus (same order as a full build).
        var notice = stage.Find("Notice");
        if (notice) endingScreens.transform.SetSiblingIndex(notice.GetSiblingIndex());

        so.FindProperty("endingScreens").objectReferenceValue = endingScreens;
        so.FindProperty("endingScreensAsset").objectReferenceValue = AssetDatabase.LoadAssetAtPath<TextAsset>(EndingScreensPath);
        var sprites = Directory.GetFiles(ArtRoot + "Endings", "*.png").Select(p => AssetDatabase.LoadAssetAtPath<Sprite>(p.Replace('\\', '/'))).ToArray();
        var prop = so.FindProperty("endingBackgrounds");
        prop.arraySize = sprites.Length;
        for (int i = 0; i < sprites.Length; i++) prop.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"Assessment2DBuilder: added EndingScreens to {ScenePath} ({sprites.Length} ending backgrounds).");
    }

    // Targeted: deletes only EndingScreens/Screen_03_Assessment from the saved scene, trims the two screen
    // arrays to Reveal + Epilogue and wires the Epilogue header. Everything else stays as saved.
    public static void RemoveAssessmentScreenThenBuildPlayer()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var es = UnityEngine.Object.FindFirstObjectByType<EndingScreens>(FindObjectsInactive.Include);
        var assessment = es.transform.Find("Screen_03_Assessment");
        if (assessment) UnityEngine.Object.DestroyImmediate(assessment.gameObject);
        var so = new SerializedObject(es);
        so.FindProperty("screens").arraySize = 2;
        so.FindProperty("backgrounds").arraySize = 2;
        so.FindProperty("endingId").objectReferenceValue = es.transform.Find("Screen_02_Epilogue/Ending_Id").GetComponent<TMP_Text>();
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"Assessment2DBuilder: removed the Assessment screen from {ScenePath} (was present: {(bool)assessment}).");
        BuildPlayer();
    }

    public static void AddEndingScreensThenBuildPlayer()
    {
        AddEndingScreensToSavedScene();
        BuildPlayer();
    }

    const string AmbiencePath = "Assets/Audio/Ambience/Ambience_Assessment.mp3";

    // Additive (Phase 3): opens the saved scene, wires the ambience clip into the director (if unset) and
    // applies the "visual" hotspots from Scenes2D.json (existing ones are switched and re-placed, missing ones
    // added beneath the existing hotspots). Every other object stays exactly as saved. Safe to re-run.
    [MenuItem("The Last Assessment/Add Phase 3 To Saved Scene")]
    public static void AddPhase3ToSavedScene()
    {
        // A 5-minute bed: stream it instead of decompressing ~50 MB into memory.
        AssetDatabase.ImportAsset(AmbiencePath);
        var importer = (AudioImporter)AssetImporter.GetAtPath(AmbiencePath);
        var sample = importer.defaultSampleSettings;
        sample.loadType = AudioClipLoadType.Streaming;
        sample.compressionFormat = AudioCompressionFormat.Vorbis;
        sample.quality = 0.6f;
        importer.defaultSampleSettings = sample;
        importer.SaveAndReimport();

        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var director = UnityEngine.Object.FindFirstObjectByType<StoryDirector2D>(FindObjectsInactive.Include);
        var so = new SerializedObject(director);
        var clip = so.FindProperty("ambience");
        if (!clip.objectReferenceValue)
        {
            clip.objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(AmbiencePath);
            so.ApplyModifiedPropertiesWithoutUndo();
        }
        // Camera push / drift stay inside the 1920x1080 frame on letterboxed (21:9, 4:3) screens.
        var stageRt = (RectTransform)so.FindProperty("stage").objectReferenceValue;
        if (!stageRt.GetComponent<RectMask2D>()) stageRt.gameObject.AddComponent<RectMask2D>();

        var layout = JsonUtility.FromJson<Layout>(File.ReadAllText(LayoutPath));
        var views = UnityEngine.Object.FindObjectsByType<StoryView>(FindObjectsInactive.Include);
        int switched = 0, added = 0;
        foreach (var spec in layout.views)
        {
            var view = views.First(v => v.viewId == spec.id);
            var list = view.hotspots.ToList();
            foreach (var h in (spec.hotspots ?? Array.Empty<HotspotSpec>()).Reverse())
            {
                var existing = view.FindHotspot(h.name);
                if (existing)
                {
                    // Every existing hotspot takes its action from the data; only visual ones are re-placed
                    // (the advance/disabled rects are the original Figma ones).
                    existing.action = HotspotAction(h.action);
                    if (existing.IsVisual)
                    {
                        var rt = (RectTransform)existing.transform;
                        rt.anchoredPosition = new Vector2(h.rect[0], -h.rect[1]);
                        rt.sizeDelta = ((RectTransform)rt.Find("Outline")).sizeDelta = new Vector2(h.rect[2], h.rect[3]);
                    }
                    EditorUtility.SetDirty(existing);
                    switched++;
                    continue;
                }
                if (h.action != "visual") continue;
                var made = MakeHotspot(h.name, view.transform, h.rect[0], h.rect[1], h.rect[2], h.rect[3], Hotspot.Action.Visual);
                made.transform.SetSiblingIndex(view.transform.Find("Dim").GetSiblingIndex() + 1); // beneath the existing hotspots
                list.Insert(0, made);
                added++;
            }
            view.hotspots = list.ToArray();
            EditorUtility.SetDirty(view);
        }
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"Assessment2DBuilder: Phase 3 applied to {ScenePath}: ambience wired, {switched} hotspots synced, {added} added.");
    }

    public static void AddPhase3ThenBuildPlayer()
    {
        AddPhase3ToSavedScene();
        BuildPlayer();
    }

    static Hotspot.Action HotspotAction(string action) => action switch
    {
        "advance" => Hotspot.Action.Advance,
        "visual" => Hotspot.Action.Visual,
        _ => Hotspot.Action.Disabled,
    };

    // ---------- Views ----------

    static StoryView BuildView(ViewSpec spec, Transform stage)
    {
        var rt = ChamberBuilder.Stretch($"View_{spec.id}", stage);
        var view = rt.gameObject.AddComponent<StoryView>();
        view.viewId = spec.id;
        view.group = rt.gameObject.AddComponent<CanvasGroup>();
        var bg = ChamberBuilder.Stretch("Background", rt).gameObject.AddComponent<Image>();
        bg.raycastTarget = false;
        if (string.IsNullOrEmpty(spec.bg)) bg.color = Color.black;
        else
        {
            bg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtRoot}{spec.bg}.png");
            if (!bg.sprite) Debug.LogError($"Assessment2DBuilder: view {spec.id} background {spec.bg} not found");
        }
        view.dim = ChamberBuilder.Stretch("Dim", rt).gameObject.AddComponent<Image>();
        view.dim.color = Color.clear;
        view.dim.raycastTarget = false;

        // Hotspots sit above the art; later entries are on top.
        view.hotspots = (spec.hotspots ?? Array.Empty<HotspotSpec>())
            .Select(h => MakeHotspot(h.name, rt, h.rect[0], h.rect[1], h.rect[2], h.rect[3], HotspotAction(h.action)))
            .ToArray();

        var built = new Dictionary<string, (RectTransform rt, float x, float y)>();
        foreach (var e in spec.elements ?? Array.Empty<ElementSpec>()) BuildElement(e, rt, built, spec.id);
        if (spec.hud == "pending") RoomHud(rt);
        if (spec.hud == "progress") ProgressHud(rt);
        if (!string.IsNullOrEmpty(spec.captionLabel)) view.captionLabel = built[spec.captionLabel].rt.GetComponent<TMP_Text>();
        if (!string.IsNullOrEmpty(spec.captionValue)) view.captionValue = built[spec.captionValue].rt.GetComponent<TMP_Text>();

        view.dialogue = spec.dialogue switch
        {
            "room" => RoomDialogue(rt),
            "tablet" => TabletDialogue(rt),
            _ => CardDialogue(rt),
        };
        if (spec.question != null)
        {
            view.questionPanel = spec.question.style switch
            {
                "tablet" => TabletQuestion(rt),
                "images" => ImageQuestion(rt, spec.question),
                _ => CardQuestion(rt),
            };
            view.questionPanel.labelFormat = spec.question.label;
            view.questionPanel.hideOnAnswer = spec.question.hideOnAnswer;
        }
        return view;
    }

    static void BuildElement(ElementSpec e, RectTransform view, Dictionary<string, (RectTransform rt, float x, float y)> built, string viewId)
    {
        Transform parent = view;
        float px = 0f, py = 0f;
        if (!string.IsNullOrEmpty(e.parent))
        {
            if (!built.TryGetValue(e.parent, out var p)) { Debug.LogError($"Assessment2DBuilder: view {viewId}: parent '{e.parent}' not found"); return; }
            (parent, px, py) = (p.rt, p.x, p.y);
        }
        string name = string.IsNullOrEmpty(e.name) ? $"{e.type}_{built.Count}" : e.name;
        RectTransform rt;
        float ax, ay;
        switch (e.type)
        {
            case "text":
                var tmp = Text(name, parent, e.text, e.size, ColorOf(e.color), e.x - px, e.baseline - py, e.w, e.h, e.spacing, AlignOf(e.align));
                rt = tmp.rectTransform;
                (ax, ay) = (e.x, e.baseline - e.size);
                break;
            case "panel":
                rt = PanelAt(name, parent, e.rect[0] - px, e.rect[1] - py, e.rect[2], e.rect[3], Border);
                (ax, ay) = (e.rect[0], e.rect[1]);
                break;
            case "outline": // border only, e.g. the Q12 metadata highlight
                rt = At(name, parent, e.rect[0] - px, e.rect[1] - py, e.rect[2], e.rect[3]);
                ChamberBuilder.Panel(rt, Color.clear, out var lines).raycastTarget = false;
                foreach (var line in lines) line.color = ColorOf(e.color);
                (ax, ay) = (e.rect[0], e.rect[1]);
                break;
            case "group":
                rt = At(name, parent, e.rect[0] - px, e.rect[1] - py, e.rect[2], e.rect[3]);
                (ax, ay) = (e.rect[0], e.rect[1]);
                break;
            case "bar":
                rt = Bar(name, parent, e.rect[0] - px, e.rect[1] - py, e.rect[2], e.rect[3], ColorOf(e.color)).rectTransform;
                (ax, ay) = (e.rect[0], e.rect[1]);
                break;
            case "image":
                var img = At(name, parent, e.rect[0] - px, e.rect[1] - py, e.rect[2], e.rect[3]).gameObject.AddComponent<Image>();
                img.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtRoot}{e.sprite}.png");
                img.preserveAspect = true;
                img.raycastTarget = false;
                if (!img.sprite) Debug.LogError($"Assessment2DBuilder: view {viewId}: sprite {e.sprite} not found");
                rt = img.rectTransform;
                (ax, ay) = (e.rect[0], e.rect[1]);
                break;
            case "waveform":
                rt = WaveformBars(name, parent, e.rect[0] - px, e.rect[1] - py, e.rect[2], e.rect[3]);
                (ax, ay) = (e.rect[0], e.rect[1]);
                break;
            default:
                Debug.LogError($"Assessment2DBuilder: view {viewId}: unknown element type '{e.type}'");
                return;
        }
        if (!string.IsNullOrEmpty(e.caption))
        {
            var target = rt.gameObject.AddComponent<CaptionTarget>();
            target.caption = e.caption;
            if (e.applyCaption) target.text = rt.GetComponent<TMP_Text>();
        }
        if (e.hidden) rt.gameObject.SetActive(false);
        built[name] = (rt, ax, ay);
    }

    static RectTransform WaveformBars(string name, Transform parent, float x, float y, float w, float h)
    {
        var rt = At(name, parent, x, y, w, h);
        var wave = rt.gameObject.AddComponent<Waveform>();
        const int count = 72;
        float pitch = w / count;
        wave.bars = new RectTransform[count];
        wave.tints = new Graphic[count];
        wave.maxHeight = h;
        for (int i = 0; i < count; i++)
        {
            var bar = ChamberBuilder.Rect($"Bar_{i}", rt, new(0f, 0.5f), new(0.5f, 0.5f), new(i * pitch + pitch * 0.5f, 0f), new(pitch * 0.45f, 2f));
            var img = bar.gameObject.AddComponent<Image>();
            img.color = White;
            img.raycastTarget = false;
            wave.bars[i] = bar;
            wave.tints[i] = img;
        }
        return rt;
    }

    // ---------- HUD (Figma p1 and p3) ----------

    static void RoomHud(Transform view)
    {
        var hud = PanelAt("HUD", view, 64, 48, 600, 102, Border);
        Text("Subject", hud, "SUBJECT A-4172", 16, Cyan, 20, 85 - 48, 300, spacing: 3);
        Text("Org", hud, "ETERNA / CONTINUITY", 16, Grey, 279, 85 - 48, 300, spacing: 3, align: TextAlignmentOptions.TopRight);
        Text("Status", hud, "STATUS ASSESSMENT PENDING", 16, White, 20, 126 - 48, 400, spacing: 3);
    }

    static void ProgressHud(Transform view)
    {
        var hud = PanelAt("HUD", view, 64, 48, 560, 102, ChamberBuilder.Hex("#1D3A49"));
        Text("Subject", hud, "SUBJECT A-4172", 13, Cyan, 20, 81 - 48, 300, spacing: 3);
        Text("Org", hud, "ETERNA / CONTINUITY", 13, ChamberBuilder.Hex("#799AAC"), 239, 81 - 48, 300, spacing: 3, align: TextAlignmentOptions.TopRight);
        Bar("Divider", hud, 20, 51, 520, 1, ChamberBuilder.Hex("#1D495E"));
        Text("Status", hud, "ASSESSMENT IN PROGRESS", 12, ChamberBuilder.Hex("#A5C3CD"), 20, 127 - 48, 400, spacing: 3);
    }

    // ---------- Dialogue panels ----------

    // Figma p1 dialogue panel (prologue).
    static DialoguePanel RoomDialogue(Transform view)
    {
        var rt = PanelAt("Dialogue", view, 0, 793, 1267, 287, Border);
        var panel = rt.gameObject.AddComponent<DialoguePanel>();
        panel.group = rt.gameObject.AddComponent<CanvasGroup>();
        panel.speaker = Text("Speaker", rt, "", 16, Cyan, 64, 873 - 793, 600, spacing: 2);
        panel.body = Text("Body", rt, "", 26, White, 64, 925 - 793, 1140, height: 110);
        panel.continueIndicator = Bar("Continue", rt, 64, 250, 28, 2, Cyan);
        return panel;
    }

    // Figma p3 dialogue panel ("ASSESSOR / DIALOGUE").
    static DialoguePanel TabletDialogue(Transform view)
    {
        var rt = PanelAt("Dialogue", view, 64, 856, 1232, 160, Border);
        var panel = rt.gameObject.AddComponent<DialoguePanel>();
        panel.group = rt.gameObject.AddComponent<CanvasGroup>();
        panel.speakerSuffix = "DIALOGUE";
        Bar("Marker", rt, 20, 28, 6, 6, Cyan);
        panel.speaker = Text("Speaker", rt, "", 10, Cyan, 33, 891 - 856, 600, spacing: 3);
        panel.body = Text("Body", rt, "", 22, White, 20, 941 - 856, 1180, height: 60);
        panel.continueIndicator = Bar("Continue", rt, 20, 140, 28, 2, Cyan);
        return panel;
    }

    // Figma p8 / p9 / p22 bottom card (label 16 px at x 92, line 26 px).
    static DialoguePanel CardDialogue(Transform view)
    {
        var rt = PanelAt("Dialogue", view, 64, 840, 1180, 200, Border);
        var panel = rt.gameObject.AddComponent<DialoguePanel>();
        panel.group = rt.gameObject.AddComponent<CanvasGroup>();
        panel.speaker = Text("Speaker", rt, "", 16, Cyan, 28, 47, 900, spacing: 3);
        panel.body = Text("Body", rt, "", 26, White, 28, 96, 1124, height: 92);
        AutoSize(panel.body, 19f);
        panel.continueIndicator = Bar("Continue", rt, 28, 184, 28, 2, Cyan);
        return panel;
    }

    // ---------- Question panels ----------

    // Figma p3: question box over the tablet + RESPONSE OPTIONS.
    static QuestionPanel TabletQuestion(Transform view)
    {
        var root = ChamberBuilder.Stretch("Question", view);
        var panel = root.gameObject.AddComponent<QuestionPanel>();
        panel.group = root.gameObject.AddComponent<CanvasGroup>();
        var box = PanelAt("Question_Box", root, 624, 700, 672, 150, Border);
        panel.label = Text("Question_Label", box, "", 11, Cyan, 33, 731 - 700, 400, spacing: 3);
        panel.question = Text("Question_Text", box, "", 20, White, 20, 786 - 700, 632, height: 84);
        var options = OptionsColumn(root, 1334, 656, 522, top: true);
        Ignore(Text("Header", options, "RESPONSE OPTIONS", 10, Cyan, 19, 680 - 656, 300, spacing: 3));
        var footer = Text("Footer", options, "SELECT OR SPEAK RESPONSE", 10, Grey, 0, 0, 300, spacing: 3, align: TextAlignmentOptions.BottomRight);
        Ignore(footer);
        footer.rectTransform.anchorMin = footer.rectTransform.anchorMax = footer.rectTransform.pivot = new Vector2(1f, 0f);
        footer.rectTransform.anchoredPosition = new Vector2(-16f, 12f);
        options.GetComponent<VerticalLayoutGroup>().padding = new RectOffset(8, 8, 46, 42);
        panel.options = Enumerable.Range(0, 4).Select(i => OptionRow(options, i, 506f)).ToArray();
        return panel;
    }

    // Figma p22: question in the bottom card, options in a column at x 1300 growing upward.
    static QuestionPanel CardQuestion(Transform view)
    {
        var root = ChamberBuilder.Stretch("Question", view);
        var panel = root.gameObject.AddComponent<QuestionPanel>();
        panel.group = root.gameObject.AddComponent<CanvasGroup>();
        var card = PanelAt("Question_Card", root, 64, 840, 1180, 200, Border);
        panel.label = Text("Question_Label", card, "", 16, Cyan, 28, 47, 1000, spacing: 3);
        panel.question = Text("Question_Text", card, "", 26, White, 28, 96, 1124, height: 92);
        AutoSize(panel.question, 19f);
        var options = OptionsColumn(root, 1276, 1040, 580, top: false);
        panel.options = Enumerable.Range(0, 4).Select(i => OptionRow(options, i, 564f)).ToArray();
        return panel;
    }

    // Figma p4: the four memory images are the A–D options (Q2); the canonical choice text labels each tile.
    static QuestionPanel ImageQuestion(Transform view, QuestionSpec spec)
    {
        var root = ChamberBuilder.Stretch("Question", view);
        var panel = root.gameObject.AddComponent<QuestionPanel>();
        panel.group = root.gameObject.AddComponent<CanvasGroup>();
        panel.autoHeight = false;
        var frame = PanelAt("Question_Frame", root, 222, 110, 1476, 760, Border);
        panel.label = Text("Question_Label", frame, "", 16, Cyan, 64, 173 - 110, 900, spacing: 3);
        panel.question = Text("Question_Text", frame, "", 26, White, 64, 225 - 110, 1350, height: 40);
        panel.options = spec.tiles.Select((t, i) =>
        {
            var rt = At($"Option_{(char)('A' + i)}", root, t.rect[0], t.rect[1], t.rect[2], t.rect[3]);
            rt.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            var bg = ChamberBuilder.Panel(rt, new Color(0.039f, 0.051f, 0.067f, 0.55f), out var edges);
            var raw = At("Image", rt, 1, 1, t.rect[2] - 2, t.rect[3] - 44).gameObject.AddComponent<RawImage>();
            raw.texture = AssetDatabase.LoadAssetAtPath<Texture2D>($"{ArtRoot}{t.sprite}.png");
            if (!raw.texture) Debug.LogError($"Assessment2DBuilder: Q2 tile {t.sprite} not found");
            float aspect = (t.rect[2] - 2) / (t.rect[3] - 44);
            raw.uvRect = new Rect(0f, 0.5f - 0.5f / aspect, 1f, 1f / aspect); // centre crop, as Figma's fill
            raw.raycastTarget = false;
            var button = rt.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.targetGraphic = bg;
            var choice = rt.gameObject.AddComponent<ChoiceButton>();
            choice.background = bg;
            choice.border = edges;
            choice.chevron = Text("Letter", rt, "", 12, Cyan, 12, t.rect[3] - 14, 24, spacing: 2);
            choice.label = Text("Label", rt, "", 14, White, 36, t.rect[3] - 14, t.rect[2] - 48, spacing: 0.5f);
            return choice;
        }).ToArray();
        return panel;
    }

    static RectTransform OptionsColumn(Transform parent, float x, float y, float w, bool top)
    {
        var rt = ChamberBuilder.Rect("Response_Options", parent, new(0f, 1f), new(0f, top ? 1f : 0f), new(x, -y), new(w, 10f));
        var fill = ChamberBuilder.Panel(rt, top ? PanelFill : Color.clear, out var edges);
        fill.raycastTarget = false;
        foreach (var edge in edges) edge.color = top ? Border : Color.clear;
        var layout = rt.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(8, 8, 8, 8);
        layout.spacing = 10f;
        layout.childControlWidth = layout.childForceExpandWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandHeight = false;
        rt.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        return rt;
    }

    // Response row: A–D letter at left, text 20 px; grows when the text wraps.
    static ChoiceButton OptionRow(Transform parent, int index, float width)
    {
        var rt = ChamberBuilder.Rect($"Option_{(char)('A' + index)}", parent, new(0f, 1f), new(0f, 1f), Vector2.zero, new(width, 48f));
        rt.gameObject.AddComponent<LayoutElement>().preferredHeight = 48f;
        var bg = ChamberBuilder.Panel(rt, new Color(0.039f, 0.051f, 0.067f, 0.85f), out var edges);
        var button = rt.gameObject.AddComponent<Button>();
        button.transition = Selectable.Transition.None;
        button.targetGraphic = bg;
        var choice = rt.gameObject.AddComponent<ChoiceButton>();
        choice.background = bg;
        choice.border = edges;
        choice.chevron = Text("Letter", rt, "", 11, Cyan, 12, 25, 24, spacing: 2);
        choice.label = Text("Label", rt, "", 20, White, 40, 31, width - 56, height: 60);
        return choice;
    }

    // ---------- Ending screens (Untitled.fig: 01 Reveal / 02 Epilogue; the .fig's Assessment screen is not used) ----------

    static EndingScreens BuildEndingScreens(Transform stage)
    {
        var root = ChamberBuilder.Stretch("EndingScreens", stage);
        var es = root.gameObject.AddComponent<EndingScreens>();
        es.root = root.gameObject.AddComponent<CanvasGroup>();
        es.root.alpha = 0f;
        es.root.blocksRaycasts = false;
        var grey = ChamberBuilder.Hex("#A5B4BE");
        var ink = ChamberBuilder.Hex("#020405");

        Transform Screen(int i, string name, float veil)
        {
            var s = ChamberBuilder.Stretch(name, root);
            es.screens[i] = s.gameObject.AddComponent<CanvasGroup>();
            var bg = ChamberBuilder.Stretch("Background", s).gameObject.AddComponent<Image>();
            bg.raycastTarget = false;
            es.backgrounds[i] = bg;
            var v = ChamberBuilder.Stretch("Veil", s).gameObject.AddComponent<Image>();
            v.color = new Color(ink.r, ink.g, ink.b, veil);
            v.raycastTarget = false;
            if (i == 0) es.revealVeil = v;
            return s;
        }
        TextMeshProUGUI Header(Transform s, int i, string screenLabel)
        {
            var id = Text("Ending_Id", s, "", 16, grey, 64, 64, 1400, spacing: 1);
            Text("Screen_Label", s, screenLabel, 14, Cyan, 1456, 62, 400, spacing: 1, align: TextAlignmentOptions.TopRight);
            Bar("Header_Hairline", s, 64, 92, 1792, 1, Border);
            return id;
        }

        // 01 Reveal: centred title block (Endings 2–6) or tablet status panel (Ending 1).
        var reveal = Screen(0, "Screen_01_Reveal", 0.55f);
        Text("Ending_Id", reveal, "", 16, grey, 64, 64, 1400, spacing: 1).gameObject.SetActive(false);
        var center = At("Center_Block", reveal, 160, 330, 1600, 460);
        var stack = center.gameObject.AddComponent<VerticalLayoutGroup>();
        stack.childAlignment = TextAnchor.MiddleCenter;
        stack.spacing = 24f;
        stack.childControlWidth = stack.childControlHeight = true;
        stack.childForceExpandWidth = stack.childForceExpandHeight = false;
        es.centerBlock = center.gameObject;
        es.revealOverline = Text("Overline", center, "", 14, Cyan, 0, 0, 1600, spacing: 4, align: TextAlignmentOptions.Center);
        es.revealTitle = Text("Title", center, "", 96, White, 0, 0, 1600, spacing: 12, align: TextAlignmentOptions.Center);
        es.revealTitle.lineSpacing = -18f;
        var rule = Bar("Rule", center, 0, 0, 120, 1, Cyan);
        var ruleLayout = rule.gameObject.AddComponent<LayoutElement>();
        ruleLayout.preferredWidth = 120f; ruleLayout.preferredHeight = 1f;
        es.revealSubline = Text("Subline", center, "", 16, grey, 0, 0, 1600, spacing: 3, align: TextAlignmentOptions.Center);
        es.revealFoot = Text("Foot", reveal, "", 17, grey, 460, 932, 1000, spacing: 2, align: TextAlignmentOptions.Top);
        var tablet = ChamberBuilder.Stretch("Tablet_Block", reveal);
        es.tabletBlock = tablet.gameObject;
        es.tabletOverline = Text("Overline", tablet, "", 18, Cyan, 310, 218, 1300, spacing: 1, align: TextAlignmentOptions.Top);
        es.tabletTitle = Text("Title", tablet, "", 80, White, 150, 338, 1620, spacing: 2, align: TextAlignmentOptions.Top);
        var display = PanelAt("Tablet_Display", tablet, 738, 590, 442, 143, ChamberBuilder.Hex("#20292E"));
        display.GetComponent<Image>().color = ink;
        es.tabletLabel = Text("System_Label", display, "", 10.4f, Cyan, 23, 34, 400, spacing: 1.5f);
        es.tabletValue = Text("Valuation", display, "", 39, White, 23, 92, 400, spacing: 1);

        // 02 Epilogue: centred outcome column (shifted left for Ending 5's records panel).
        var epilogue = Screen(1, "Screen_02_Epilogue", 0.72f);
        es.endingId = Header(epilogue, 1, "02 / EPILOGUE");
        var column = ChamberBuilder.Stretch("Epilogue_Column", epilogue);
        es.epilogueColumn = column;
        Text("Outcome_Label", column, "FINAL ASSESSMENT / OUTCOME", 16, Cyan, 410, 220, 1100, spacing: 1, align: TextAlignmentOptions.Top);
        es.epilogueHeading = Text("Outcome_Heading", column, "", 58, White, 410, 308, 1100, spacing: 2, align: TextAlignmentOptions.Top);
        Bar("Epilogue_Hairline", column, 720, 365, 480, 1, Border);
        es.epilogueText = Text("Epilogue_Text", column, "", 24, White, 440, 434, 1040, height: 260, align: TextAlignmentOptions.Top);
        es.epilogueText.lineSpacing = 30f;
        es.epilogueStatus = Text("Ending_Status", column, "", 18, Cyan, 410, 788, 1100, spacing: 1, align: TextAlignmentOptions.Top);
        var records = PanelAt("Records_Panel", epilogue, 1260, 270, 560, 382, Border);
        es.recordsPanel = records.gameObject;
        es.recordsText = Text("Records_Text", records, "", 18, ChamberBuilder.Hex("#E3F0F5"), 20, 44, 520, height: 330, spacing: 1);

        // Shared navigation: ← (back), NEXT → / NEW RUN, and "0N / 03".
        es.back = NavButton("Back", root, 64, 964, 82, "←", out _);
        es.next = NavButton("Next", root, 1656, 964, 200, "NEXT →", out es.nextLabel);
        es.nextColor = White;
        es.newRunColor = Cyan;
        es.sequence = Text("Sequence", root, "", 14, grey, 864, 996, 192, align: TextAlignmentOptions.Top);
        return es;
    }

    static Button NavButton(string name, Transform parent, float x, float y, float w, string text, out TMP_Text label)
    {
        var rt = PanelAt(name, parent, x, y, w, 52, Border);
        var bg = rt.GetComponent<Image>();
        bg.color = Color.white;
        bg.raycastTarget = true;
        var button = rt.gameObject.AddComponent<Button>();
        button.targetGraphic = bg;
        var colors = button.colors;
        colors.normalColor = new Color(0.024f, 0.047f, 0.063f, 0.85f);
        colors.highlightedColor = colors.selectedColor = new Color(0.07f, 0.2f, 0.27f, 0.95f);
        colors.pressedColor = new Color(0.11f, 0.3f, 0.38f, 1f);
        colors.disabledColor = new Color(0.024f, 0.047f, 0.063f, 0.35f);
        button.colors = colors;
        button.navigation = new Navigation { mode = Navigation.Mode.None }; // ← / → are handled as back / next
        label = Text("Label", rt, text, 17, White, 0, 36, w, align: TextAlignmentOptions.Top);
        return button;
    }

    // ---------- Menus ----------

    static GameMenus BuildMenus(Transform stage)
    {
        var root = ChamberBuilder.Stretch("Menus", stage);
        var menus = root.gameObject.AddComponent<GameMenus>();

        // Title: the designed room as backdrop, the game title, and New / Continue / Quit.
        var title = ChamberBuilder.Stretch("Title", root);
        menus.title = title.gameObject.AddComponent<CanvasGroup>();
        var titleBg = ChamberBuilder.Stretch("Background", title).gameObject.AddComponent<Image>();
        titleBg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtRoot}Backgrounds/BG_0-1_AssessmentRoom.png");
        var shade = ChamberBuilder.Stretch("Shade", title).gameObject.AddComponent<Image>();
        shade.color = new Color(0f, 0f, 0f, 0.55f);
        Text("Game_Title", title, "THE LAST ASSESSMENT", 72, White, 120, 470, 1400, spacing: 6);
        Bar("Title_Rule", title, 122, 500, 56, 2, Cyan);
        var titleMenu = MenuColumn("Title_Menu", title, 120, 560);
        menus.newGame = MenuButton("New_Game", titleMenu, "NEW GAME");
        menus.continueGame = MenuButton("Continue", titleMenu, "CONTINUE");
        menus.quitFromTitle = MenuButton("Quit", titleMenu, "QUIT");

        var confirm = ChamberBuilder.Stretch("Confirm", root);
        menus.confirm = confirm.gameObject.AddComponent<CanvasGroup>();
        confirm.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);
        var dialog = PanelAt("Confirm_Panel", confirm, 600, 400, 720, 260, Border);
        Text("Confirm_Text", dialog, "Start a new assessment? Your saved progress will be lost.", 22, White, 40, 70, 640, height: 70);
        var confirmMenu = MenuColumn("Confirm_Menu", dialog, 40, 130, horizontal: true);
        menus.confirmYes = MenuButton("Start_Over", confirmMenu, "START OVER");
        menus.confirmNo = MenuButton("Cancel", confirmMenu, "CANCEL");

        var pause = ChamberBuilder.Stretch("Pause", root);
        menus.pause = pause.gameObject.AddComponent<CanvasGroup>();
        pause.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);
        var pausePanel = PanelAt("Pause_Panel", pause, 760, 300, 400, 420, Border);
        Text("Pause_Label", pausePanel, "ASSESSMENT PAUSED", 13, Cyan, 40, 60, 320, spacing: 3);
        var pauseMenu = MenuColumn("Pause_Menu", pausePanel, 40, 100);
        menus.resume = MenuButton("Resume", pauseMenu, "RESUME");
        menus.openJournal = MenuButton("Evidence", pauseMenu, "EVIDENCE");
        menus.toTitle = MenuButton("Title", pauseMenu, "QUIT TO TITLE");
        menus.quitGame = MenuButton("Quit", pauseMenu, "QUIT GAME");

        var journal = ChamberBuilder.Stretch("Journal", root);
        menus.journal = journal.gameObject.AddComponent<CanvasGroup>();
        journal.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);
        var journalPanel = PanelAt("Journal_Panel", journal, 360, 160, 1200, 760, Border);
        menus.journalHeader = Text("Journal_Header", journalPanel, "", 16, Cyan, 48, 64, 900, spacing: 3);
        Bar("Journal_Rule", journalPanel, 48, 84, 1104, 1, Border);
        menus.journalBody = Text("Journal_Body", journalPanel, "", 24, White, 48, 140, 1104, height: 500);
        menus.journalBody.richText = true;
        var journalMenu = MenuColumn("Journal_Menu", journalPanel, 48, 680);
        menus.closeJournal = MenuButton("Close", journalMenu, "CLOSE");
        return menus;
    }

    static RectTransform MenuColumn(string name, Transform parent, float x, float y, bool horizontal = false)
    {
        var rt = At(name, parent, x, y, horizontal ? 640 : 320, horizontal ? 52 : 260);
        HorizontalOrVerticalLayoutGroup layout = horizontal ? rt.gameObject.AddComponent<HorizontalLayoutGroup>() : rt.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = horizontal ? 24f : 14f;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = layout.childControlHeight = false;
        return rt;
    }

    static ChoiceButton MenuButton(string name, Transform parent, string text)
    {
        var button = ChamberBuilder.MakeChoiceButton(name, parent, new(300f, 52f), text);
        button.label.fontSize = 16f;
        button.label.characterSpacing = 12f;
        button.label.rectTransform.anchoredPosition = new(28f, -16f);
        if (font) button.label.font = button.chevron.font = font;
        return button;
    }

    // ---------- Primitives ----------

    static Hotspot MakeHotspot(string name, Transform parent, float x, float y, float w, float h, Hotspot.Action action)
    {
        var rt = At(name, parent, x, y, w, h);
        var img = rt.gameObject.AddComponent<Image>();
        img.color = Color.clear;
        var button = rt.gameObject.AddComponent<Button>();
        button.transition = Selectable.Transition.None;
        var hotspot = rt.gameObject.AddComponent<Hotspot>();
        hotspot.action = action;
        ChamberBuilder.Panel(At("Outline", rt, 0, 0, w, h), Color.clear, out var edges).raycastTarget = false;
        foreach (var edge in edges)
        {
            edge.color = Cyan;
            ((RectTransform)edge.transform).sizeDelta = Vector2.Max(((RectTransform)edge.transform).sizeDelta * 2f, Vector2.zero); // 2 px lines
            edge.enabled = false;
        }
        hotspot.outline = edges;
        return hotspot;
    }

    // Translucent black panel with 1 px border, positioned in Figma coordinates relative to its parent.
    static RectTransform PanelAt(string name, Transform parent, float x, float y, float w, float h, Color border)
    {
        var rt = At(name, parent, x, y, w, h);
        var fill = ChamberBuilder.Panel(rt, PanelFill, out var edges);
        fill.raycastTarget = false;
        foreach (var edge in edges) edge.color = border;
        return rt;
    }

    static Image Bar(string name, Transform parent, float x, float y, float w, float h, Color color)
    {
        var img = At(name, parent, x, y, w, h).gameObject.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    // y is the Figma text baseline; the box top sits one font size above it.
    static TextMeshProUGUI Text(string name, Transform parent, string text, float size, Color color, float x, float baseline, float width,
        float height = 0f, float spacing = 0f, TextAlignmentOptions align = TextAlignmentOptions.TopLeft)
    {
        var rt = At(name, parent, x, baseline - size, width, height > 0f ? height : size * 1.4f);
        var tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
        if (font) tmp.font = font;
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.characterSpacing = spacing;
        tmp.alignment = align;
        tmp.raycastTarget = false;
        return tmp;
    }

    static void AutoSize(TMP_Text t, float min)
    {
        t.enableAutoSizing = true;
        t.fontSizeMax = t.fontSize;
        t.fontSizeMin = min;
    }

    static RectTransform At(string name, Transform parent, float x, float y, float w, float h) =>
        ChamberBuilder.Rect(name, parent, new(0f, 1f), new(0f, 1f), new(x, -y), new(w, h));

    static void Ignore(Component c) => c.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;

    static Color ColorOf(string c) => c switch
    {
        null or "" or "white" => White,
        "cyan" => Cyan,
        "grey" => Grey,
        "border" => Border,
        "dark" => Dark,
        _ => ChamberBuilder.Hex(c),
    };

    static TextAlignmentOptions AlignOf(string a) => a switch
    {
        "center" => TextAlignmentOptions.Top,
        "right" => TextAlignmentOptions.TopRight,
        _ => TextAlignmentOptions.TopLeft,
    };

    // Colours sampled from the rendered Figma frames; overridable in Scenes2D.json "style".
    static void ApplyStyle(Style s)
    {
        Color Pick(string hex, string fallback) => ChamberBuilder.Hex(string.IsNullOrEmpty(hex) ? fallback : hex);
        Cyan = Pick(s.cyan, "#6EB8D6");
        White = Pick(s.white, "#EDF2F4");
        Grey = Pick(s.grey, "#A5B3BD");
        Border = Pick(s.border, "#344B59");
        Dark = Pick(s.dark, "#2A2F33");
        var panel = Pick(s.panel, "#060D10");
        PanelFill = new Color(panel.r, panel.g, panel.b, s.panelAlpha > 0f ? s.panelAlpha : 0.9f);
        // Figma's fonts are not available yet (flattened in the PDF); empty = TextMeshPro default (LiberationSans).
        font = string.IsNullOrEmpty(s.font) ? null : AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(s.font);
    }

    static void ImportSprite(string path)
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        if (importer == null) { AssetDatabase.ImportAsset(path); importer = (TextureImporter)AssetImporter.GetAtPath(path); }
        if (importer.textureType == TextureImporterType.Sprite && !importer.mipmapEnabled && importer.maxTextureSize == 2048) return;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.filterMode = FilterMode.Bilinear;
        importer.maxTextureSize = 2048;
        importer.alphaIsTransparency = true;
        importer.SaveAndReimport();
    }
}
