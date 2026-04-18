using Content.Server.Objectives.Components;
using Content.Shared.Objectives.Components;

namespace Content.Server._Sunrise.Objectives.IngestTargetCondition;

/// <summary>
/// Handles progress for the ingest target objective condition.
/// Actual counting is done by <see cref="Content.Server._Sunrise.Objectives.CritterIngestedCondition.CritterIngestedConditionSystem"/>.
/// </summary>
public sealed class IngestTargetConditionSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<IngestTargetConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);
    }

    private void OnGetProgress(Entity<IngestTargetConditionComponent> ent, ref ObjectiveGetProgressEvent args)
    {
        var target = CompOrNull<NumberObjectiveComponent>(ent)?.Target ?? ent.Comp.Target;
        args.Progress = target <= 0 ? 0f : MathF.Min(1f, (float) ent.Comp.Ingested / target);
    }
}
