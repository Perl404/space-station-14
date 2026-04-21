namespace Content.Server._Sunrise.GameTicking.PinnedOutcomes;

public enum RoundEndTextFragmentSourceKind : byte
{
    Unknown = 0,
    LegacyParagraph,
    LegacyLine,
    RulePrepend,
    ObjectiveSummary,
    ObjectiveIssuer,
    ObjectiveEntry,
    ExplicitOutcome,
    ShuttleStatus,
}
