using Content.Server._Sunrise.Objectives.Components;
using Content.Server.Objectives.Components;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Objectives.Components;
using Robust.Shared.Map;

namespace Content.Server._Sunrise.Objectives.Systems;

/// <summary>
/// Handles progress for the walk distance objective condition.
/// </summary>
public sealed class WalkDistanceConditionSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    /// <summary>
    /// Ignore tiny sub-tile drifts (e.g. physics jitter) so we only count intentional movement.
    /// </summary>
    private const float MinMoveDelta = 0.05f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WalkDistanceConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);
        SubscribeLocalEvent<MindContainerComponent, MoveEvent>(OnMove);
    }

    private void OnGetProgress(EntityUid uid, WalkDistanceConditionComponent comp, ref ObjectiveGetProgressEvent args)
    {
        var target = CompOrNull<NumberObjectiveComponent>(uid)?.Target ?? comp.Target;
        args.Progress = target <= 0 ? 0f : MathF.Min(1f, comp.Walked / (float) target);
    }

    private void OnMove(EntityUid uid, MindContainerComponent mindContainer, ref MoveEvent args)
    {
        // Parent changes (teleports, entering containers, grid transitions) shouldn't count as walking.
        if (args.ParentChanged)
            return;

        if (!_mind.TryGetMind(uid, out _, out var mindComp, mindContainer))
            return;

        // Compute actual world-space displacement in tiles.
        var oldMap = _transform.ToMapCoordinates(args.OldPosition, logError: false);
        var newMap = _transform.ToMapCoordinates(args.NewPosition, logError: false);

        if (oldMap.MapId != newMap.MapId || oldMap.MapId == MapId.Nullspace)
            return;

        var delta = (newMap.Position - oldMap.Position).Length();
        if (delta < MinMoveDelta)
            return;

        var query = EntityQueryEnumerator<WalkDistanceConditionComponent, ObjectiveComponent>();
        while (query.MoveNext(out var objectiveUid, out var walkComp, out _))
        {
            if (!mindComp.Objectives.Contains(objectiveUid))
                continue;

            walkComp.Walked += delta;
            Dirty(objectiveUid, walkComp);
        }
    }
}
