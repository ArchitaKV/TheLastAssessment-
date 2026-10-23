using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Invisible clickable area over the artwork. A thin cyan outline appears on hover or keyboard focus.
// Advance: armed only while the Script awaits it (e.g. the tablet). Visual: an object in the art with no
// approved text, so a click only pulses the outline (no story effect); live while its view is idle.
// Hotspots without approved inspection text are built but left disabled (see VerticalSlice2D_Plan §4).
[RequireComponent(typeof(Button))]
public class Hotspot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    public enum Action { Disabled, Advance, Visual }

    public Action action = Action.Disabled;
    public Graphic[] outline;

    public event System.Action Clicked;

    public bool IsAdvance => action == Action.Advance;
    public bool IsVisual => action == Action.Visual;

    // The armed hotspot under the pointer; a mouse press there belongs to it, not to the dialogue.
    public static Hotspot Hovered { get; private set; }

    static Texture2D cursor;
    bool pulsing;

    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
        if (IsVisual) GetComponent<Button>().navigation = new Navigation { mode = Navigation.Mode.None }; // mouse-only extra
        SetArmed(false);
    }

    void OnClick()
    {
        if (IsVisual) { StoryDirector2D.Trace($"LOOK {name}"); if (!pulsing) StartCoroutine(Pulse()); }
        else Clicked?.Invoke();
    }

    // Armed hotspots take clicks and focus; everything else on the stage ignores the pointer.
    public void SetArmed(bool armed)
    {
        bool on = armed && action != Action.Disabled;
        GetComponent<Button>().interactable = on;
        GetComponent<Image>().raycastTarget = on;
        if (!on) { ShowOutline(false); Leave(); }
    }

    public void OnPointerEnter(PointerEventData _)
    {
        if (!GetComponent<Button>().interactable) return;
        Hovered = this;
        Cursor.SetCursor(RingCursor(), new Vector2(16f, 16f), CursorMode.Auto);
        // Visual hotspots highlight without taking the selection, so Enter keeps advancing the dialogue.
        if (IsVisual) ShowOutline(true, HoverAlpha);
        else EventSystem.current.SetSelectedGameObject(gameObject);
    }

    public void OnPointerExit(PointerEventData _)
    {
        if (IsVisual) ShowOutline(false);
        else if (EventSystem.current.currentSelectedGameObject == gameObject) EventSystem.current.SetSelectedGameObject(null);
        Leave();
    }

    public void OnSelect(BaseEventData _) => ShowOutline(GetComponent<Button>().interactable);
    public void OnDeselect(BaseEventData _) => ShowOutline(false);

    void Leave()
    {
        if (Hovered != this) return;
        Hovered = null;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    void OnDisable() => Leave();

    const float HoverAlpha = 0.85f;

    void ShowOutline(bool show, float alpha = 1f)
    {
        foreach (var edge in outline)
        {
            edge.enabled = show;
            var c = edge.color; c.a = alpha; edge.color = c;
        }
        // Visual hotspots also get a faint wash, so the area reads on light art where a thin line alone is lost.
        if (IsVisual) GetComponent<Image>().color = show ? new Color(0.45f, 0.85f, 0.95f, 0.045f * alpha / HoverAlpha) : Color.clear;
    }

    // Selection feedback: the outline brightens and opens out by a few pixels, then settles back.
    IEnumerator Pulse()
    {
        pulsing = true;
        var rt = (RectTransform)outline[0].transform.parent;
        Vector2 pos = rt.anchoredPosition, size = rt.sizeDelta;
        for (float t = 0f; t < 0.35f; t += Time.unscaledDeltaTime)
        {
            float k = Mathf.Sin(t / 0.35f * Mathf.PI); // 0 → 1 → 0
            rt.anchoredPosition = pos + new Vector2(-8f, 8f) * k;
            rt.sizeDelta = size + new Vector2(16f, 16f) * k;
            ShowOutline(true, Mathf.Lerp(HoverAlpha, 1.6f, k));
            yield return null;
        }
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        ShowOutline(Hovered == this, HoverAlpha);
        pulsing = false;
    }

    // A small hollow ring, drawn once at runtime (no external cursor asset).
    static Texture2D RingCursor()
    {
        if (cursor) return cursor;
        cursor = new Texture2D(32, 32, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
        for (int y = 0; y < 32; y++)
            for (int x = 0; x < 32; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(16f, 16f));
                float ring = Mathf.Clamp01(1.5f - Mathf.Abs(d - 10f));  // 3 px white ring
                float edge = Mathf.Clamp01(1.5f - Mathf.Abs(d - 12.5f)) + Mathf.Clamp01(1.5f - Mathf.Abs(d - 7.5f)); // dark rims
                cursor.SetPixel(x, y, ring > 0f ? new Color(0.8f, 1f, 1f, ring) : new Color(0f, 0f, 0f, Mathf.Clamp01(edge) * 0.6f));
            }
        cursor.Apply();
        return cursor;
    }
}
