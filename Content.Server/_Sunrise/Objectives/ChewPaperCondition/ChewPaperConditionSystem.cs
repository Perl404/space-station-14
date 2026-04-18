using Content.Server._Sunrise.Objectives.Components;
using Content.Server.Objectives.Components;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Nutrition;
using Content.Shared.Objectives.Components;
using Content.Shared.Paper;

namespace Content.Server._Sunrise.Objectives.Systems;

/// <summary>
/// Handles progress for the chew paper objective condition.
/// Counts only when the player themselves ingests a paper item.
/// </summary>
public sealed class ChewPaperConditionSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChewPaperConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);
        // IngestingEvent is raised on the eater, so attach via MindContainerComponent to
        // guarantee we only ever credit the actual player who chewed the paper.
        SubscribeLocalEvent<MindContainerComponent, IngestingEvent>(OnIngesting);
    }

    private void OnGetProgress(Entity<ChewPaperConditionComponent> ent, ref ObjectiveGetProgressEvent args)
    {
        var target = CompOrNull<NumberObjectiveComponent>(ent)?.Target ?? ent.Comp.Target;
        args.Progress = target <= 0 ? 0f : MathF.Min(1f, (float) ent.Comp.Chewed / target);
    }

    private void OnIngesting(Entity<MindContainerComponent> ent, ref IngestingEvent args)
    {
        // Only paper items count for this objective.
        if (!HasComp<PaperComponent>(args.Food))
            return;

        if (!_mind.TryGetMind(ent.Owner, out _, out var mindComp, ent.Comp))
            return;

        var query = EntityQueryEnumerator<ChewPaperConditionComponent, ObjectiveComponent>();
        while (query.MoveNext(out var objUid, out var chewComp, out _))
        {
            if (!mindComp.Objectives.Contains(objUid))
                continue;

            chewComp.Chewed++;
            Dirty(objUid, chewComp);
        }
    }
}
