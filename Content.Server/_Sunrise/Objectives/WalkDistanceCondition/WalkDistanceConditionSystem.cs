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

    private void OnGetProgress(EntityUid uid, WalkDistanceConditionComponent comp, ref ObjectiveGetProgressEvent args)
    {
        var target = CompOrNull<NumberObjectiveComponent>(uid)?.Target ?? comp.Target;
        args.Progress = target <= 0 ? 0f : MathF.Min(1f, comp.Walked / (float) target);
    }

    private void OnMindAdded(EntityUid uid, MindContainerComponent mindContainer, MindAddedMessage args)
    {
        RefreshTracker(uid, args.Mind.Comp);
    }

    private void OnMindRemoved(EntityUid uid, MindContainerComponent mindContainer, MindRemovedMessage args)
    {
        RemComp<WalkDistanceTrackerComponent>(uid);
    }

    /// <summary>
    /// Attaches or refreshes the tracker on <paramref name="body"/> so it mirrors the
    /// walk-distance objectives currently on the mind. Call this after assigning objectives.
    /// </summary>
    public void RefreshTracker(EntityUid body, MindComponent mind)
    {
        var objectives = new List<EntityUid>();
        foreach (var objective in mind.Objectives)
        {
            if (HasComp<WalkDistanceConditionComponent>(objective))
                objectives.Add(objective);
        }

        if (objectives.Count == 0)
        {
            RemComp<WalkDistanceTrackerComponent>(body);
            return;
        }

        var tracker = EnsureComp<WalkDistanceTrackerComponent>(body);
        tracker.Objectives = objectives;
    }

    private void OnTrackerMove(EntityUid uid, WalkDistanceTrackerComponent tracker, ref MoveEvent args)
    {
        // Parent changes (teleports, entering containers, grid transitions) shouldn't count as walking.
        if (args.ParentChanged)
            return;

        // Compute actual world-space displacement in tiles.
        var oldMap = _transform.ToMapCoordinates(args.OldPosition, logError: false);
        var newMap = _transform.ToMapCoordinates(args.NewPosition, logError: false);

        if (oldMap.MapId != newMap.MapId || oldMap.MapId == MapId.Nullspace)
            return;

        var delta = (newMap.Position - oldMap.Position).Length();
        if (delta < MinMoveDelta)
            return;

        for (var i = tracker.Objectives.Count - 1; i >= 0; i--)
        {
            var objectiveUid = tracker.Objectives[i];
            if (!TryComp<WalkDistanceConditionComponent>(objectiveUid, out var walkComp))
            {
                // Objective was deleted — drop the stale reference.
                tracker.Objectives.RemoveAt(i);
                continue;
            }

            walkComp.Walked += delta;
            Dirty(objectiveUid, walkComp);
        }
    }
}
