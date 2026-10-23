using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Invisible clickable area over the artwork. A thin cyan outline appears on hover or keyboard focus.
// Hotspots without approved inspection text are built but left disabled (see VerticalSlice2D_Plan §4).
[RequireComponent(typeof(Button))]
public class Hotspot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    public enum Action { Disabled, Advance }

    public Action action = Action.Disabled;
    public Graphic[] outline;

    public event System.Action Clicked;

    public bool IsAdvance => action == Action.Advance;

    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() => Clicked?.Invoke());
        SetArmed(false);
    }

    // Armed hotspots take clicks and focus; everything else on the stage ignores the pointer.
    public void SetArmed(bool armed)
    {
        bool on = armed && action != Action.Disabled;
        GetComponent<Button>().interactable = on;
        GetComponent<Image>().raycastTarget = on;
        if (!on) ShowOutline(false);
    }

    public void OnPointerEnter(PointerEventData _) { if (GetComponent<Button>().interactable) EventSystem.current.SetSelectedGameObject(gameObject); }
    public void OnPointerExit(PointerEventData _) { if (EventSystem.current.currentSelectedGameObject == gameObject) EventSystem.current.SetSelectedGameObject(null); }
    public void OnSelect(BaseEventData _) => ShowOutline(GetComponent<Button>().interactable);
    public void OnDeselect(BaseEventData _) => ShowOutline(false);

    void ShowOutline(bool show)
    {
        foreach (var edge in outline) edge.enabled = show;
    }
}
