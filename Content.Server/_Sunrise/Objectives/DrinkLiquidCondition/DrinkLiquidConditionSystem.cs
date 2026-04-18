using Content.Server.Objectives.Components;
using Content.Shared.Objectives.Components;

namespace Content.Server._Sunrise.Objectives.DrinkLiquidCondition;

/// <summary>
/// Handles progress for the drink liquid objective condition.
/// </summary>
public sealed class DrinkLiquidConditionSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DrinkLiquidConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);
    }

    private void OnGetProgress(EntityUid uid, DrinkLiquidConditionComponent comp, ref ObjectiveGetProgressEvent args)
    {
        var target = CompOrNull<NumberObjectiveComponent>(uid)?.Target ?? (int) comp.Target;
        args.Progress = target <= 0 ? 0f : MathF.Min(1f, comp.Drunk / target);
    }

    public void AddDrunk(Entity<DrinkLiquidConditionComponent> ent, float amount)
    {
        if (amount <= 0f)
            return;

        ent.Comp.Drunk += amount;
        Dirty(ent);
    }
}
