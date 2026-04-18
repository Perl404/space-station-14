using Content.Server._Sunrise.Objectives.CritterIngestedCondition;

namespace Content.Server._Sunrise.Objectives.EatFoodCondition;

/// <summary>
/// Requires that the player eats a specific amount of food items.
/// </summary>
[RegisterComponent, Access(typeof(EatFoodConditionSystem), typeof(CritterIngestedConditionSystem))]
public sealed partial class EatFoodConditionComponent : Component
{
    /// <summary>
    /// Target number of food items to eat
    /// </summary>
    [DataField]
    public int Target = 5;

    /// <summary>
    /// Current number of food items eaten.
    /// Not networked — progress is read on demand via ObjectiveGetProgressEvent.
    /// </summary>
    [ViewVariables]
    public int Eaten;
}
