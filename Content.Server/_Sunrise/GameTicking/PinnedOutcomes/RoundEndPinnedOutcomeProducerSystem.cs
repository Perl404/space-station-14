using Content.Server.Shuttles.Systems;

namespace Content.Server._Sunrise.GameTicking.PinnedOutcomes;

/// <summary>
/// First explicit producer for pinned outcomes. Systems can gradually move outcome emission here
/// instead of relying on legacy round-end text reconstruction.
/// </summary>
public sealed class RoundEndPinnedOutcomeProducerSystem : EntitySystem
{
    [Dependency] private readonly EmergencyShuttleSystem _emergencyShuttle = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundEndPinnedOutcomeBuildEvent>(OnBuildPinnedOutcomes);
    }

    private void OnBuildPinnedOutcomes(RoundEndPinnedOutcomeBuildEvent ev)
    {
        ev.AddFragment(RoundEndPinnedOutcomeBuilder.Explicit(
            _emergencyShuttle.EmergencyShuttleArrived
                ? Loc.GetString("round-end-summary-window-key-outcome-shuttle-arrived")
                : Loc.GetString("round-end-summary-window-key-outcome-shuttle-not-called"),
            -1,
            0,
            "shuttle"));
    }
}
