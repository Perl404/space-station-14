namespace Content.Server._Sunrise.GameTicking.PinnedOutcomes;

/// <summary>
/// Raised during round-end summary construction to let systems contribute structured
/// pinned-outcome fragments without relying on legacy round-end text parsing.
/// </summary>
public sealed class RoundEndPinnedOutcomeBuildEvent : EntityEventArgs
{
    public List<RoundEndTextFragment> Fragments { get; } = new();

    public void AddFragment(RoundEndTextFragment fragment)
    {
        Fragments.Add(fragment);
    }

    public void AddFragments(IEnumerable<RoundEndTextFragment> fragments)
    {
        Fragments.AddRange(fragments);
    }
}
