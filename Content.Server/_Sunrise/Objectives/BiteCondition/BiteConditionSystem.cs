using Content.Server.Objectives.Components;
using Content.Shared.Objectives.Components;

namespace Content.Server._Sunrise.Objectives.BiteCondition;

/// <summary>
/// Handles progress for the bite objective condition.
/// Actual bite counting is routed through
/// <see cref="MeleeHitCondition.MeleeHitConditionSystem"/>
/// to avoid duplicate EventBus subscriptions on the same (MeleeWeaponComponent, MeleeHitEvent) pair.
/// </summary>
public sealed class BiteConditionSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BiteConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);
    }

    private void OnGetProgress(Entity<BiteConditionComponent> ent, ref ObjectiveGetProgressEvent args)
    {
        var target = CompOrNull<NumberObjectiveComponent>(ent)?.Target ?? ent.Comp.Target;
        args.Progress = target <= 0 ? 0f : MathF.Min(1f, (float) ent.Comp.Bites / target);
    }
}
