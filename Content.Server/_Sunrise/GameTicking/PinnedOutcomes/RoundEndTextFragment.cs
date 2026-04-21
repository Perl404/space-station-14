namespace Content.Server._Sunrise.GameTicking.PinnedOutcomes;

public readonly record struct RoundEndTextFragment(
    string Text,
    RoundEndTextFragmentSourceKind SourceKind,
    int ParagraphIndex,
    int LineIndex,
    bool IsFirstLineInParagraph,
    string? SectionKey = null)
{
    public bool IsLegacy => SourceKind is RoundEndTextFragmentSourceKind.LegacyParagraph or RoundEndTextFragmentSourceKind.LegacyLine;
}
