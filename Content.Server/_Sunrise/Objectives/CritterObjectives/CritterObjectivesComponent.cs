using Robust.Shared.Prototypes;

namespace Content.Server._Sunrise.Objectives.CritterObjectives;

/// <summary>
/// Defines which objectives a critter ghost role can receive
/// and how many to assign. Attached to critter entity prototypes
/// so that the objective pool is data-driven rather than hardcoded.
/// </summary>
[RegisterComponent, Access(typeof(CritterObjectivesSystem))]
public sealed partial class CritterObjectivesComponent : Component
{
    /// <summary>
    /// Pool of objective prototype IDs to randomly pick from.
    /// </summary>
    [DataField(required: true)]
    public List<EntProtoId> ObjectivesPool = new();

    /// <summary>
    /// How many objectives to assign from the pool.
    /// Defaults to 4 if not specified.
    /// </summary>
    [DataField]
    public int ObjectiveCount = 4;

    /// <summary>
    /// FTL localization key for the greeting chat message sent to the player.
    /// Receives <c>("count", objectiveCount)</c> as parameters.
    /// </summary>
    [DataField(required: true)]
    public LocId Greeting = string.Empty;

    /// <summary>
    /// Game rule prototype to register critter minds with for the round-end summary.
    /// </summary>
    [DataField(required: true)]
    public EntProtoId GameRule = string.Empty;

    /// <summary>
    /// Whether objectives have already been assigned to this entity.
    /// Prevents duplicate assignment if <see cref="MindAddedMessage"/> fires again
    /// (e.g. ghost role re-takeover after slot freeing).
    /// </summary>
    [DataField]
    public bool Assigned;
}
