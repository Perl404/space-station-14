using Content.Server._Sunrise.Objectives.Systems;

namespace Content.Server._Sunrise.Objectives.Components;

/// <summary>
/// Requires that the player walks a specific distance in tiles.
/// </summary>
[RegisterComponent, Access(typeof(WalkDistanceConditionSystem))]
public sealed partial class WalkDistanceConditionComponent : Component
{
    /// <summary>
    /// Target distance in tiles to walk
    /// </summary>
    [DataField]
    public int Target = 2000;

    /// <summary>
    /// Current distance walked in tiles
    /// </summary>
    [DataField]
    public float Walked;
}
