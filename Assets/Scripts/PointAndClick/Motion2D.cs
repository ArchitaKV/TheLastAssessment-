using System.Collections;
using UnityEngine;

// Small presentation motions shared by the 2D panels. Cosmetic only: callers keep input enabled throughout.
public static class Motion2D
{
    public static bool Instant; // QA screenshots (-shots) capture panels at rest

    // Fade in while rising a few pixels into place.
    public static IEnumerator Appear(CanvasGroup group, RectTransform rt, Vector2 home, float seconds = 0.25f, float rise = 14f)
    {
        for (float t = 0f; !Instant && t < seconds; t += Time.unscaledDeltaTime)
        {
            float e = Mathf.SmoothStep(0f, 1f, t / seconds);
            group.alpha = e;
            rt.anchoredPosition = home + Vector2.down * (rise * (1f - e));
            yield return null;
        }
        group.alpha = 1f;
        rt.anchoredPosition = home;
    }

    // Eases a uniform scale (camera push) to 1; ends exactly at 1 so hotspots line up with the art.
    public static IEnumerator Push(Transform t, float from, float seconds)
    {
        for (float s = 0f; !Instant && s < seconds; s += Time.deltaTime)
        {
            t.localScale = Vector3.one * Mathf.Lerp(from, 1f, Mathf.SmoothStep(0f, 1f, s / seconds));
            yield return null;
        }
        t.localScale = Vector3.one;
    }
}
