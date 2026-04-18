using Content.Server._Sunrise.Objectives.Components;
using Content.Server.Objectives.Components;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Server._Sunrise.Objectives.Systems;

/// <summary>
/// Handles progress for the bite objective condition.
/// </summary>
public sealed class BiteConditionSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BiteConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);
        // Subscribe directed on MeleeWeaponComponent — MeleeHitEvent is raised on the weapon entity
        // without the broadcast flag, so a broadcast subscription would never fire.
        SubscribeLocalEvent<MeleeWeaponComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnGetProgress(Entity<BiteConditionComponent> ent, ref ObjectiveGetProgressEvent args)
    {
        var target = CompOrNull<NumberObjectiveComponent>(ent)?.Target ?? ent.Comp.Target;
        args.Progress = target <= 0 ? 0f : MathF.Min(1f, (float) ent.Comp.Bites / target);
    }

    private void OnMeleeHit(Entity<MeleeWeaponComponent> ent, ref MeleeHitEvent args)
    {
        // Only count actual landed hits (empty HitEntities means a miss/swing).
        if (!args.IsHit || args.HitEntities.Count == 0)
            return;

        // MeleeHitEvent is raised on the weapon entity, so use args.User for the attacker.
        if (!_mind.TryGetMind(args.User, out _, out var mindComp))
            return;

        // Find all BiteCondition objectives for this mind
        var query = EntityQueryEnumerator<BiteConditionComponent, ObjectiveComponent>();
        while (query.MoveNext(out var objectiveUid, out var biteComp, out _))
        {
            // Check if this objective belongs to the attacking entity's mind
            if (!mindComp.Objectives.Contains(objectiveUid))
                continue;

            biteComp.Bites++;
            Dirty(objectiveUid, biteComp);
        }
    }
}
