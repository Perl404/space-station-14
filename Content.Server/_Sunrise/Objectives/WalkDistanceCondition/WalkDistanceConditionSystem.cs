using Content.Server.Objectives.Components;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Objectives.Components;
using Robust.Shared.Map;

namespace Content.Server._Sunrise.Objectives.WalkDistanceCondition;

/// <summary>
/// Handles progress for the walk distance objective condition.
/// Movement is tracked through <see cref="WalkDistanceTrackerComponent"/>, which is
/// attached only to entities that actually have the objective — this keeps MoveEvent
/// dispatch cost bound to those players instead of every mind-bearing entity.
/// </summary>
public sealed class WalkDistanceConditionSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    /// <summary>
    /// Ignore tiny sub-tile drifts (e.g. physics jitter) so we only count intentional movement.
    /// </summary>
    private const float MinMoveDelta = 0.05f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WalkDistanceConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);

        SubscribeLocalEvent<MindContainerComponent, MindAddedMessage>(OnMindAdded);
        SubscribeLocalEvent<MindContainerComponent, MindRemovedMessage>(OnMindRemoved);

        SubscribeLocalEvent<WalkDistanceTrackerComponent, MoveEvent>(OnTrackerMove);
    }

    private void OnGetProgress(Entity<WalkDistanceConditionComponent> ent, ref ObjectiveGetProgressEvent args)
    {
        var target = CompOrNull<NumberObjectiveComponent>(ent)?.Target ?? ent.Comp.Target;
        args.Progress = target <= 0 ? 0f : MathF.Min(1f, ent.Comp.Walked / (float) target);
    }

    private void OnMindAdded(EntityUid uid, MindContainerComponent comp, MindAddedMessage args)
    {
        RefreshTracker(uid, args.Mind.Comp);
    }

    private void OnMindRemoved(EntityUid uid, MindContainerComponent comp, MindRemovedMessage args)
    {
        RemComp<WalkDistanceTrackerComponent>(uid);
    }

    /// <summary>
    /// Attaches or refreshes the tracker on <paramref name="body"/> so it mirrors the
    /// walk-distance objectives currently on the mind. Call this after assigning objectives.
    /// </summary>
    public void RefreshTracker(EntityUid body, MindComponent mind)
    {
        var tracker = EnsureComp<WalkDistanceTrackerComponent>(body);
        tracker.Objectives.Clear();

        foreach (var objective in mind.Objectives)
        {
            if (HasComp<WalkDistanceConditionComponent>(objective))
                tracker.Objectives.Add(objective);
        }

        if (tracker.Objectives.Count == 0)
            RemComp<WalkDistanceTrackerComponent>(body);
    }

    private void OnTrackerMove(Entity<WalkDistanceTrackerComponent> ent, ref MoveEvent args)
    {
        // Parent changes (teleports, entering containers, grid transitions) shouldn't count as walking.
        if (args.ParentChanged)
            return;

        // Cheap early-exit on local displacement before touching the transform system.
        // If both positions share a parent this is already the real delta; otherwise it's
        // still a conservative upper bound — a tiny local delta means tiny world delta too.
        var localDeltaSq = (args.NewPosition.Position - args.OldPosition.Position).LengthSquared();
        if (localDeltaSq < MinMoveDelta * MinMoveDelta)
            return;

        // Compute actual world-space displacement in tiles.
        // logError: false is intentional — after ParentChanged or grid transitions the old
        // EntityCoordinates may reference a no-longer-valid parent, which is expected here;
        // the MapId comparison below safely discards such cases.
        var oldMap = _transform.ToMapCoordinates(args.OldPosition, logError: false);
        var newMap = _transform.ToMapCoordinates(args.NewPosition, logError: false);

        if (oldMap.MapId != newMap.MapId || oldMap.MapId == MapId.Nullspace)
            return;

        var delta = (newMap.Position - oldMap.Position).Length();
        if (delta < MinMoveDelta)
            return;

        var tracker = ent.Comp;
        for (var i = tracker.Objectives.Count - 1; i >= 0; i--)
        {
            var objectiveUid = tracker.Objectives[i];
            if (!TryComp<WalkDistanceConditionComponent>(objectiveUid, out var walkComp))
            {
                // Objective was deleted — drop the stale reference.
                tracker.Objectives.RemoveAt(i);
                continue;
            }

            // Progress is read on demand in OnGetProgress; no Dirty needed — component isn't networked.
            walkComp.Walked += delta;
        }
    }
}
