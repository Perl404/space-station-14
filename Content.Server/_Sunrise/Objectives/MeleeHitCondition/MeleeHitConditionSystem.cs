using Content.Server.Objectives.Components;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;
using Content.Shared.Tag;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Server._Sunrise.Objectives.MeleeHitCondition;

/// <summary>
/// Handles progress for the melee hit objective condition.
/// Counts hits on entities that match the optional whitelist/blacklist tag filters.
/// </summary>
public sealed class MeleeHitConditionSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly TagSystem _tag = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MeleeHitConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);
        SubscribeLocalEvent<MeleeWeaponComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnGetProgress(EntityUid uid, MeleeHitConditionComponent comp, ref ObjectiveGetProgressEvent args)
    {
        var target = CompOrNull<NumberObjectiveComponent>(uid)?.Target ?? comp.Target;
        args.Progress = target <= 0 ? 0f : MathF.Min(1f, (float) comp.Hits / target);
    }

    private void OnMeleeHit(EntityUid weapon, MeleeWeaponComponent comp, MeleeHitEvent args)
    {
        // Only count actual landed hits (empty HitEntities means a miss/swing).
        if (!args.IsHit || args.HitEntities.Count == 0)
            return;

        // MeleeHitEvent is raised on the weapon entity, so use args.User for the attacker.
        if (!_mind.TryGetMind(args.User, out _, out var mindComp))
            return;

        var query = EntityQueryEnumerator<MeleeHitConditionComponent, ObjectiveComponent>();
        while (query.MoveNext(out var objectiveUid, out var hitComp, out _))
        {
            // Check if this objective belongs to the attacking entity's mind
            if (!mindComp.Objectives.Contains(objectiveUid))
                continue;

            var whitelist = hitComp.WhitelistTags;
            var blacklist = hitComp.BlacklistTags;
            var hasWhitelist = whitelist.Count > 0;
            var hasBlacklist = blacklist.Count > 0;

            foreach (var hitEntity in args.HitEntities)
            {
                // If whitelist is set, entity must have at least one matching tag.
                if (hasWhitelist && !_tag.HasAnyTag(hitEntity, whitelist.ToArray()))
                    continue;

                // If blacklist is set, entity must not have any matching tag.
                if (hasBlacklist && _tag.HasAnyTag(hitEntity, blacklist.ToArray()))
                    continue;

                hitComp.Hits++;
                Dirty(objectiveUid, hitComp);
            }
        }
    }
}
