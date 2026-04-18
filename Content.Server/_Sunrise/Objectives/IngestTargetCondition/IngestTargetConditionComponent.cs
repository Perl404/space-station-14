using Content.Server.Objectives.Components;
using Content.Shared.Objectives.Components;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;
using Robust.Shared.ViewVariables;

namespace Content.Server._Sunrise.Objectives.IngestTargetCondition;

/// <summary>
/// Requires that the player ingests entities matching the tag filter
/// a specific number of times.
/// Actual hit counting is done by <see cref="Content.Server._Sunrise.Objectives.CritterIngestedCondition.CritterIngestedConditionSystem"/>.
/// </summary>
[RegisterComponent, Access(typeof(IngestTargetConditionSystem), typeof(CritterIngestedCondition.CritterIngestedConditionSystem))]
public sealed partial class IngestTargetConditionComponent : Component
{
    /// <summary>
    /// Target number of qualifying ingested items.
    /// </summary>
    [DataField]
    public int Target = 10;

    /// <summary>
    /// Current number of qualifying items ingested.
    /// </summary>
    [AutoNetworkedField]
    public int Ingested;

    /// <summary>
    /// Only count ingested entities that have at least one of these tags.
    /// Empty list means all entities qualify.
    /// </summary>
    [DataField]
    public List<ProtoId<TagPrototype>> WhitelistTags = new();

    /// <summary>
    /// Do not count ingested entities that have any of these tags.
    /// Checked after whitelist; empty list means nothing is excluded.
    /// </summary>
    [DataField]
    public List<ProtoId<TagPrototype>> BlacklistTags = new();
}
