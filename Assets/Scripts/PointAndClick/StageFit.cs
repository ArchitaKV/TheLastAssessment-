using UnityEngine;

// Keeps the 1920×1080 stage (background, hotspots, Figma-positioned UI) at 16:9 inside any window:
// uniform scale plus letterbox, so artwork is never stretched or cropped and hotspots stay aligned.
[ExecuteAlways, RequireComponent(typeof(RectTransform))]
public class StageFit : MonoBehaviour
{
    public static readonly Vector2 Size = new(1920f, 1080f);

    void OnEnable() => Fit();
    void OnRectTransformDimensionsChange() => Fit();
    void Update() => Fit(); // the parent canvas can resize without notifying children

    void Fit()
    {
        var rt = (RectTransform)transform;
        var parent = rt.parent as RectTransform;
        if (!parent) return;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Size;
        var area = parent.rect.size;
        float scale = Mathf.Min(area.x / Size.x, area.y / Size.y);
        if (scale > 0f && !Mathf.Approximately(rt.localScale.x, scale)) rt.localScale = new Vector3(scale, scale, 1f);
    }
}
