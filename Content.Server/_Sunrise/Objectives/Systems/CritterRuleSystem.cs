using Content.Server._Sunrise.Objectives.Components;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.Objectives;
using Content.Shared.GameTicking.Components;
using Content.Shared.Mind.Components;

namespace Content.Server._Sunrise.Objectives.Systems;

/// <summary>
/// Manages the critter game rule so that critter objectives appear
/// in the round-end summary (F9 manifest).
/// </summary>
public sealed class CritterRuleSystem : GameRuleSystem<CritterRuleComponent>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CritterRuleComponent, ObjectivesTextGetInfoEvent>(OnObjectivesTextGetInfo);
    }

    private void OnObjectivesTextGetInfo(EntityUid uid, CritterRuleComponent comp, ref ObjectivesTextGetInfoEvent args)
    {
        args.Minds = comp.Minds;
        args.AgentName = Loc.GetString("critter-round-end-agent-name");
    }

    /// <summary>
    /// Ensures a <see cref="CritterRuleComponent"/> game rule exists and adds the mind to it.
    /// Called by <see cref="CritterObjectivesSystem"/> when a critter gets objectives.
    /// </summary>
    public EntityUid? AddCritterMind(EntityUid mindId, string name)
    {
        // Find an existing critter rule, or create one if none exists.
        var rule = EntityQueryEnumerator<CritterRuleComponent, GameRuleComponent>();
        while (rule.MoveNext(out var ruleUid, out var critterRule, out var gameRule))
        {
            if (GameTicker.IsGameRuleAdded(ruleUid, gameRule))
            {
                critterRule.Minds.Add((mindId, name));
                return ruleUid;
            }
        }

        // No active rule — create one.
        var newRuleUid = GameTicker.AddGameRule("CritterRule");
        if (!TryComp<CritterRuleComponent>(newRuleUid, out var newComp))
        {
            Log.Error("CritterRule prototype is missing CritterRuleComponent, deleting it.");
            Del(newRuleUid);
            return null;
        }

        if (!GameTicker.StartGameRule(newRuleUid))
        {
            Log.Error("CritterRule failed to start, deleting it.");
            Del(newRuleUid);
            return null;
        }

        newComp.Minds.Add((mindId, name));
        return newRuleUid;
    }
}
