using Content.Server._Sunrise.Objectives.DrinkLiquidCondition;
using Content.Server._Sunrise.Objectives.EatFoodCondition;
using Content.Server._Sunrise.Objectives.IngestTargetCondition;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Nutrition;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Objectives.Components;
using Content.Shared.Tag;

namespace Content.Server._Sunrise.Objectives.CritterIngestedCondition;

/// <summary>
/// Sunrise added - handles critter ingestion objective progress from a single directed ingestion hook.
/// Routes ingested items to the appropriate condition based on type and tag filters.
/// </summary>
public sealed class CritterIngestedConditionSystem : EntitySystem
{
    [Dependency] private readonly DrinkLiquidConditionSystem _drinkLiquid = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly TagSystem _tag = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MetaDataComponent, IngestingEvent>(OnIngesting);
    }

    private void OnIngesting(Entity<MetaDataComponent> ent, ref IngestingEvent args)
    {
        if (!_mind.TryGetMind(ent.Owner, out _, out var mindComp))
            return;

        if (TryComp<EdibleComponent>(args.Food, out var edible))
        {
            if (edible.Edible == IngestionSystem.Food)
                OnEatFood(args.Food, mindComp);
            else if (edible.Edible == IngestionSystem.Drink)
                OnDrinkLiquid(args, mindComp);
        }

        OnIngestTarget(args.Food, mindComp);
    }

    private void OnEatFood(EntityUid food, MindComponent mindComp)
    {
        var query = EntityQueryEnumerator<EatFoodConditionComponent, ObjectiveComponent>();
        while (query.MoveNext(out var objectiveUid, out var eatComp, out _))
        {
            if (!mindComp.Objectives.Contains(objectiveUid))
                continue;

            eatComp.Eaten++;
            Dirty(objectiveUid, eatComp);
        }
    }

    private void OnDrinkLiquid(IngestingEvent args, MindComponent mindComp)
    {
        var volume = args.Split.Volume.Float();
        if (volume <= 0f)
            return;

        var query = EntityQueryEnumerator<DrinkLiquidConditionComponent, ObjectiveComponent>();
        while (query.MoveNext(out var objectiveUid, out var drinkComp, out _))
        {
            if (!mindComp.Objectives.Contains(objectiveUid))
                continue;

            _drinkLiquid.AddDrunk((objectiveUid, drinkComp), volume);
        }
    }

    private void OnIngestTarget(EntityUid food, MindComponent mindComp)
    {
        var query = EntityQueryEnumerator<IngestTargetConditionComponent, ObjectiveComponent>();
        while (query.MoveNext(out var objectiveUid, out var ingestComp, out _))
        {
            if (!mindComp.Objectives.Contains(objectiveUid))
                continue;

            var whitelist = ingestComp.WhitelistTags;
            var blacklist = ingestComp.BlacklistTags;

            if (whitelist.Count > 0 && !_tag.HasAnyTag(food, whitelist.ToArray()))
                continue;

            if (blacklist.Count > 0 && _tag.HasAnyTag(food, blacklist.ToArray()))
                continue;

            ingestComp.Ingested++;
            Dirty(objectiveUid, ingestComp);
        }
    }
}
