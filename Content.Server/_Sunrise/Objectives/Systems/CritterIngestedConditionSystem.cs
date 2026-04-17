using Content.Server._Sunrise.Objectives.Components;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Nutrition;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Objectives.Components;

namespace Content.Server._Sunrise.Objectives.Systems;

/// <summary>
/// Sunrise added - handles critter ingestion objective progress from a single directed ingestion hook.
/// </summary>
public sealed class CritterIngestedConditionSystem : EntitySystem
{
    [Dependency] private readonly DrinkLiquidConditionSystem _drinkLiquid = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MetaDataComponent, IngestingEvent>(OnIngesting);
    }

    private void OnIngesting(Entity<MetaDataComponent> ent, ref IngestingEvent args)
    {
        if (!TryComp<EdibleComponent>(args.Food, out var edible))
            return;

        if (!_mind.TryGetMind(ent.Owner, out _, out var mindComp))
            return;

        if (edible.Edible == IngestionSystem.Food)
        {
            var eatQuery = EntityQueryEnumerator<EatFoodConditionComponent, ObjectiveComponent>();
            while (eatQuery.MoveNext(out var objectiveUid, out var eatComp, out _))
            {
                if (!mindComp.Objectives.Contains(objectiveUid))
                    continue;

                eatComp.Eaten++;
                Dirty(objectiveUid, eatComp);
            }

            return;
        }

        if (edible.Edible != IngestionSystem.Drink)
            return;

        var volume = args.Split.Volume.Float();
        if (volume <= 0f)
            return;

        var drinkQuery = EntityQueryEnumerator<DrinkLiquidConditionComponent, ObjectiveComponent>();
        while (drinkQuery.MoveNext(out var objectiveUid, out var drinkComp, out _))
        {
            if (!mindComp.Objectives.Contains(objectiveUid))
                continue;

            _drinkLiquid.AddDrunk((objectiveUid, drinkComp), volume);
        }
    }
}
