using UnityEngine;

// Slow, restrained "breathing" glow for the Assessor's eye indicator.
public class AssessorEyePulse : MonoBehaviour
{
    [SerializeField] Renderer iris;
    [SerializeField, ColorUsage(false, true)] Color glow = new(0.75f, 0.85f, 0.88f, 1f);
    [SerializeField] float periodSeconds = 6f;
    [SerializeField, Range(0f, 1f)] float depth = 0.35f;

    static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
    MaterialPropertyBlock block;

    void Update()
    {
        block ??= new MaterialPropertyBlock();
        float breath = 0.5f + 0.5f * Mathf.Sin(Time.time * 2f * Mathf.PI / periodSeconds);
        block.SetColor(EmissionColor, glow * (1f - depth * breath));
        iris.SetPropertyBlock(block);
    }
}
