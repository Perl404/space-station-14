using Robust.Shared.ViewVariables;

namespace Content.Server._Sunrise.Objectives.BiteCondition;

/// <summary>
/// Requires that the player bites food a specific number of times.
/// </summary>
[RegisterComponent, Access(typeof(BiteConditionSystem))]
public sealed partial class BiteConditionComponent : Component
{
    /// <summary>
    /// Target number of bites
    /// </summary>
    [DataField]
    public int Target = 20;

    /// <summary>
    /// Current number of bites performed
    /// </summary>
    [AutoNetworkedField]
    public int Bites;
}
