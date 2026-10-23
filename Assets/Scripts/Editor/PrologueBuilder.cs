using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// Builds the Prologue scene (opening narrative screen) and places it before AssessmentChamber in the build.
// Rerunnable: regenerates the scene and its background texture from scratch.
public static class PrologueBuilder
{
    const string ScenePath = "Assets/Scenes/Prologue.unity";
    const string ChamberPath = "Assets/Scenes/AssessmentChamber.unity";
    const string BackgroundPath = "Assets/Art/UI/Prologue_Background.png";

    // Approved three-paragraph prologue (condensed from the supplied text; no new lore).
    static readonly string[] Paragraphs =
    {
        "There was a time when a memory belonged to the person who lived it. Then Eterna Life Continuity turned memory preservation into an industry: for a price, the experiences and skills that made people who they were could outlast them. The company called it continuity. The world learned to call it necessary.",
        "But credits decided what survived, and records decided what a life was worth. The protagonist once audited those records inside Eterna, and found things that did not fit the company's promises. Before the truth could reach the public, the records changed—and the auditor's life came to an abrupt end, officially recorded as an accident.",
        "Now there is only darkness, a low mechanical hum, and the suggestion of a room waiting beyond it. Someone—or something—is ready to ask questions about the life that has ended.",
    };
    const string Quote = "“Your previous life has concluded. Your next-life allocation has not yet been finalised.”";
    const string Closing = "The assessment is about to begin.";

    static readonly Color BodyText = ChamberBuilder.Hex("#C9CDD1");
    static readonly Color MutedText = ChamberBuilder.Hex("#737B83");

    [MenuItem("The Last Assessment/Build Prologue Scene")]
    public static void Build()
    {
        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            Debug.LogError("PrologueBuilder: save the open scene first; building opens a new scene.");
            return;
        }

