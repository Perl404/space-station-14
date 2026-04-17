using Content.Server._Sunrise.Objectives.Systems;

namespace Content.Server._Sunrise.Objectives.Components;

/// <summary>
/// Requires that the player drinks a specific amount of liquid units.
/// </summary>
[RegisterComponent, Access(typeof(DrinkLiquidConditionSystem))]
public sealed partial class DrinkLiquidConditionComponent : Component
{
    /// <summary>
    /// Target amount of liquid units to drink
    /// </summary>
    [DataField]
    public float Target = 100f;

    /// <summary>
    /// Current amount of liquid units drunk
    /// </summary>
    [DataField]
    public float Drunk;
}
