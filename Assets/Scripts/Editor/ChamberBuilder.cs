using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

// Builds the approved interrogation chamber into the open AssessmentChamber scene.
// Rerunnable: reuses the original greybox objects by name and rebuilds only its own roots.
// Framing, proportions and placement are measured from the approved Scene 1 reference image.
public static class ChamberBuilder
{
    const string MatDir = "Assets/Art/Materials/Chamber";
    const string ProfilePath = "Assets/Settings/ChamberVolumeProfile.asset";
    const string BuildRoot = "Chamber_Build";
    const string UiRoot = "Assessment_UI";

    // Camera and room anchors (metres). Back wall inner face is z = 4.7, glass at z = 4.85.
    // Level camera at seated eye height: sill lands at ~46% of frame height, desk top at ~55%.
    static readonly Vector3 CameraPos = new(0f, 0.85f, 1.1f);
    const float CameraFov = 45f;
    const float WindowHalfWidth = 2.5f, SillY = 1.0f, WindowTopY = 2.28f;

    // Narrow observation room behind the glass so its side walls and doorways read through the window.
    const float ObsHalfWidth = 2.2f, ObsNear = 5.0f, ObsFar = 7.1f, ObsCeiling = 2.6f;

    // Desk: pale top at 0.75 m, front edge ~2.4 m from the camera.
    const float DeskZ = 3.875f, DeskHalfWidth = 1.05f, DeskHalfDepth = 0.375f, DeskTop = 0.75f;

    [MenuItem("The Last Assessment/Build Assessment Chamber")]
    public static void Build()
    {
        if (Resources.Load<TMP_Settings>("TMP Settings") == null)
        {
            AssetDatabase.ImportPackage("Packages/com.unity.ugui/Package Resources/TMP Essential Resources.unitypackage", false);
            Debug.LogWarning("ChamberBuilder: imported TMP Essential Resources. Run the build again once the import finishes.");
            return;
        }

        if (!AssetDatabase.IsValidFolder(MatDir))
            AssetDatabase.CreateFolder("Assets/Art/Materials", "Chamber");

        var m = new Mats();
        foreach (var name in new[] { BuildRoot, UiRoot, "EventSystem" })
        {
            var old = GameObject.Find(name);
            if (old) Object.DestroyImmediate(old);
        }

        var root = new GameObject(BuildRoot).transform;
        BuildArchitecture(root, m);
        BuildObservationRoom(root, m);
        BuildFurniture(root, m);
        BuildProps(root, m);
        BuildLighting(root);
        BuildCamera();
        BuildUi();

        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log("ChamberBuilder: Assessment chamber built and scene saved.");
    }

    // ---------- Materials ----------
    // Darks stay above pure black so edges separate from the background in shadow.

    class Mats
    {
        public readonly Material Wall = Lit("Chamber_Wall", "#2E3135", 0.36f);
        public readonly Material Recess = Lit("Chamber_Recess", "#1A1C1F", 0.3f);
        public readonly Material Floor = Lit("Chamber_Floor_Concrete", "#1E2023", 0.72f);
        public readonly Material Ceiling = Lit("Chamber_Ceiling", "#1E2023", 0.15f);
        public readonly Material Frame = Lit("Chamber_Frame_Black", "#24272B", 0.72f, 0.75f);
        public readonly Material FrameEdge = Lit("Chamber_Frame_Edge", "#4A4F56", 0.8f, 0.8f);
        public readonly Material Glass = GlassMat();
        public readonly Material DeskTop = Lit("Desk_Top_Laminate", "#D6D2CB", 0.22f);
        public readonly Material Steel = Lit("Steel_Black", "#16171A", 0.65f, 0.7f);
        public readonly Material Chair = Lit("Chair_Pale", "#DCD8D1", 0.35f);
        public readonly Material ChairFrame = Lit("Chair_Frame_Pale", "#C9C5BE", 0.5f, 0.4f);
        public readonly Material LampMetal = Lit("Lamp_Matte_Black", "#2A2C30", 0.55f, 0.55f);
        public readonly Material LampInner = Lit("Lamp_Shade_Inner", "#F2EADF", 0.3f, 0f, Hex("#FFE6C8") * 0.9f);
        public readonly Material LampBulb = Lit("Lamp_Bulb", "#FFF0DC", 0.5f, 0f, Hex("#FFE2C0") * 1.8f);
        public readonly Material Paper = Lit("Notebook_Paper", "#ECE8DF", 0.15f);
        public readonly Material PenDark = Lit("Pen_Dark", "#2B2D31", 0.6f, 0.3f);
        public readonly Material Cctv = Lit("CCTV_Housing", "#3C3F44", 0.5f, 0.3f);
        public readonly Material Lens = Lit("CCTV_Lens", "#08090A", 0.9f);
        public readonly Material Led = Lit("CCTV_LED", "#FF1A1A", 0.5f, 0f, Hex("#FF2020") * 4f);
        public readonly Material EyeIris = Lit("Assessor_Eye_Iris", "#1A2023", 0.6f, 0f, Hex("#BFD8E0") * 0.18f);
        public readonly Material CeilingPuck = Lit("Ceiling_Puck", "#FFFFFF", 0.5f, 0f, Hex("#E9F1FF") * 8f);
        public readonly Material ObsWall = Lit("Observation_Wall", "#2B3036", 0.3f);
        public readonly Material Console = Lit("Observation_Console", "#16191C", 0.45f);
        public readonly Material Monitor = Lit("Observation_Monitor", "#080B0D", 0.85f, 0f, Hex("#3A5664") * 0.1f);
        public readonly Material Silhouette = Lit("Silhouette_Lit", "#0B0C0E", 0.35f);
    }

    static Material Asset(string name, string shader)
    {
        string path = $"{MatDir}/{name}.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (!mat)
        {
            mat = new Material(Shader.Find(shader));
            AssetDatabase.CreateAsset(mat, path);
        }
        return mat;
    }

