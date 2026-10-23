using System.Collections;
using UnityEngine;

// Faint looping background bed (the supplied track). One instance survives scene reloads (Quit to Title,
// NEW RUN) so it never restarts or doubles. Every change glides to its target: a slow fade-in, a dip while
// the pause menu is open, and ducking while dialogue or a question is on screen (also the hook for any
// future voice lines). Not tied to story state or saves.
public class Ambience : MonoBehaviour
{
    const float PausedLevel = 0.4f;  // fraction of the bed while the pause menu is open
    const float DuckedLevel = 0.75f; // fraction while text is on screen; cinematic beats get the full bed
    const float Glide = 1f;          // SmoothDamp time (s): every change settles over ~3 s, never abruptly

    static Ambience instance;
    static int textPanels;           // dialogue / question panels currently showing
    AudioSource source;
    float volume, speed;
    bool paused;

    public static void Play(AudioClip clip, float volume)
    {
        if (!clip || instance) return; // already playing from an earlier load of the scene
        instance = new GameObject("Ambience").AddComponent<Ambience>();
        DontDestroyOnLoad(instance.gameObject);
        var s = instance.source = instance.gameObject.AddComponent<AudioSource>();
        s.clip = clip;
        s.loop = true;
        s.playOnAwake = false;
        s.spatialBlend = 0f;
        s.ignoreListenerPause = true;
        s.volume = 0f; // fades in from silence
        s.Play();
        instance.volume = volume;
        StoryDirector2D.Trace($"AMBIENCE play {clip.name} volume={volume:0.00}");
        instance.StartCoroutine(instance.MeasureLevel());
    }

    public static void SetPaused(bool paused)
    {
        if (instance) instance.paused = paused;
    }

    // Panels report when text appears or goes; a scene reload clears them all.
    public static void TextShown(bool shown) => textPanels = Mathf.Max(0, textPanels + (shown ? 1 : -1));
    public static void ResetText() => textPanels = 0;

    // Unscaled: the pause menu sets timeScale to 0.
    void Update()
    {
        float target = volume * (paused ? PausedLevel : 1f) * (textPanels > 0 ? DuckedLevel : 1f);
        source.volume = Mathf.SmoothDamp(source.volume, target, ref speed, Glide, Mathf.Infinity, Time.unscaledDeltaTime);
    }

    // QA: logs the mixed output level (RMS, dBFS) every 10 s, so the bed's loudness is checked in the real game.
    IEnumerator MeasureLevel()
    {
        var samples = new float[4096];
        while (true)
        {
            yield return new WaitForSecondsRealtime(10f);
            AudioListener.GetOutputData(samples, 0);
            double sum = 0;
            foreach (var x in samples) sum += x * x;
            float rms = Mathf.Sqrt((float)(sum / samples.Length));
            StoryDirector2D.Trace($"AMBIENCE level {(rms > 0f ? 20f * Mathf.Log10(rms) : -120f):0.0} dBFS volume={source.volume:0.000} " +
                $"ducked={textPanels > 0} paused={paused} t={source.time:0.0}s");
        }
    }
}
