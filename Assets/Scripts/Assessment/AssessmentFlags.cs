// Flag names that C# systems read. Narrative data sets flags by the same strings (setFlag / setFlags).
public static class AssessmentFlags
{
    // Q3: the valuation shrank into the corner; restored on Continue.
    public const string ValuationInCorner = "valuation_in_corner";

    // Q12 (not yet authored): hearing the recording and retaining it are separate states (decision D2).
    public const string ResidualAudioHeard = "residual_audio_heard";
    public const string ResidualAudioRetained = "residual_audio_retained";
}