        var background = BuildBackgroundSprite();
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)) { tag = "MainCamera" };
        var cam = camGo.GetComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = ChamberBuilder.Hex("#050607");

        var canvasGo = new GameObject("Prologue_UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        var canvas = canvasGo.transform;

        var bg = Stretch("Background", canvas).gameObject.AddComponent<Image>();
        bg.sprite = background;
        bg.raycastTarget = false;

        // Stage 1: title block — the game title is the screen's heading.
        var titleStage = Stage("Stage_Title", canvas);
        var title = Label("Game_Title", titleStage, "THE LAST ASSESSMENT", 40f, ChamberBuilder.TextWhite, -104f, 1200f, 56f, spacing: 18f);
        var subtitle = Label("Subtitle", titleStage, "A psychological science-fiction mystery", 16f, MutedText, -166f, 900f, 26f, spacing: 2f, style: FontStyles.Italic);
        Rule("Title_Rule", titleStage, -216f);

        // Stage 2: body — autosizes inside a fixed box so it can never clip.
        var bodyStage = Stage("Stage_Body", canvas);
        var body = Label("Prologue_Body", bodyStage, string.Join("\n", Paragraphs), 24f, BodyText, -254f, 900f, 430f, align: TextAlignmentOptions.TopLeft);
        body.enableAutoSizing = true;
        body.fontSizeMin = 18f;
        body.fontSizeMax = 24f;
        body.lineSpacing = 22f;
        body.paragraphSpacing = 48f;

        // Stage 3: the Assessor's line, set apart as an understated quotation.
        var quoteStage = Stage("Stage_Quote", canvas);
        Rule("Quote_Rule", quoteStage, -722f);
        var quote = Label("Quote", quoteStage, Quote, 23f, ChamberBuilder.TextWhite, -746f, 1100f, 40f, spacing: 0.5f, style: FontStyles.Italic);

        // Stage 4: closing line.
        var closingStage = Stage("Stage_Closing", canvas);
        var closing = Label("Closing", closingStage, Closing, 18f, MutedText, -812f, 900f, 30f, spacing: 3f);

        new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));

        // NEW GAME / CONTINUE in the chamber's response-button style (cyan only on the selection).
        var menu = ButtonRow("Menu", canvas, new(0f, 40f));
        var newGame = MenuButton("New_Game", menu, "NEW GAME");
        var resume = MenuButton("Continue", menu, "CONTINUE");

        // Start-over confirmation (system UI, not story dialogue).
        var confirm = Stretch("Confirm_Dialog", canvas);
        confirm.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);
        var confirmGroup = confirm.gameObject.AddComponent<CanvasGroup>();
        var dialog = ChamberBuilder.Rect("Confirm_Panel", confirm, new(0.5f, 0.5f), new(0.5f, 0.5f), Vector2.zero, new(660f, 200f));
        ChamberBuilder.Panel(dialog, new Color(0.035f, 0.047f, 0.063f, 0.95f));
        Label("Confirm_Text", dialog, "Start a new assessment? Your saved progress will be lost.", 20f, ChamberBuilder.TextWhite, -40f, 600f, 30f);
        var confirmRow = ButtonRow("Confirm_Buttons", dialog, new(0f, 30f));
        var startOver = MenuButton("Start_Over", confirmRow, "START OVER");
        var cancel = MenuButton("Cancel", confirmRow, "CANCEL");

        // Exit fade to black (above everything).
        var exit = Stretch("Exit_Fade", canvas);
        exit.gameObject.AddComponent<Image>().color = Color.black;
        var exitGroup = exit.gameObject.AddComponent<CanvasGroup>();
        exitGroup.alpha = 0f;
        exitGroup.blocksRaycasts = false;

        var screen = canvasGo.AddComponent<PrologueScreen>();
        var so = new SerializedObject(screen);
        var stages = so.FindProperty("stages");
        // Typing order and pace: deliberate title, quote and closing; faster for the narrative paragraphs.
        var stageSpecs = new (Transform group, TMP_Text[] texts, float cps)[]
        {
            (titleStage, new TMP_Text[] { title, subtitle }, 24f),
            (bodyStage, new TMP_Text[] { body }, 100f),
            (quoteStage, new TMP_Text[] { quote }, 42f),
            (closingStage, new TMP_Text[] { closing }, 32f),
        };
        stages.arraySize = stageSpecs.Length;
        for (int i = 0; i < stageSpecs.Length; i++)
        {
            var element = stages.GetArrayElementAtIndex(i);
            element.FindPropertyRelative("group").objectReferenceValue = stageSpecs[i].group.GetComponent<CanvasGroup>();
            element.FindPropertyRelative("charsPerSecond").floatValue = stageSpecs[i].cps;
            var texts = element.FindPropertyRelative("texts");
            texts.arraySize = stageSpecs[i].texts.Length;
            for (int j = 0; j < stageSpecs[i].texts.Length; j++)
                texts.GetArrayElementAtIndex(j).objectReferenceValue = stageSpecs[i].texts[j];
        }
        so.FindProperty("exitFade").objectReferenceValue = exitGroup;
        so.FindProperty("scriptAsset").objectReferenceValue = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Narrative/Data/AssessmentScript.json");
        so.FindProperty("menu").objectReferenceValue = menu.GetComponent<CanvasGroup>();
        so.FindProperty("newGameButton").objectReferenceValue = newGame;
        so.FindProperty("continueButton").objectReferenceValue = resume;
        so.FindProperty("confirm").objectReferenceValue = confirmGroup;
        so.FindProperty("confirmButton").objectReferenceValue = startOver;
        so.FindProperty("cancelButton").objectReferenceValue = cancel;
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.SaveScene(scene, ScenePath);

        // Prologue first, then the chamber; keep any other scenes after them.
        var others = EditorBuildSettings.scenes.Where(s => s.path != ScenePath && s.path != ChamberPath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true), new EditorBuildSettingsScene(ChamberPath, true) }
            .Concat(others).ToArray();

        Debug.Log("PrologueBuilder: Prologue scene built, saved and placed first in Build Settings.");
    }

    static RectTransform ButtonRow(string name, Transform parent, Vector2 bottomOffset)
    {
        var row = ChamberBuilder.Rect(name, parent, new(0.5f, 0f), new(0.5f, 0f), bottomOffset, new(540f, 52f));
        var layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 28f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = layout.childControlHeight = false;
        row.gameObject.AddComponent<CanvasGroup>();
        return row;
    }

    static ChoiceButton MenuButton(string name, Transform parent, string text)
    {
        var button = ChamberBuilder.MakeChoiceButton(name, parent, new(256f, 52f), text);
        button.label.fontSize = 16f;
        button.label.characterSpacing = 12f;
        button.label.rectTransform.anchoredPosition = new(28f, -16f);
        return button;
    }

    static RectTransform Stretch(string name, Transform parent)
    {
        var rt = ChamberBuilder.Rect(name, parent, Vector2.zero, new(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        return rt;
    }

    static Transform Stage(string name, Transform parent)
    {
        var rt = Stretch(name, parent);
        rt.gameObject.AddComponent<CanvasGroup>();
        return rt;
    }

    static TextMeshProUGUI Label(string name, Transform parent, string text, float size, Color color, float y, float width, float height,
        float spacing = 0f, FontStyles style = FontStyles.Normal, TextAlignmentOptions align = TextAlignmentOptions.Top)
    {
        var rt = ChamberBuilder.Rect(name, parent, new(0.5f, 1f), new(0.5f, 1f), new(0f, y), new(width, height));
        var tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.characterSpacing = spacing;
        tmp.fontStyle = style;
        tmp.alignment = align;
        tmp.raycastTarget = false;
        return tmp;
    }

    static void Rule(string name, Transform parent, float y)
    {
        var img = ChamberBuilder.Rect(name, parent, new(0.5f, 1f), new(0.5f, 1f), new(0f, y), new(56f, 1f)).gameObject.AddComponent<Image>();
        img.color = ChamberBuilder.PanelBorder;
        img.raycastTarget = false;
    }

    // Soft charcoal radial gradient, lighter just above centre, dithered to avoid banding.
    static Sprite BuildBackgroundSprite()
    {
        const int w = 640, h = 360;
        var inner = ChamberBuilder.Hex("#15181C");
        var outer = ChamberBuilder.Hex("#050607");
        var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
        var rng = new System.Random(7);
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            float dx = (x / (float)w - 0.5f) * 1.6f, dy = y / (float)h - 0.58f;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(Mathf.Sqrt(dx * dx + dy * dy) / 0.75f));
            var c = Color.Lerp(inner, outer, t);
            float n = ((float)rng.NextDouble() - 0.5f) / 255f;
            tex.SetPixel(x, y, new Color(c.r + n, c.g + n, c.b + n));
        }
        Directory.CreateDirectory(Path.GetDirectoryName(BackgroundPath));
        File.WriteAllBytes(BackgroundPath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);

        AssetDatabase.ImportAsset(BackgroundPath);
        var importer = (TextureImporter)AssetImporter.GetAtPath(BackgroundPath);
        importer.textureType = TextureImporterType.Sprite;
        importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundPath);
    }
}
