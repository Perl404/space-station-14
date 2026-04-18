namespace Content.Server._Sunrise.Objectives.DrinkLiquidCondition;

/// <summary>
/// Requires that the player drinks a specific amount of liquid units.
/// </summary>
[RegisterComponent, Access(typeof(DrinkLiquidConditionSystem), typeof(CritterIngestedConditionSystem))]
public sealed partial class DrinkLiquidConditionComponent : Component
{
    /// <summary>
    /// Target amount of liquid units to drink
    /// </summary>
    [DataField]
    public float Target = 100f;

    /// <summary>
    /// Current amount of liquid units drunk.
    /// Not networked — progress is read on demand via ObjectiveGetProgressEvent.
    /// </summary>
    [ViewVariables]
    public float Drunk;
}
