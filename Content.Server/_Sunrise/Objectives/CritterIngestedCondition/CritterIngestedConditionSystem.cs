using Content.Server._Sunrise.Objectives.ChewPaperCondition;
using Content.Server._Sunrise.Objectives.DrinkLiquidCondition;
using Content.Server._Sunrise.Objectives.EatFoodCondition;
using Content.Server._Sunrise.Objectives.IngestTargetCondition;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Nutrition;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Paper;
using Content.Shared.Tag;

namespace Content.Server._Sunrise.Objectives.CritterIngestedCondition;

/// <summary>
/// Sunrise added - handles critter ingestion objective progress from a single directed ingestion hook.
/// Routes ingested items to the appropriate condition based on type and tag filters.
/// This is the sole subscriber to <see cref="IngestingEvent"/> via <see cref="MindContainerComponent"/>
/// to avoid duplicate-subscription errors in the EventBus.
/// </summary>
public sealed class CritterIngestedConditionSystem : EntitySystem
{
    [Dependency] private readonly DrinkLiquidConditionSystem _drinkLiquid = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly TagSystem _tag = default!;

    public override void Initialize()
    {
        base.Initialize();

        // IngestingEvent is raised on the eater, so subscribe via MindContainerComponent
        // to only dispatch for entities that can have a mind (not every MetaData entity).
        // IMPORTANT: Only one system may subscribe to (MindContainerComponent, IngestingEvent).
        // All ingestion routing is handled here — other condition systems must NOT subscribe
        // to the same pair or the EventBus will throw DuplicateSubscription.
        SubscribeLocalEvent<MindContainerComponent, IngestingEvent>(OnIngesting);
    }

    private void OnIngesting(EntityUid uid, MindContainerComponent comp, ref IngestingEvent args)
    {
        if (!_mind.TryGetMind(uid, out _, out var mindComp, comp))
            return;

        if (TryComp<EdibleComponent>(args.Food, out var edible))
        {
            if (edible.Edible == IngestionSystem.Food)
                OnEatFood(args.Food, mindComp);
            else if (edible.Edible == IngestionSystem.Drink)
                OnDrinkLiquid(args, mindComp);
        }

        OnChewPaper(args.Food, mindComp);
        OnIngestTarget(args.Food, mindComp);
    }

    private void OnEatFood(EntityUid food, MindComponent mindComp)
    {
        foreach (var objectiveUid in mindComp.Objectives)
        {
            if (!TryComp<EatFoodConditionComponent>(objectiveUid, out var eatComp))
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

        foreach (var objectiveUid in mindComp.Objectives)
        {
            if (!TryComp<DrinkLiquidConditionComponent>(objectiveUid, out var drinkComp))
                continue;

            _drinkLiquid.AddDrunk((objectiveUid, drinkComp), volume);
        }
    }

    private void OnIngestTarget(EntityUid food, MindComponent mindComp)
    {
        foreach (var objectiveUid in mindComp.Objectives)
        {
            if (!TryComp<IngestTargetConditionComponent>(objectiveUid, out var ingestComp))
                continue;

            var whitelist = ingestComp.WhitelistTags;
            var blacklist = ingestComp.BlacklistTags;

            if (whitelist.Count > 0 && !_tag.HasAnyTag(food, whitelist))
                continue;

            if (blacklist.Count > 0 && _tag.HasAnyTag(food, blacklist))
                continue;

            ingestComp.Ingested++;
            Dirty(objectiveUid, ingestComp);
        }
    }

    private void OnChewPaper(EntityUid food, MindComponent mindComp)
    {
        if (!HasComp<PaperComponent>(food))
            return;

        foreach (var objectiveUid in mindComp.Objectives)
        {
            if (!TryComp<ChewPaperConditionComponent>(objectiveUid, out var chewComp))
                continue;

            chewComp.Chewed++;
            Dirty(objectiveUid, chewComp);
        }
    }
}
