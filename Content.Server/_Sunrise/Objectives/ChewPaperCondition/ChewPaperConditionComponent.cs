using Content.Server._Sunrise.Objectives.Systems;

namespace Content.Server._Sunrise.Objectives.Components;

/// <summary>
/// Requires that the player chews a specific number of paper sheets.
/// </summary>
[RegisterComponent, Access(typeof(ChewPaperConditionSystem))]
public sealed partial class ChewPaperConditionComponent : Component
{
    /// <summary>
    /// Target number of paper sheets to chew
    /// </summary>
    [DataField]
    public int Target = 10;

    /// <summary>
    /// Current number of paper sheets chewed
    /// </summary>
    [DataField]
    public int Chewed;
}
