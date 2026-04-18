using Content.Server.Objectives.Components;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Server._Sunrise.Objectives.BiteCondition;

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

        // Only count unarmed attacks (bites/claws) — if the weapon is a separate item, it's not a bite.
        if (ent.Owner != args.User)
            return;

        // MeleeHitEvent is raised on the weapon entity, so use args.User for the attacker.
        if (!_mind.TryGetMind(args.User, out _, out var mindComp))
            return;

        // Iterate only this mind's objectives (usually ≤ a handful) instead of querying all bite objectives globally.
        foreach (var objectiveUid in mindComp.Objectives)
        {
            if (!TryComp<BiteConditionComponent>(objectiveUid, out var biteComp))
                continue;

            biteComp.Bites++;
            Dirty(objectiveUid, biteComp);
        }
    }
}
