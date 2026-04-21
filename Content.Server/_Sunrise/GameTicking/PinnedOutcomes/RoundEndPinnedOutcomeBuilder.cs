namespace Content.Server._Sunrise.GameTicking.PinnedOutcomes;

/// <summary>
/// Helper for explicit producer-side pinned outcomes.
/// Keeps fragment construction centralized so modes can contribute round-level outcomes
/// without duplicating fragment/source metadata logic.
/// </summary>
public static class RoundEndPinnedOutcomeBuilder
{
    public static RoundEndTextFragment Explicit(string text, int paragraphIndex, int lineIndex = 0, string? sectionKey = null)
    {
        return new RoundEndTextFragment(
            text,
            RoundEndTextFragmentSourceKind.ExplicitOutcome,
            paragraphIndex,
            lineIndex,
            lineIndex == 0,
            sectionKey);
    }
}
