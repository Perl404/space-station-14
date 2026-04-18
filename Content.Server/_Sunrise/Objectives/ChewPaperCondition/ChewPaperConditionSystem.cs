using Content.Server.Objectives.Components;
using Content.Shared.Objectives.Components;

namespace Content.Server._Sunrise.Objectives.ChewPaperCondition;

/// <summary>
/// Handles progress for the chew paper objective condition.
/// Actual ingestion counting is routed through
/// <see cref="CritterIngestedCondition.CritterIngestedConditionSystem"/>
/// to avoid duplicate EventBus subscriptions on the same (MindContainerComponent, IngestingEvent) pair.
/// </summary>
public sealed class ChewPaperConditionSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChewPaperConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);
    }

    private void OnGetProgress(Entity<ChewPaperConditionComponent> ent, ref ObjectiveGetProgressEvent args)
    {
        var target = CompOrNull<NumberObjectiveComponent>(ent)?.Target ?? ent.Comp.Target;
        args.Progress = target <= 0 ? 0f : MathF.Min(1f, (float) ent.Comp.Chewed / target);
    }
}
