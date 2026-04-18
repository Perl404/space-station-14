using Content.Server._Sunrise.Objectives.CritterRule;
using Content.Server._Sunrise.Objectives.WalkDistanceCondition;
using Content.Server.Chat.Managers;
using Content.Server.Ghost.Roles.Components;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Robust.Server.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Sunrise.Objectives.CritterObjectives;

/// <summary>
/// Assigns random objectives to critter ghost roles when they are taken.
/// Reads the objective pool from <see cref="CritterObjectivesComponent"/> on the entity,
/// so adding objectives to new critter types requires only a YAML change.
/// Also registers them with the critter game rule for the round-end summary.
/// </summary>
public sealed class CritterObjectivesSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly IPlayerManager _players = default!;
    [Dependency] private readonly CritterRuleSystem _critterRule = default!;
    [Dependency] private readonly WalkDistanceConditionSystem _walkDistance = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GhostRoleComponent, MindAddedMessage>(OnMindAdded);
    }

    private void OnMindAdded(Entity<GhostRoleComponent> ent, ref MindAddedMessage args)
    {
        if (!TryComp<CritterObjectivesComponent>(ent, out var critterObj))
            return;

        if (critterObj.Assigned)
            return;

        if (!TryComp<MindComponent>(args.Mind, out var mindComp))
            return;

        critterObj.Assigned = true;
        Dirty(ent, critterObj);

        var pool = critterObj.ObjectivesPool;
        var count = Math.Min(critterObj.ObjectiveCount, pool.Count);

        if (count <= 0)
            return;

        var shuffled = new List<EntProtoId>(pool);
        _random.Shuffle(shuffled);

        for (var i = 0; i < count; i++)
            _mind.TryAddObjective(args.Mind, mindComp, shuffled[i]);

        // Objectives are assigned — refresh the movement tracker so MoveEvent only
        // fires for critters that actually have a walk-distance objective.
        _walkDistance.RefreshTracker(ent.Owner, mindComp);

        // Register this critter with the game rule so objectives appear in the round-end summary.
        var characterName = mindComp.CharacterName ?? MetaData(ent.Owner).EntityName;
        _critterRule.AddCritterMind(args.Mind, characterName, critterObj.GameRule);

        if (mindComp.UserId is { } userId && _players.TryGetSessionById(userId, out var session))
        {
            _chat.DispatchServerMessage(session, Loc.GetString(critterObj.Greeting, ("count", count)));
        }
    }
}
