using UnityEngine;
using UnityEngine.UI;

// Audio waveform drawn as bars (Figma p6 "AUDIO / SIGNAL HELD"). No audio assets exist yet, so
// playback is visual only. Cut freezes the bars mid-signal, as the Script's fragment cuts off.
public class Waveform : MonoBehaviour
{
    public RectTransform[] bars;
    public Graphic[] tints;
    public float maxHeight = 80f;

    enum Mode { Idle, Play, Faint, Cut }
    Mode mode;
    float seed;

    void OnEnable() { seed = Random.value * 100f; Set(Mode.Idle); }

    public void Play() => Set(Mode.Play);
    public void Faint() => Set(Mode.Faint);
    public void Cut() => Set(Mode.Cut);
    public void Stop() => Set(Mode.Idle);

    void Set(Mode m)
    {
        mode = m;
        float alpha = m == Mode.Faint ? 0.45f : m == Mode.Cut ? 0.35f : 1f;
        foreach (var g in tints) { var c = g.color; c.a = alpha; g.color = c; }
        if (m == Mode.Idle) foreach (var b in bars) b.sizeDelta = new Vector2(b.sizeDelta.x, 2f);
    }

    void Update()
    {
        if (mode != Mode.Play && mode != Mode.Faint) return;
        float amp = mode == Mode.Faint ? 0.4f : 1f;
        for (int i = 0; i < bars.Length; i++)
        {
            float n = Mathf.PerlinNoise(seed + i * 0.35f, Time.time * 3.2f);
            bars[i].sizeDelta = new Vector2(bars[i].sizeDelta.x, Mathf.Max(2f, n * n * maxHeight * amp * 1.6f));
        }
    }
}
