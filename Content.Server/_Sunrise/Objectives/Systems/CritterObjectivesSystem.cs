using Content.Server.Chat.Managers;
using Content.Server.Ghost.Roles.Components;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Robust.Server.Player;
using Robust.Shared.Random;

namespace Content.Server._Sunrise.Objectives.Systems;

/// <summary>
/// Assigns random objectives to critter ghost roles (mice, mothroach) when they are taken.
/// Also registers them with the critter game rule so they appear in the round-end summary.
/// </summary>
public sealed class CritterObjectivesSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly IPlayerManager _players = default!;
    [Dependency] private readonly CritterRuleSystem _critterRule = default!;

    private const int ObjectiveCount = 4;

    private static readonly string[] MouseObjectives =
    {
        "EatFoodObjective",
        "DrinkLiquidObjective",
        "BiteObjective",
        "WalkDistanceObjective",
    };

    private static readonly string[] MothroachObjectives =
    {
        "DrinkLiquidObjective",
        "BiteObjective",
        "WalkDistanceObjective",
        "ChewPaperObjective",
    };

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GhostRoleComponent, MindAddedMessage>(OnMindAdded);
    }

    private void OnMindAdded(EntityUid uid, GhostRoleComponent component, MindAddedMessage args)
    {
        if (!TryComp<MindComponent>(args.Mind, out var mindComp))
            return;

        var protoId = MetaData(uid).EntityPrototype?.ID;
        var pool = protoId switch
        {
            "MobMouse" or "MobMouse1" or "MobMouse2" or "MobMouseCancer" => MouseObjectives,
            "MobMothroach" => MothroachObjectives,
            _ => null,
        };

        if (pool == null)
            return;

        var shuffled = (string[]) pool.Clone();
        _random.Shuffle(shuffled);

        var count = Math.Min(ObjectiveCount, shuffled.Length);
        for (var i = 0; i < count; i++)
            _mind.TryAddObjective(args.Mind, mindComp, shuffled[i]);

        // Register this critter with the game rule so objectives appear in the round-end summary.
        var characterName = mindComp.CharacterName ?? MetaData(uid).EntityName;
        _critterRule.AddCritterMind(args.Mind, characterName);

        if (mindComp.UserId is { } userId && _players.TryGetSessionById(userId, out var session))
        {
            var greetingKey = protoId == "MobMothroach"
                ? "critter-objectives-greeting-mothroach"
                : "critter-objectives-greeting-mouse";
            _chat.DispatchServerMessage(session, Loc.GetString(greetingKey, ("count", count)));
        }
    }
}
