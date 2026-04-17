using Content.Server._Sunrise.Objectives.Components;
using Content.Server.Objectives.Components;
using Content.Shared.Objectives.Components;

namespace Content.Server._Sunrise.Objectives.Systems;

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

    private void OnGetProgress(EntityUid uid, EatFoodConditionComponent comp, ref ObjectiveGetProgressEvent args)
    {
        var target = CompOrNull<NumberObjectiveComponent>(uid)?.Target ?? comp.Target;
        args.Progress = target <= 0 ? 0f : MathF.Min(1f, (float) comp.Eaten / target);
    }
}
