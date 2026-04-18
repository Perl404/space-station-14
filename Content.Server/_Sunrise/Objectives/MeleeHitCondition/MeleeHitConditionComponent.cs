using Content.Shared.Tag;
using Robust.Shared.Prototypes;
using Robust.Shared.ViewVariables;

namespace Content.Server._Sunrise.Objectives.MeleeHitCondition;

/// <summary>
/// Requires that the player lands melee hits on entities matching the tag filter
/// a specific number of times.
/// </summary>
[RegisterComponent, Access(typeof(MeleeHitConditionSystem))]
public sealed partial class MeleeHitConditionComponent : Component
{
    /// <summary>
    /// Target number of qualifying melee hits.
    /// </summary>
    [DataField]
    public int Target = 20;

    /// <summary>
    /// Current number of qualifying melee hits performed.
    /// </summary>
    [AutoNetworkedField]
    public int Hits;

    /// <summary>
    /// Only count hits on entities that have at least one of these tags.
    /// Empty list means all entities qualify.
    /// </summary>
    [DataField]
    public List<ProtoId<TagPrototype>> WhitelistTags = new();

    /// <summary>
    /// Do not count hits on entities that have any of these tags.
    /// Checked after whitelist; empty list means nothing is excluded.
    /// </summary>
    [DataField]
    public List<ProtoId<TagPrototype>> BlacklistTags = new();
}
