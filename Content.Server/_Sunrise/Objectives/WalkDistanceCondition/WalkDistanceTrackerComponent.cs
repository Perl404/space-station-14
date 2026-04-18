namespace Content.Server._Sunrise.Objectives.WalkDistanceCondition;

/// <summary>
/// Attached to an entity whose movement should count toward walk-distance objectives.
/// Exists so <see cref="WalkDistanceConditionSystem"/> can subscribe to MoveEvent
/// against this component only, instead of every moving entity with a mind.
/// </summary>
[RegisterComponent, Access(typeof(WalkDistanceConditionSystem))]
public sealed partial class WalkDistanceTrackerComponent : Component
{
    /// <summary>
    /// Objective entities (with <see cref="WalkDistanceConditionComponent"/>) whose
    /// Walked counter should be incremented when this entity moves.
    /// </summary>
    [DataField]
    public List<EntityUid> Objectives = new();
}
