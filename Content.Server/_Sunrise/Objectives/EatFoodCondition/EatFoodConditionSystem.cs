using Content.Server.Objectives.Components;
using Content.Shared.Objectives.Components;

namespace Content.Server._Sunrise.Objectives.EatFoodCondition;

/// <summary>
/// Handles progress for the eat food objective condition.
/// </summary>
public sealed class EatFoodConditionSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EatFoodConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);
    }

    private void OnGetProgress(Entity<EatFoodConditionComponent> ent, ref ObjectiveGetProgressEvent args)
    {
        var target = CompOrNull<NumberObjectiveComponent>(ent)?.Target ?? ent.Comp.Target;
        args.Progress = target <= 0 ? 0f : MathF.Min(1f, (float) ent.Comp.Eaten / target);
    }
}
