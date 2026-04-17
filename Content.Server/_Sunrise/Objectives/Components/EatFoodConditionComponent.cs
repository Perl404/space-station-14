using Content.Server._Sunrise.Objectives.Systems;

namespace Content.Server._Sunrise.Objectives.Components;

/// <summary>
/// Requires that the player eats a specific amount of food items.
/// </summary>
[RegisterComponent, Access(typeof(EatFoodConditionSystem))]
public sealed partial class EatFoodConditionComponent : Component
{
    /// <summary>
    /// Target number of food items to eat
    /// </summary>
    [DataField]
    public int Target = 5;

    /// <summary>
    /// Current number of food items eaten
    /// </summary>
    [DataField]
    public int Eaten;
}
