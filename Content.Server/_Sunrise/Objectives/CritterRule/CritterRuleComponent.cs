
namespace Content.Server._Sunrise.Objectives.CritterRule;

/// <summary>
/// Gamerule for critter ghost roles that have objectives.
/// Stores all critter minds so they appear in the round-end summary.
/// </summary>
[RegisterComponent, Access(typeof(CritterRuleSystem))]
public sealed partial class CritterRuleComponent : Component
{
    /// <summary>
    /// All critter minds that have objectives under this rule.
    /// </summary>
    [DataField]
    public List<(EntityUid MindId, string Name)> Minds = new();
}
