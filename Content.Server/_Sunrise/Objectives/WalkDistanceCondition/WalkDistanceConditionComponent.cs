namespace Content.Server._Sunrise.Objectives.WalkDistanceCondition;

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
    /// Current distance walked in tiles.
    /// Not networked — progress is read on demand in OnGetProgress.
    /// </summary>
    [ViewVariables]
    public float Walked;
}
