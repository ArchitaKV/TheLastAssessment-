using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Thin-bordered response button. Cyan is reserved for the selected choice only.
public class ChoiceButton : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler
{
    public Image background;
    public Graphic[] border;
    public TMP_Text label;
    public TMP_Text chevron;

    public static readonly Color Cyan = new(0.247f, 0.722f, 0.910f, 1f);
    static readonly Color IdleBorder = new(1f, 1f, 1f, 0.15f);
    static readonly Color IdleFill = new(0.039f, 0.051f, 0.067f, 0.55f);
    static readonly Color SelectedFill = new(0.055f, 0.141f, 0.2f, 0.85f);
    static readonly Color IdleText = new(0.72f, 0.75f, 0.77f, 1f);
    static readonly Color SelectedText = new(0.91f, 0.92f, 0.93f, 1f);

    void OnEnable() => SetHighlighted(EventSystem.current && EventSystem.current.currentSelectedGameObject == gameObject);

    public void OnSelect(BaseEventData _) => SetHighlighted(true);

    public void OnDeselect(BaseEventData _) => SetHighlighted(false);

    // Hover moves keyboard/controller selection too, so only one button is ever highlighted.
    public void OnPointerEnter(PointerEventData _) => EventSystem.current.SetSelectedGameObject(gameObject);

    public void SetHighlighted(bool selected)
    {
        background.color = selected ? SelectedFill : IdleFill;
        foreach (var edge in border) edge.color = selected ? Cyan : IdleBorder;
        label.color = selected ? SelectedText : IdleText;
        chevron.color = selected ? Cyan : IdleText;
    }
}
