using Robust.Shared.ViewVariables;

namespace Content.Server._Sunrise.Objectives.ChewPaperCondition;

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
    [AutoNetworkedField]
    public int Chewed;
}