    static Material Lit(string name, string hex, float smoothness, float metallic = 0f, Color? emission = null)
    {
        var mat = Asset(name, "Universal Render Pipeline/Lit");
        mat.SetColor("_BaseColor", Hex(hex));
        mat.SetFloat("_Smoothness", smoothness);
        mat.SetFloat("_Metallic", metallic);
        if (emission.HasValue)
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", emission.Value);
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }
        else
        {
            mat.DisableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", Color.black);
        }
        EditorUtility.SetDirty(mat);
        return mat;
    }

    // Transparent glass that keeps its specular reflections (lamp and room read faintly on it).
    static Material GlassMat()
    {
        var mat = Lit("Observation_Glass", "#0A0E12", 0.97f);
        mat.SetColor("_BaseColor", new Color(0.06f, 0.075f, 0.09f, 0.16f));
        mat.SetFloat("_Surface", 1f);
        mat.SetFloat("_Blend", 0f);
        mat.SetFloat("_BlendModePreserveSpecular", 1f);
        mat.SetFloat("_SrcBlend", (float)BlendMode.One);
        mat.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
        mat.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
        mat.SetFloat("_DstBlendAlpha", (float)BlendMode.OneMinusSrcAlpha);
        mat.SetFloat("_ZWrite", 0f);
        mat.SetOverrideTag("RenderType", "Transparent");
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
        // No punctual-light highlights (they bloom into blobs); environment reflections stay on.
        mat.SetFloat("_SpecularHighlights", 0f);
        mat.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
        mat.renderQueue = (int)RenderQueue.Transparent;
        EditorUtility.SetDirty(mat);
        return mat;
    }

    internal static Color Hex(string hex) => ColorUtility.TryParseHtmlString(hex, out var c) ? c : Color.magenta;

    // ---------- Geometry helpers ----------

    static Transform Group(Transform parent, string name)
    {
        var t = new GameObject(name).transform;
        t.SetParent(parent, false);
        return t;
    }

    static GameObject Box(string name, Transform parent, Vector3 pos, Vector3 scale, Material mat, Vector3 euler = default, bool collider = false)
        => Prim(PrimitiveType.Cube, name, parent, pos, scale, mat, euler, collider);

    static GameObject Prim(PrimitiveType type, string name, Transform parent, Vector3 pos, Vector3 scale, Material mat, Vector3 euler = default, bool collider = false)
    {
        var go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.SetPositionAndRotation(pos, Quaternion.Euler(euler));
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = mat;
        if (!collider) Object.DestroyImmediate(go.GetComponent<Collider>());
        return go;
    }

    // Thin bar between two points (cylinder or square tube).
    static GameObject Bar(string name, Transform parent, Vector3 a, Vector3 b, float thickness, Material mat, PrimitiveType type = PrimitiveType.Cylinder)
    {
        var go = Prim(type, name, parent, Vector3.zero, Vector3.one, mat);
        Place(go.transform, a, b, thickness, type);
        return go;
    }

    static void Place(Transform t, Vector3 a, Vector3 b, float thickness, PrimitiveType type)
    {
        var dir = b - a;
        t.SetPositionAndRotation((a + b) * 0.5f, Quaternion.FromToRotation(Vector3.up, dir));
        float length = type == PrimitiveType.Cylinder ? dir.magnitude * 0.5f : dir.magnitude;
        t.localScale = new Vector3(thickness, length, thickness);
    }

    // Disc facing -Z (towards the camera).
    static GameObject Disc(string name, Transform parent, Vector3 pos, float diameter, float depth, Material mat)
        => Prim(PrimitiveType.Cylinder, name, parent, pos, new Vector3(diameter, depth * 0.5f, diameter), mat, new Vector3(90f, 0f, 0f));

    static Transform Reuse(string name, Vector3 pos, Vector3 scale, Material mat, Vector3 euler = default, string rename = null)
    {
        var go = GameObject.Find(name) ?? (rename != null ? GameObject.Find(rename) : null);
        if (!go)
        {
            go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
        }
        if (rename != null) go.name = rename;
        go.transform.SetPositionAndRotation(pos, Quaternion.Euler(euler));
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = mat;
        return go.transform;
    }

    // ---------- Room ----------

    static void BuildArchitecture(Transform root, Mats m)
    {
        var arch = Group(root, "Architecture");

        Reuse("Chamber_Floor", new(0f, -0.15f, 0f), new(10f, 0.3f, 10f), m.Floor);
        Reuse("Chamber_Ceiling", new(0f, 3.15f, 0f), new(10f, 0.3f, 10f), m.Ceiling);
        Reuse("Chamber_Wall_Left", new(-4.85f, 1.5f, 0f), new(0.3f, 3f, 10f), m.Wall);
        Reuse("Chamber_Wall_Right", new(4.85f, 1.5f, 0f), new(0.3f, 3f, 10f), m.Wall);
        Reuse("Chamber_Wall_Entry", new(0f, 1.5f, -4.85f), new(10f, 3f, 0.3f), m.Wall);

        // Back wall split around the wide observation window.
        float openH = WindowTopY - SillY, midY = (SillY + WindowTopY) * 0.5f;
        float jambW = 5f - WindowHalfWidth;
        Reuse("Chamber_Wall_Observation", new(0f, SillY * 0.5f, 4.85f), new(10f, SillY, 0.3f), m.Wall);
        Box("Wall_Observation_Header", arch, new(0f, (WindowTopY + 3f) * 0.5f, 4.85f), new(10f, 3f - WindowTopY, 0.3f), m.Wall, collider: true);
        Box("Wall_Observation_Jamb_L", arch, new(-(WindowHalfWidth + jambW * 0.5f), midY, 4.85f), new(jambW, openH, 0.3f), m.Wall, collider: true);
        Box("Wall_Observation_Jamb_R", arch, new(WindowHalfWidth + jambW * 0.5f, midY, 4.85f), new(jambW, openH, 0.3f), m.Wall, collider: true);

        // Thin black frame: a face trim on the chamber side plus the reveal lining the opening.
        float w = WindowHalfWidth * 2f;
        Box("Window_Frame_Top", arch, new(0f, WindowTopY - 0.015f, 4.85f), new(w, 0.03f, 0.32f), m.Frame);
        Box("Window_Frame_Bottom", arch, new(0f, SillY + 0.015f, 4.85f), new(w, 0.03f, 0.32f), m.Frame);
        Box("Window_Frame_L", arch, new(-WindowHalfWidth + 0.015f, midY, 4.85f), new(0.03f, openH, 0.32f), m.Frame);
        Box("Window_Frame_R", arch, new(WindowHalfWidth - 0.015f, midY, 4.85f), new(0.03f, openH, 0.32f), m.Frame);
        Box("Window_Trim_Top", arch, new(0f, WindowTopY + 0.02f, 4.69f), new(w + 0.08f, 0.04f, 0.02f), m.Frame);
        Box("Window_Trim_L", arch, new(-WindowHalfWidth - 0.02f, midY, 4.69f), new(0.04f, openH + 0.08f, 0.02f), m.Frame);
        Box("Window_Trim_R", arch, new(WindowHalfWidth + 0.02f, midY, 4.69f), new(0.04f, openH + 0.08f, 0.02f), m.Frame);
        Box("Window_Sill_Ledge", arch, new(0f, SillY - 0.01f, 4.65f), new(w + 0.08f, 0.02f, 0.1f), m.Frame);
        // Glazing beads just in front of the glass so its boundary reads against both rooms.
        Box("Window_Bead_Top", arch, new(0f, WindowTopY - 0.035f, 4.835f), new(w - 0.06f, 0.012f, 0.012f), m.FrameEdge);
        Box("Window_Bead_Bottom", arch, new(0f, SillY + 0.035f, 4.835f), new(w - 0.06f, 0.012f, 0.012f), m.FrameEdge);
        Box("Window_Bead_L", arch, new(-WindowHalfWidth + 0.035f, midY, 4.835f), new(0.012f, openH - 0.06f, 0.012f), m.FrameEdge);
        Box("Window_Bead_R", arch, new(WindowHalfWidth - 0.035f, midY, 4.835f), new(0.012f, openH - 0.06f, 0.012f), m.FrameEdge);
        Reuse("Observation_Window_Placeholder", new(0f, midY, 4.85f), new(w - 0.03f, openH - 0.03f, 0.012f), m.Glass, rename: "Observation_Window_Glass");

        // Faint horizontal reveal along the partition, as in the reference wall panelling.
        Box("Partition_Reveal", arch, new(0f, 0.12f, 4.695f), new(10f, 0.012f, 0.01f), m.Recess);

        // Sealed door on the right wall in a dark recess; matching empty recess on the left.
        Reuse("Sealed_Door", new(4.68f, 1.1f, 3.0f), new(0.06f, 2.2f, 1.0f), m.Recess);
        Reuse("Door_Frame_Left", new(4.66f, 1.15f, 2.46f), new(0.12f, 2.3f, 0.08f), m.Frame);
        Reuse("Door_Frame_Right", new(4.66f, 1.15f, 3.54f), new(0.12f, 2.3f, 0.08f), m.Frame);
        Reuse("Door_Frame_Top", new(4.66f, 2.26f, 3.0f), new(0.12f, 0.08f, 1.16f), m.Frame);
        Box("Wall_Recess_Left", arch, new(-4.69f, 1.15f, 3.0f), new(0.02f, 2.3f, 1.16f), m.Recess);

        foreach (var p in new[] { new Vector3(-1.5f, 2.995f, 4.2f), new Vector3(1.5f, 2.995f, 4.2f), new Vector3(0f, 2.995f, 3.2f), new Vector3(0f, 2.995f, 1.6f) })
            Prim(PrimitiveType.Cylinder, "Ceiling_Puck", arch, p, new(0.12f, 0.005f, 0.12f), m.CeilingPuck);
    }

    static void BuildObservationRoom(Transform root, Mats m)
    {
        var obs = Group(root, "Observation_Room");
        float depth = ObsFar - ObsNear, midZ = (ObsNear + ObsFar) * 0.5f, width = ObsHalfWidth * 2f + 0.6f;
        Box("Obs_Floor", obs, new(0f, -0.15f, midZ), new(width, 0.3f, depth), m.Floor, collider: true);
        Box("Obs_Ceiling", obs, new(0f, ObsCeiling + 0.15f, midZ), new(width, 0.3f, depth), m.Ceiling);
        Box("Obs_Wall_Back", obs, new(0f, 1.5f, ObsFar + 0.15f), new(width, 3f, 0.3f), m.ObsWall);
        Box("Obs_Wall_Left", obs, new(-ObsHalfWidth - 0.15f, 1.5f, midZ), new(0.3f, 3f, depth), m.ObsWall);
        Box("Obs_Wall_Right", obs, new(ObsHalfWidth + 0.15f, 1.5f, midZ), new(0.3f, 3f, depth), m.ObsWall);

        // Tall dark doorways in the side walls near the glass (reference: dark recesses at both edges).
        foreach (float s in new[] { -1f, 1f })
        {
            float x = s * (ObsHalfWidth - 0.01f), z = 5.55f;
            Box("Obs_Doorway", obs, new(x, 1.1f, z), new(0.02f, 2.2f, 0.8f), m.Recess);
            Box("Obs_Doorway_Frame_Near", obs, new(x - s * 0.01f, 1.12f, z - 0.42f), new(0.04f, 2.24f, 0.04f), m.Frame);
            Box("Obs_Doorway_Frame_Far", obs, new(x - s * 0.01f, 1.12f, z + 0.42f), new(0.04f, 2.24f, 0.04f), m.Frame);
            Box("Obs_Doorway_Frame_Top", obs, new(x - s * 0.01f, 2.24f, z), new(0.04f, 0.04f, 0.88f), m.Frame);
        }

        // Low console between the glass and the observer, with dim blue-grey monitors on its left.
        Box("Obs_Console", obs, new(-0.25f, 0.55f, 5.3f), new(1.6f, 1.1f, 0.45f), m.Console);
        foreach (float x in new[] { -0.85f, -0.45f })
            Box("Obs_Monitor", obs, new(x, 1.24f, 5.42f), new(0.36f, 0.24f, 0.025f), m.Monitor, new(-10f, 0f, 0f));

        // Three rows of ceiling pucks, visible through the top of the glass.
        foreach (var p in new[] { new Vector3(-0.9f, 0f, 5.6f), new Vector3(0.9f, 0f, 5.6f), new Vector3(-0.85f, 0f, 5.95f), new Vector3(0.85f, 0f, 5.95f), new Vector3(-0.8f, 0f, 6.8f), new Vector3(0.8f, 0f, 6.8f) })
            Prim(PrimitiveType.Cylinder, "Obs_Ceiling_Puck", obs, new(p.x, ObsCeiling - 0.005f, p.z), new(0.11f, 0.005f, 0.11f), m.CeilingPuck);

        // Observer: dark but lit material so the rim light separates it from the wall (not a flat cutout).
        var figure = Group(obs, "Observer_Silhouette");
        var at = new Vector3(-0.12f, 0f, 5.75f);
        Prim(PrimitiveType.Capsule, "Torso", figure, at + new Vector3(0f, 1.2f, 0f), new(0.38f, 0.33f, 0.22f), m.Silhouette);
        Prim(PrimitiveType.Capsule, "Shoulders", figure, at + new Vector3(0f, 1.44f, 0f), new(0.14f, 0.24f, 0.14f), m.Silhouette, new(0f, 0f, 90f));
        Prim(PrimitiveType.Cylinder, "Neck", figure, at + new Vector3(0f, 1.56f, 0f), new(0.1f, 0.05f, 0.1f), m.Silhouette);
        Prim(PrimitiveType.Sphere, "Head", figure, at + new Vector3(0f, 1.69f, 0f), new(0.19f, 0.23f, 0.21f), m.Silhouette);
        foreach (float x in new[] { -0.1f, 0.1f })
            Prim(PrimitiveType.Capsule, "Leg", figure, at + new Vector3(x, 0.45f, 0f), new(0.15f, 0.46f, 0.15f), m.Silhouette);
        foreach (float x in new[] { -0.215f, 0.215f })
            Prim(PrimitiveType.Capsule, "Arm", figure, at + new Vector3(x, 1.16f, 0.02f), new(0.1f, 0.3f, 0.1f), m.Silhouette, new(0f, 0f, x > 0 ? 3f : -3f));
    }

    // ---------- Furniture and props ----------

    static void BuildFurniture(Transform root, Mats m)
    {
        // Desk: substantial pale top, thin black splayed legs (two per end, front and back).
        Reuse("Assessment_Desk_Top", new(0f, DeskTop - 0.02f, DeskZ), new(DeskHalfWidth * 2f, 0.04f, DeskHalfDepth * 2f), m.DeskTop);
        float legTopX = DeskHalfWidth - 0.1f, legFootX = DeskHalfWidth - 0.02f;
        float front = DeskZ - DeskHalfDepth + 0.08f, back = DeskZ + DeskHalfDepth - 0.08f;
        var legs = new (string name, Vector3 top, Vector3 foot)[]
        {
            ("Assessment_Desk_Leg_Left", new(-legTopX, DeskTop - 0.04f, front), new(-legFootX, 0f, front - 0.06f)),
            ("Assessment_Desk_Leg_Right", new(legTopX, DeskTop - 0.04f, front), new(legFootX, 0f, front - 0.06f)),
            ("Assessment_Desk_Leg_Left_Back", new(-legTopX, DeskTop - 0.04f, back), new(-legFootX, 0f, back + 0.06f)),
            ("Assessment_Desk_Leg_Right_Back", new(legTopX, DeskTop - 0.04f, back), new(legFootX, 0f, back + 0.06f)),
        };
        foreach (var (name, top, foot) in legs)
        {
            var leg = GameObject.Find(name);
            if (!leg)
            {
                leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                leg.name = name;
            }
            leg.GetComponent<Renderer>().sharedMaterial = m.Steel;
            Place(leg.transform, top, foot, 0.022f, PrimitiveType.Cube);
        }
        var deskFrame = Group(root, "Desk_Frame");
        Box("Desk_Apron_Front", deskFrame, new(0f, DeskTop - 0.055f, front), new(legTopX * 2f, 0.03f, 0.02f), m.Steel);
        Box("Desk_Apron_Back", deskFrame, new(0f, DeskTop - 0.055f, back), new(legTopX * 2f, 0.03f, 0.02f), m.Steel);

        // Chair: pale chair centred and partly tucked under the far side of the desk, facing the camera (reference).
        // Seat shell with a rounded front edge, curved backrest, rear legs that run up into the back posts.
        float seatZ = 4.05f, seatY = 0.46f, halfW = 0.215f, halfD = 0.205f;
        var chair = Group(root, "Chair_Frame");
        Reuse("Assessment_Chair_Seat", new(0f, seatY, seatZ + 0.01f), new(halfW * 2f, 0.035f, halfD * 2f - 0.02f), m.Chair, new(-3f, 0f, 0f));
        Prim(PrimitiveType.Capsule, "Chair_Seat_Front_Roll", chair, new(0f, seatY - 0.004f, seatZ - halfD + 0.012f), new(0.04f, halfW, 0.04f), m.Chair, new(0f, 0f, 90f));

        // Backrest: five panels on an arc (concave towards the sitter), slight recline, top at ~0.82 m.
        const float arcRadius = 0.55f, stepDeg = 10f, backY = 0.745f;
        float backZ = seatZ + halfD + 0.03f;
        for (int i = -2; i <= 2; i++)
        {
            float a = i * stepDeg * Mathf.Deg2Rad;
            var pos = new Vector3(arcRadius * Mathf.Sin(a), backY, backZ - arcRadius * (1f - Mathf.Cos(a)));
            var scale = new Vector3(0.1f, 0.15f, 0.018f);
            if (i == 0) Reuse("Assessment_Chair_Back", pos, scale, m.Chair, new(9f, 0f, 0f));
            else Box("Chair_Back_Panel", chair, pos, scale, m.Chair, new(9f, i * stepDeg, 0f));
        }

        foreach (float s in new[] { -1f, 1f })
        {
            float x = (halfW - 0.015f) * s, xFoot = (halfW + 0.005f) * s;
            float edgeX = arcRadius * Mathf.Sin(2f * stepDeg * Mathf.Deg2Rad) * s;
            float edgeZ = backZ - arcRadius * (1f - Mathf.Cos(2f * stepDeg * Mathf.Deg2Rad));
            // Rear leg continues up into the back post that carries the backrest edge.
            Bar("Chair_Rear_Leg", chair, new(xFoot, 0f, seatZ + halfD + 0.06f), new(x, seatY - 0.02f, seatZ + halfD - 0.01f), 0.018f, m.ChairFrame);
            Bar("Chair_Back_Post", chair, new(x, seatY - 0.02f, seatZ + halfD - 0.01f), new(edgeX * 0.98f, 0.8f, edgeZ + 0.03f), 0.016f, m.ChairFrame);
            Bar("Chair_Front_Leg", chair, new(x, seatY - 0.02f, seatZ - halfD + 0.03f), new(xFoot, 0f, seatZ - halfD - 0.01f), 0.018f, m.ChairFrame);
            Bar("Chair_Side_Rail", chair, new(x, seatY - 0.03f, seatZ - halfD + 0.03f), new(x, seatY - 0.03f, seatZ + halfD - 0.01f), 0.014f, m.ChairFrame);
            Bar("Chair_Low_Stretcher", chair, new(x * 1.01f, 0.16f, seatZ - halfD + 0.01f), new(x * 1.01f, 0.16f, seatZ + halfD + 0.03f), 0.01f, m.ChairFrame);
        }
        Bar("Chair_Front_Rail", chair, new(-halfW + 0.015f, seatY - 0.03f, seatZ - halfD + 0.03f), new(halfW - 0.015f, seatY - 0.03f, seatZ - halfD + 0.03f), 0.014f, m.ChairFrame);
    }

    static void BuildProps(Transform root, Mats m)
    {
        var props = Group(root, "Props");

        // Notebook (paper sheet), a small rolled document and a dark pen — left of centre, in the lamp pool.
        Box("Notebook", props, new(-0.3f, DeskTop + 0.004f, 3.85f), new(0.21f, 0.008f, 0.28f), m.Paper, new(0f, 8f, 0f));
        Bar("Document_Roll", props, new(-0.5f, DeskTop + 0.022f, 3.8f), new(-0.37f, DeskTop + 0.022f, 3.78f), 0.04f, m.Paper);
        Bar("Document_Roll_Cap", props, new(-0.51f, DeskTop + 0.022f, 3.8f), new(-0.49f, DeskTop + 0.022f, 3.8f), 0.041f, m.PenDark);
        Bar("Pen", props, new(-0.17f, DeskTop + 0.005f, 3.76f), new(-0.13f, DeskTop + 0.005f, 3.92f), 0.009f, m.PenDark);

        // Architect desk lamp, right side: weighted base, short post, two arms with joints, conical-reading shade.
        var lamp = Group(props, "Desk_Lamp");
        float lz = 3.95f;
        Vector3 basePos = new(0.6f, DeskTop, lz), post = new(0.62f, DeskTop + 0.1f, lz), elbow = new(0.76f, 1.22f, lz), pivot = new(0.34f, 1.33f, lz);
        Vector3 target = new(-0.08f, DeskTop, 3.85f);
        Prim(PrimitiveType.Cylinder, "Lamp_Base", lamp, basePos + new Vector3(0f, 0.012f, 0f), new(0.22f, 0.012f, 0.22f), m.LampMetal);
        Prim(PrimitiveType.Cylinder, "Lamp_Base_Collar", lamp, basePos + new Vector3(0.02f, 0.035f, 0f), new(0.045f, 0.012f, 0.045f), m.LampMetal);
        Bar("Lamp_Post", lamp, basePos + new Vector3(0.02f, 0.02f, 0f), post, 0.02f, m.LampMetal);
        Prim(PrimitiveType.Sphere, "Lamp_Joint_Base", lamp, post, Vector3.one * 0.032f, m.LampMetal);
        Bar("Lamp_Arm_Lower", lamp, post, elbow, 0.014f, m.LampMetal);
        Bar("Lamp_Arm_Lower_Rod", lamp, post + new Vector3(0.012f, 0f, 0.012f), elbow + new Vector3(0.012f, 0f, 0.012f), 0.006f, m.LampMetal);
        Prim(PrimitiveType.Cylinder, "Lamp_Joint_Elbow", lamp, elbow, new(0.04f, 0.015f, 0.04f), m.LampMetal, new(90f, 0f, 0f));
        Bar("Lamp_Arm_Upper", lamp, elbow, pivot, 0.012f, m.LampMetal);
        Bar("Lamp_Arm_Upper_Rod", lamp, elbow + new Vector3(0f, 0.012f, 0.012f), pivot + new Vector3(0f, 0.012f, 0.012f), 0.005f, m.LampMetal);
        Prim(PrimitiveType.Cylinder, "Lamp_Joint_Head", lamp, pivot, new(0.035f, 0.014f, 0.035f), m.LampMetal, new(90f, 0f, 0f));
        var aim = (target - pivot).normalized;
        // Shade: narrow neck into a wider mouth so it reads as a cone; lit inner liner faces the desk.
        Bar("Lamp_Shade_Neck", lamp, pivot, pivot + aim * 0.06f, 0.06f, m.LampMetal).GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
        Bar("Lamp_Shade", lamp, pivot + aim * 0.05f, pivot + aim * 0.17f, 0.13f, m.LampMetal).GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
        Bar("Lamp_Shade_Liner", lamp, pivot + aim * 0.168f, pivot + aim * 0.172f, 0.12f, m.LampInner).GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
        var bulb = Prim(PrimitiveType.Sphere, "Lamp_Bulb", lamp, pivot + aim * 0.165f, Vector3.one * 0.045f, m.LampBulb);
        bulb.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;

        // CCTV dome-style camera high in the upper-right, read through the top of the glass, restrained red LED.
        var cctv = Reuse("Surveillance_Camera_Body", new(1.15f, 2.27f, 5.35f), new(0.1f, 0.1f, 0.2f), m.Cctv);
        cctv.rotation = Quaternion.LookRotation(CameraPos - cctv.position);
        Bar("CCTV_Mount", props, new(1.15f, ObsCeiling, 5.4f), new(1.15f, 2.31f, 5.38f), 0.025f, m.Cctv);
        var forward = cctv.forward;
        Bar("CCTV_Hood", props, cctv.position + forward * 0.06f + cctv.up * 0.05f, cctv.position + forward * 0.14f + cctv.up * 0.05f, 0.11f, m.Cctv, PrimitiveType.Cube);
        Bar("CCTV_Lens", props, cctv.position + forward * 0.09f, cctv.position + forward * 0.11f, 0.065f, m.Lens);
        Prim(PrimitiveType.Sphere, "CCTV_LED", props, cctv.position + forward * 0.105f - cctv.up * 0.035f + cctv.right * 0.03f, Vector3.one * 0.014f, m.Led);

        // Assessor: small eye-like indicator set in the partition below the glass; kept very subtle.
        var eye = Reuse("Assessor_Eye_Indicator_Placeholder", new(0f, 0.9f, 4.695f), new(0.1f, 0.005f, 0.1f), m.Frame, new(90f, 0f, 0f), "Assessor_Eye_Indicator");
        var eyeCollider = eye.GetComponent<Collider>();
        if (eyeCollider) Object.DestroyImmediate(eyeCollider);
        if (eye.GetComponent<MeshFilter>().sharedMesh.name != "Cylinder")
            eye.GetComponent<MeshFilter>().sharedMesh = Resources.GetBuiltinResource<Mesh>("Cylinder.fbx");
        var iris = Disc("Assessor_Eye_Iris", props, new(0f, 0.9f, 4.69f), 0.05f, 0.004f, m.EyeIris);
        Disc("Assessor_Eye_Pupil", props, new(0f, 0.9f, 4.687f), 0.016f, 0.004f, m.Lens);
        var pulse = eye.gameObject.GetComponent<AssessorEyePulse>() ?? eye.gameObject.AddComponent<AssessorEyePulse>();
        var so = new SerializedObject(pulse);
        so.FindProperty("iris").objectReferenceValue = iris.GetComponent<Renderer>();
        so.FindProperty("glow").colorValue = Hex("#BFD8E0") * 0.18f;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // ---------- Lighting ----------

    static Light Spot(string name, Transform parent, Vector3 pos, Vector3 target, float kelvin, float intensity, float range, float angle, float inner, bool shadows)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.SetPositionAndRotation(pos, Quaternion.LookRotation(target - pos));
        var light = go.AddComponent<Light>();
        light.type = LightType.Spot;
        light.color = Mathf.CorrelatedColorTemperatureToRGB(kelvin);
        light.intensity = intensity;
        light.range = range;
        light.spotAngle = angle;
        light.innerSpotAngle = inner;
        light.shadows = shadows ? LightShadows.Soft : LightShadows.None;
        return light;
    }

    static Light Point(string name, Transform parent, Vector3 pos, Color color, float intensity, float range)
    {
        var light = new GameObject(name).AddComponent<Light>();
        light.transform.SetParent(parent, false);
        light.transform.position = pos;
        light.type = LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
        return light;
    }

    static void BuildLighting(Transform root)
    {
        var lights = Group(root, "Lighting");

        var sun = GameObject.Find("Directional Light");
        if (sun) sun.SetActive(false); // interior only; kept disabled rather than deleted

        // Cool, low ambient keeps shadowed surfaces readable without lifting the blacks to grey.
        RenderSettings.skybox = null;
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.06f, 0.066f, 0.078f);
        RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
        RenderSettings.reflectionIntensity = 0.6f;
        RenderSettings.fog = false;

        // Chamber: soft wash on the partition and frame, gentle cold fill on desk, chair and floor.
        // URP spot falloff is inverse-square, so ceiling-height lights need double-digit intensities.
        Spot("Chamber_Wall_Wash_L", lights, new(-1.5f, 2.95f, 4.2f), new(-1.5f, 0.3f, 4.7f), 6500f, 11f, 4f, 80f, 15f, false);
        Spot("Chamber_Wall_Wash_R", lights, new(1.5f, 2.95f, 4.2f), new(1.5f, 0.3f, 4.7f), 6500f, 11f, 4f, 80f, 15f, false);
        Spot("Chamber_Desk_Fill", lights, new(0f, 2.95f, 3.2f), new(0f, 0.75f, 3.9f), 6500f, 1.5f, 4f, 75f, 10f, true);
        Spot("Chamber_Front_Fill", lights, new(0f, 2.95f, 1.6f), new(0f, 0f, 2.8f), 6500f, 1.8f, 5f, 90f, 10f, false);
        // Soft vertical gradients on the wall sections either side of the glass (reference depth cue).
        Spot("Chamber_Jamb_Wash_L", lights, new(-2.9f, 2.95f, 4.25f), new(-2.9f, 0.5f, 4.7f), 6500f, 6f, 3.5f, 50f, 8f, false);
        Spot("Chamber_Jamb_Wash_R", lights, new(2.9f, 2.95f, 4.25f), new(2.9f, 0.5f, 4.7f), 6500f, 6f, 3.5f, 50f, 8f, false);
        Point("Chamber_Bounce", lights, new(0f, 1.6f, 2.2f), Hex("#8A97A6"), 0.6f, 5f);

        // The only warm source: a soft, wide desk-lamp pool with a long falloff.
        var bulb = GameObject.Find("Lamp_Bulb").transform;
        var deskTarget = new Vector3(-0.08f, DeskTop, 3.85f);
        var aim = (deskTarget - bulb.position).normalized;
        Spot("Desk_Lamp_Light", lights, bulb.position + aim * 0.03f, deskTarget, 3800f, 2.4f, 2.8f, 115f, 5f, true);
        Point("Desk_Lamp_Glow", lights, bulb.position + aim * 0.12f, Mathf.CorrelatedColorTemperatureToRGB(3800f), 0.25f, 1.2f);

        // Observation room: cones on the back wall, grazing light on the side walls, blue-grey depth fill.
        foreach (float s in new[] { -1f, 1f })
        {
            Spot("Obs_Back_Cone", lights, new(s * 0.8f, ObsCeiling - 0.05f, 6.8f), new(s * 0.8f, 0.8f, ObsFar), 6000f, 7f, 3.5f, 60f, 12f, false);
            Spot("Obs_Side_Graze", lights, new(s * 1.85f, ObsCeiling - 0.05f, 5.9f), new(s * ObsHalfWidth, 0.6f, 6.1f), 6000f, 10f, 3f, 55f, 10f, false);
            Spot("Obs_Near_Down", lights, new(s * 0.9f, ObsCeiling - 0.05f, 5.6f), new(s * 0.9f, 0f, 5.7f), 6000f, 2f, 3f, 60f, 10f, false);
        }
        Point("Obs_Depth_Fill", lights, new(0f, 1.7f, 6.4f), Hex("#5F7A88"), 1.1f, 3.5f);
        Point("Obs_Silhouette_Rim", lights, new(-0.12f, 1.55f, 6.3f), Hex("#9DB4C0"), 2.2f, 1.4f);
        Point("Obs_Monitor_Glow", lights, new(-0.65f, 1.3f, 5.3f), Hex("#4F6E7C"), 0.25f, 0.7f);
        // Small cold key so the CCTV housing reads against the dark ceiling.
        Point("CCTV_Key", lights, new(0.95f, 2.05f, 5.0f), Hex("#C9D3DC"), 0.25f, 0.6f);

        var probe = new GameObject("Chamber_Reflection_Probe").AddComponent<ReflectionProbe>();
        probe.transform.SetParent(lights, false);
        probe.transform.position = new Vector3(0f, 1.2f, 3.0f);
        probe.size = new Vector3(9.4f, 3f, 9.4f);
        probe.center = new Vector3(0f, 0.3f, -3.0f);
        probe.boxProjection = true;
        probe.mode = ReflectionProbeMode.Realtime;
        probe.refreshMode = ReflectionProbeRefreshMode.OnAwake;
        probe.resolution = 256;
        probe.intensity = 1.6f; // lifts the faint room reflections on the glass and polished floor
        probe.RenderProbe();

        BuildVolume();
    }

    static void BuildVolume()
    {
        var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath);
        if (!profile)
        {
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, ProfilePath);
        }
        foreach (var c in profile.components.ToArray())
        {
            profile.Remove(c.GetType());
            Object.DestroyImmediate(c, true);
        }

        var tone = profile.Add<Tonemapping>(true);
        tone.mode.Override(TonemappingMode.ACES);
        var bloom = profile.Add<Bloom>(true);
        bloom.threshold.Override(1.1f);
        bloom.intensity.Override(0.35f);
        bloom.scatter.Override(0.7f);
        var vignette = profile.Add<Vignette>(true);
        vignette.intensity.Override(0.28f);
        vignette.smoothness.Override(0.45f);
        var color = profile.Add<ColorAdjustments>(true);
        color.postExposure.Override(0.35f);
        color.contrast.Override(8f);
        color.saturation.Override(-18f);
        var lift = profile.Add<LiftGammaGain>(true);
        lift.lift.Override(new Vector4(1f, 1f, 1f, 0.004f)); // keeps darks off pure black
        // Gentle far blur on the observation room only, for foreground/background separation.
        var dof = profile.Add<DepthOfField>(true);
        dof.mode.Override(DepthOfFieldMode.Gaussian);
        dof.gaussianStart.Override(5.2f);
        dof.gaussianEnd.Override(9f);
        dof.gaussianMaxRadius.Override(0.8f);
        dof.highQualitySampling.Override(true);
        var balance = profile.Add<WhiteBalance>(true);
        balance.temperature.Override(-8f);
        var grain = profile.Add<FilmGrain>(true);
        grain.type.Override(FilmGrainLookup.Thin1);
        grain.intensity.Override(0.12f);
        foreach (var c in profile.components)
            if (!AssetDatabase.Contains(c)) AssetDatabase.AddObjectToAsset(c, profile);
        EditorUtility.SetDirty(profile);

        var volume = GameObject.Find("Global Volume").GetComponent<Volume>();
        volume.isGlobal = true;
        volume.sharedProfile = profile;
    }

    static void BuildCamera()
    {
        var cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        cam.transform.SetPositionAndRotation(CameraPos, Quaternion.identity);
        cam.fieldOfView = CameraFov;
        cam.nearClipPlane = 0.05f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        var data = cam.GetUniversalAdditionalCameraData();
        data.renderPostProcessing = true;
        data.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
        data.antialiasingQuality = AntialiasingQuality.High;
    }

    // ---------- UI ----------

    static readonly Color PanelFill = new(0.035f, 0.047f, 0.063f, 0.78f);
    internal static readonly Color PanelBorder = new(1f, 1f, 1f, 0.14f);
    internal static readonly Color LabelGrey = Hex("#9AA3AB");
    internal static readonly Color TextWhite = Hex("#E8EAEC");

    internal static RectTransform Rect(string name, Transform parent, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        var rt = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return rt;
    }

    static Image Panel(RectTransform rt, Color fill) => Panel(rt, fill, out _);

    // Translucent fill plus four 1px edge strips (an Outline effect would tint the fill itself).
    static Image Panel(RectTransform rt, Color fill, out Graphic[] edges)
    {
        var img = rt.gameObject.AddComponent<Image>();
        img.color = fill;
        edges = new Graphic[4];
        var specs = new (string name, Vector2 min, Vector2 max, Vector2 size)[]
        {
            ("Border_Top", new(0f, 1f), new(1f, 1f), new(0f, 1f)),
            ("Border_Bottom", new(0f, 0f), new(1f, 0f), new(0f, 1f)),
            ("Border_Left", new(0f, 0f), new(0f, 1f), new(1f, 0f)),
            ("Border_Right", new(1f, 0f), new(1f, 1f), new(1f, 0f)),
        };
        for (int i = 0; i < specs.Length; i++)
        {
            var edge = new GameObject(specs[i].name, typeof(RectTransform), typeof(LayoutElement)).GetComponent<RectTransform>();
            edge.SetParent(rt, false);
            edge.GetComponent<LayoutElement>().ignoreLayout = true;
            edge.anchorMin = specs[i].min;
            edge.anchorMax = specs[i].max;
            edge.pivot = specs[i].min;
            edge.anchoredPosition = Vector2.zero;
            edge.sizeDelta = specs[i].size;
            var line = edge.gameObject.AddComponent<Image>();
            line.color = PanelBorder;
            line.raycastTarget = false;
            edges[i] = line;
        }
        return img;
    }

    static TextMeshProUGUI Text(string name, Transform parent, string text, float size, Color color, Vector2 pos, Vector2 boxSize, float spacing = 0f, TextAlignmentOptions align = TextAlignmentOptions.TopLeft)
    {
        var rt = Rect(name, parent, new(0f, 1f), new(0f, 1f), pos, boxSize);
        var tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.characterSpacing = spacing;
        tmp.alignment = align;
        tmp.raycastTarget = false;
        return tmp;
    }

    static void BuildUi()
    {
        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

        var canvasGo = new GameObject(UiRoot, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        var canvas = canvasGo.transform;

        // Top-left subject/status panel. Name is intentionally absent (Scene 1).
        var subject = Rect("Subject_Panel", canvas, new(0f, 1f), new(0f, 1f), new(36f, -56f), new(400f, 82f));
        Panel(subject, new Color(0.035f, 0.047f, 0.063f, 0.45f));
        Text("Subject_Label", subject, "SUBJECT", 13f, LabelGrey, new(26f, -17f), new(120f, 20f), 14f);
        Text("Subject_Value", subject, "A-4172", 24f, TextWhite, new(26f, -38f), new(130f, 32f), 2f);
        var divider = Rect("Divider", subject, new(0f, 1f), new(0f, 1f), new(162f, -16f), new(1f, 50f));
        divider.gameObject.AddComponent<Image>().color = PanelBorder;
        Text("Status_Label", subject, "STATUS", 13f, LabelGrey, new(188f, -17f), new(200f, 20f), 14f);
        Text("Status_Value", subject, "ASSESSMENT PENDING", 14f, Hex("#D0D4D8"), new(188f, -45f), new(210f, 22f), 3f);

        // Bottom-left translucent dialogue panel.
        var dialogue = Rect("Dialogue_Panel", canvas, new(0f, 0f), new(0f, 0f), new(36f, 48f), new(1338f, 162f));
        Panel(dialogue, PanelFill);
        Text("Speaker_Label", dialogue, "ASSESSOR", 15f, LabelGrey, new(72f, -28f), new(300f, 22f), 16f);
        var body = Text("Dialogue_Body", dialogue, string.Empty, 27f, TextWhite, new(72f, -58f), new(1180f, 90f), 0.5f);
        body.lineSpacing = 14f;
        Text("Continue_Indicator", dialogue, "▼", 14f, LabelGrey, new(1300f, -134f), new(20f, 20f));

        // Right-hand stacked responses.
        var choices = Rect("Choices_Panel", canvas, new(1f, 0f), new(1f, 0f), new(-32f, 48f), new(514f, 0f));
        Panel(choices, new Color(0.035f, 0.047f, 0.063f, 0.62f));
        var layout = choices.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 22, 22);
        layout.spacing = 8f;
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandHeight = false;
        choices.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var buttons = new ChoiceButton[4];
        for (int i = 0; i < buttons.Length; i++)
        {
            var b = Rect($"Choice_{i + 1}", choices, new(0f, 1f), new(0f, 1f), Vector2.zero, new(466f, 54f));
            b.gameObject.AddComponent<LayoutElement>().preferredHeight = 54f;
            var bg = Panel(b, new Color(0.039f, 0.051f, 0.067f, 0.55f), out var edges);
            var button = b.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.targetGraphic = bg;
            var label = Text("Label", b, "Response", 21f, TextWhite, new(28f, -15f), new(380f, 28f), 0.5f);
            var chevron = Text("Chevron", b, "›", 30f, TextWhite, new(430f, -8f), new(20f, 36f));
            var choice = b.gameObject.AddComponent<ChoiceButton>();
            choice.background = bg;
            choice.border = edges;
            choice.label = label;
            choice.chevron = chevron;
            buttons[i] = choice;
        }

        // Full-screen black for the opening fade from darkness.
        var fade = Rect("Fade", canvas, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
        fade.anchorMin = Vector2.zero;
        fade.anchorMax = Vector2.one;
        fade.gameObject.AddComponent<Image>().color = Color.black;
        var fadeGroup = fade.gameObject.AddComponent<CanvasGroup>();
        fadeGroup.alpha = 0f; // visible in the editor; the controller starts it at black in play mode

        var director = canvasGo.AddComponent<Scene01Dialogue>();
        var so = new SerializedObject(director);
        so.FindProperty("fade").objectReferenceValue = fadeGroup;
        so.FindProperty("body").objectReferenceValue = body;
        var arr = so.FindProperty("buttons");
        arr.arraySize = buttons.Length;
        for (int i = 0; i < buttons.Length; i++) arr.GetArrayElementAtIndex(i).objectReferenceValue = buttons[i];
        so.ApplyModifiedPropertiesWithoutUndo();

        // Editor preview of the opening state so the Game View shows the composed frame.
        body.text = "Consciousness recovery successful.\nWelcome to Eterna Continuity Processing.";
        string[] preview = { "Where am I?", "Who are you?", "What is this place?" };
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].gameObject.SetActive(i < preview.Length);
            if (i < preview.Length) buttons[i].label.text = preview[i];
            buttons[i].SetHighlighted(i == 0);
        }
    }
}
