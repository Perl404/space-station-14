using Content.Server._Sunrise.Objectives.CritterIngestedCondition;

namespace Content.Server._Sunrise.Objectives.ChewPaperCondition;

/// <summary>
/// Requires that the player chews a specific number of paper sheets.
/// </summary>
[RegisterComponent, Access(typeof(ChewPaperConditionSystem), typeof(CritterIngestedConditionSystem))]
public sealed partial class ChewPaperConditionComponent : Component
{
    /// <summary>
    /// Target number of paper sheets to chew
    /// </summary>
    [DataField]
    public int Target = 10;

    /// <summary>
    /// Current number of paper sheets chewed.
    /// Not networked — progress is read on demand via ObjectiveGetProgressEvent.
    /// </summary>
    [ViewVariables]
    public int Chewed;
}
