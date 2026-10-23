using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Scripted visual events for the assessment (captions, valuation display, waveform, employment
// summary, office reconstruction). Purely presentational: no story text, no state decisions.
public class AssessmentPresentation : MonoBehaviour
{
    [Header("Caption")]
    [SerializeField] CanvasGroup captionGroup;
    [SerializeField] TMP_Text caption;

    [Header("Valuation (Q3 onward)")]
    [SerializeField] CanvasGroup cornerValuation;
    [SerializeField] Graphic cornerHighlight;
    [SerializeField] CanvasGroup secondFigure;

    [Header("Waveform (Q4, Q6)")]
    [SerializeField] CanvasGroup waveformGroup;
    [SerializeField] RectTransform[] waveformBars;
    [SerializeField] float waveformMaxHeight = 64f;

    [Header("Employment summary (Q5)")]
    [SerializeField] CanvasGroup employmentSummary;

    [Header("Office reconstruction (Q7)")]
    [SerializeField] Renderer reconstructionDisplay;
    [SerializeField] Camera reconstructionCamera;
    [SerializeField] float reconstructionFadeSeconds = 1.2f;

    static readonly HashSet<string> Visuals = new()
    {
        "valuation_to_corner", "second_figure_flicker", "valuation_highlight",
        "waveform_show", "waveform_play", "waveform_play_faint", "waveform_cut", "waveform_hide",
        "employment_summary_show", "employment_summary_hide",
        "office_show", "office_hide",
    };

    enum Wave { Hidden, Idle, Playing, Faint }

    Wave wave;
    Material displayMaterial;
    RectTransform captionPanel;
    Vector2 captionHome; // anchored position: independent of canvas size (which is not final in Awake)
    Coroutine reconstructionFade;
    static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
    static readonly int BaseMap = Shader.PropertyToID("_BaseMap");

    public bool IsKnown(string visual) => Visuals.Contains(visual);

    void Awake()
    {
        captionPanel = (RectTransform)caption.transform.parent;
        captionHome = captionPanel.anchoredPosition;
        var rt = new RenderTexture(1620, 400, 24) { antiAliasing = 2, name = "Reconstruction_RT" };
        reconstructionCamera.targetTexture = rt;
        displayMaterial = reconstructionDisplay.material; // runtime instance
        displayMaterial.SetTexture(BaseMap, rt);
        SetDisplayAlpha(0f);
        reconstructionDisplay.gameObject.SetActive(false);
        reconstructionCamera.enabled = false;
    }

    // Persistent visuals that survive Continue (driven by recorded flags).
    public void Restore(AssessmentState state)
    {
        cornerValuation.alpha = state.HasFlag(AssessmentFlags.ValuationInCorner) ? 1f : 0f;
    }

    // Transient visuals belong to a single question; clear them when a new one starts.
    public void ResetForNode()
    {
        StopAllCoroutines();
        reconstructionFade = null;
        SetCaption(null);
        captionPanel.anchoredPosition = captionHome;
        captionPanel.localScale = Vector3.one;
        secondFigure.alpha = 0f;
        cornerHighlight.color = WithAlpha(cornerHighlight.color, 0f);
        SetWave(Wave.Hidden);
        employmentSummary.alpha = 0f;
        HideReconstructionNow();
    }

    public void SetCaption(string text)
    {
        caption.text = text ?? string.Empty;
        captionGroup.alpha = string.IsNullOrEmpty(text) ? 0f : 1f;
    }

    public void Play(string visual)
    {
        switch (visual)
        {
            case "valuation_to_corner": StartCoroutine(ValuationToCorner()); break;
            case "second_figure_flicker": StartCoroutine(SecondFigureFlicker()); break;
            case "valuation_highlight": StartCoroutine(ValuationHighlight()); break;
            case "waveform_show": SetWave(Wave.Idle); break;
            case "waveform_play": SetWave(Wave.Playing); break;
            case "waveform_play_faint": SetWave(Wave.Faint); break;
            case "waveform_cut": SetWave(Wave.Idle); break;
            case "waveform_hide": SetWave(Wave.Hidden); break;
            case "employment_summary_show": StartCoroutine(FadeGroup(employmentSummary, 1f, 0.4f)); break;
            case "employment_summary_hide": StartCoroutine(FadeGroup(employmentSummary, 0f, 0.4f)); break;
            case "office_show": ShowReconstruction(true); break;
            case "office_hide": ShowReconstruction(false); break;
            default: Debug.LogError($"AssessmentPresentation: unknown visual '{visual}'"); break;
        }
    }

    // ---------- Valuation ----------

    // "The number shrinks into the corner."
    IEnumerator ValuationToCorner()
    {
        Vector3 from = captionPanel.position, to = cornerValuation.transform.position;
        for (float t = 0f; t < 0.8f; t += Time.deltaTime)
        {
            float k = Mathf.SmoothStep(0f, 1f, t / 0.8f);
            captionPanel.position = Vector3.Lerp(from, to, k);
            captionPanel.localScale = Vector3.one * Mathf.Lerp(1f, 0.4f, k);
            captionGroup.alpha = 1f - k * k;
            cornerValuation.alpha = k;
            yield return null;
        }
        SetCaption(null);
        captionPanel.anchoredPosition = captionHome;
        captionPanel.localScale = Vector3.one;
        cornerValuation.alpha = 1f;
    }

    // "For a brief moment, another figure appears beneath it before vanishing."
    IEnumerator SecondFigureFlicker()
    {
        yield return new WaitForSeconds(1.0f);
        foreach (var (on, seconds) in new[] { (0.85f, 0.12f), (0f, 0.08f), (0.85f, 0.35f), (0.3f, 0.06f), (0f, 0f) })
        {
            secondFigure.alpha = on;
            if (seconds > 0f) yield return new WaitForSeconds(seconds);
        }
    }

    IEnumerator ValuationHighlight()
    {
        for (float t = 0f; t < 1.6f; t += Time.deltaTime)
        {
            cornerHighlight.color = WithAlpha(cornerHighlight.color, 0.12f * Mathf.Sin(t / 1.6f * Mathf.PI));
            yield return null;
        }
        cornerHighlight.color = WithAlpha(cornerHighlight.color, 0f);
    }

    // ---------- Waveform ----------

    void SetWave(Wave mode)
    {
        wave = mode;
        waveformGroup.alpha = mode switch { Wave.Hidden => 0f, Wave.Faint => 0.55f, _ => 1f };
        if (mode is Wave.Idle or Wave.Hidden) SetBars(_ => 3f);
    }

    void Update()
    {
        if (wave is not (Wave.Playing or Wave.Faint)) return;
        float amplitude = wave == Wave.Faint ? 0.35f : 1f, time = Time.time * 7f;
        SetBars(i =>
        {
            float envelope = Mathf.Sin((i + 0.5f) / waveformBars.Length * Mathf.PI);
            return 3f + waveformMaxHeight * amplitude * envelope * Mathf.PerlinNoise(time, i * 0.37f);
        });
    }

    void SetBars(System.Func<int, float> height)
    {
        for (int i = 0; i < waveformBars.Length; i++)
            waveformBars[i].sizeDelta = new Vector2(waveformBars[i].sizeDelta.x, height(i));
    }

    // ---------- Reconstruction ----------

    void ShowReconstruction(bool show)
    {
        if (reconstructionFade != null) StopCoroutine(reconstructionFade);
        reconstructionFade = StartCoroutine(FadeReconstruction(show));
    }

    IEnumerator FadeReconstruction(bool show)
    {
        if (show)
        {
            reconstructionCamera.enabled = true;
            reconstructionDisplay.gameObject.SetActive(true);
        }
        float from = displayMaterial.GetColor(BaseColor).a, to = show ? 1f : 0f;
        for (float t = 0f; t < reconstructionFadeSeconds; t += Time.deltaTime)
        {
            SetDisplayAlpha(Mathf.Lerp(from, to, t / reconstructionFadeSeconds));
            yield return null;
        }
        SetDisplayAlpha(to);
        if (!show) HideReconstructionNow();
        reconstructionFade = null;
    }

    void HideReconstructionNow()
    {
        SetDisplayAlpha(0f);
        reconstructionDisplay.gameObject.SetActive(false);
        reconstructionCamera.enabled = false;
    }

    void SetDisplayAlpha(float a) => displayMaterial.SetColor(BaseColor, WithAlpha(displayMaterial.GetColor(BaseColor), a));

    // ---------- Helpers ----------

    static IEnumerator FadeGroup(CanvasGroup group, float to, float seconds)
    {
        float from = group.alpha;
        for (float t = 0f; t < seconds; t += Time.deltaTime)
        {
            group.alpha = Mathf.Lerp(from, to, t / seconds);
            yield return null;
        }
        group.alpha = to;
    }

    static Color WithAlpha(Color c, float a)
    {
        c.a = a;
        return c;
    }
}
