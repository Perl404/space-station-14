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

    private void OnGetProgress(Entity<DrinkLiquidConditionComponent> ent, ref ObjectiveGetProgressEvent args)
    {
        var target = CompOrNull<NumberObjectiveComponent>(ent)?.Target ?? (int) ent.Comp.Target;
        args.Progress = target <= 0 ? 0f : MathF.Min(1f, ent.Comp.Drunk / target);
    }

    public void AddDrunk(Entity<DrinkLiquidConditionComponent> ent, float amount)
    {
        if (amount <= 0f)
            return;

        ent.Comp.Drunk += amount;
    }
}
