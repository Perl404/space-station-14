namespace Content.Server._Sunrise.GameTicking.PinnedOutcomes;

public enum RoundEndPinnedOutcomeLineKind : byte
{
    Unknown = 0,
    Noise,
    ObjectiveBullet,
    ObjectiveIntro,
    IssuerLabel,
    PlayerHeader,
    RoundSummary,
    RoundOutcome,
    StatusSummary,
}
