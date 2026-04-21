using System.Linq;
using System.Text;
using Content.Server._Sunrise.GameTicking.PinnedOutcomes;
using Content.Server.GameTicking;
using Content.Server.Objectives;
using Content.Server.Shuttles.Systems;
using Content.Shared.Objectives.Systems;
using Content.Shared.Cuffs.Components;
using Content.Shared.GameTicking;
using Content.Shared.GameTicking.Components;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;
using Robust.Shared.Configuration;

namespace Content.Server._Sunrise.GameTicking;

public sealed class RoundEndSectionSystem : EntitySystem
{
    private const int MaxPinnedOutcomes = 4;

    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly ObjectivesSystem _objectives = default!;

    private bool _showGreentext;

    public override void Initialize()
    {
        base.Initialize();

        Subs.CVar(_cfg, Content.Shared.CCVar.CCVars.GameShowGreentext, value => _showGreentext = value, true);
    }

    public RoundEndKeyOutcome[] BuildKeyOutcomes(string roundEndText)
    {
        var candidates = new List<(RoundEndKeyOutcome Outcome, int Priority, int Order)>();
        var order = 0;
        var fragments = BuildPinnedOutcomeFragments(roundEndText);

        foreach (var (line, priority) in RoundEndPinnedOutcomeClassifier.EnumerateRankedCandidates(fragments))
        {
            candidates.Add((new RoundEndKeyOutcome(line, RoundEndPinnedOutcomeClassifier.GetColor(priority)), priority, order));
            order++;
        }

        return candidates
            .OrderByDescending(entry => entry.Priority)
            .ThenBy(entry => entry.Order)
            .Select(entry => entry.Outcome)
            .DistinctBy(outcome => outcome.Text)
            .Take(MaxPinnedOutcomes)
            .ToArray();
    }

    public RoundEndSection[] BuildSections()
    {
        var summaries = GatherRoundEndSummaries();
        var sections = new List<RoundEndSection>(summaries.Count);

        foreach (var (agent, summary) in summaries)
        {
            var total = 0;
            var totalInCustody = 0;
            foreach (var (_, minds) in summary)
            {
                total += minds.Count;
                totalInCustody += minds.Count(pair => IsInCustody(pair.Item1));
            }

            var result = new StringBuilder();
            result.AppendLine(Loc.GetString("objectives-round-end-result", ("count", total), ("agent", agent)));
            if (agent == Loc.GetString("traitor-round-end-agent-name"))
            {
                result.AppendLine(Loc.GetString("objectives-round-end-result-in-custody", ("count", total), ("custody", totalInCustody), ("agent", agent)));
            }

            foreach (var (prepend, minds) in summary)
            {
                if (!string.IsNullOrEmpty(prepend))
                    result.Append(prepend);

                result.AppendLine();
                AppendMindSummaries(result, agent, minds);
            }

            sections.Add(new RoundEndSection(agent, result.ToString().TrimEnd(), true));
        }

        return sections.ToArray();
    }

    private List<RoundEndTextFragment> BuildPinnedOutcomeFragments(string roundEndText)
    {
        var buildEvent = new RoundEndPinnedOutcomeBuildEvent();
        RaiseLocalEvent(buildEvent);

        var fragments = buildEvent.Fragments.ToList();
        var summaries = GatherRoundEndSummaries();
        if (summaries.Count == 0)
        {
            if (fragments.Count == 0)
                return RoundEndTextFragmentParser.ParseLegacyText(roundEndText).ToList();

            return fragments;
        }

        var paragraphIndex = fragments.Count > 0 ? fragments.Max(fragment => fragment.ParagraphIndex) + 1 : 0;

        foreach (var (agent, summary) in summaries)
        {
            var total = 0;
            var totalInCustody = 0;
            foreach (var (_, minds) in summary)
            {
                total += minds.Count;
                totalInCustody += minds.Count(pair => IsInCustody(pair.Item1));
            }

            fragments.Add(new RoundEndTextFragment(
                Loc.GetString("objectives-round-end-result", ("count", total), ("agent", agent)),
                RoundEndTextFragmentSourceKind.ObjectiveSummary,
                paragraphIndex,
                0,
                true,
                agent));

            var lineIndex = 1;
            if (agent == Loc.GetString("traitor-round-end-agent-name"))
            {
                fragments.Add(new RoundEndTextFragment(
                    Loc.GetString("objectives-round-end-result-in-custody", ("count", total), ("custody", totalInCustody), ("agent", agent)),
                    RoundEndTextFragmentSourceKind.ObjectiveSummary,
                    paragraphIndex,
                    lineIndex++,
                    false,
                    agent));
            }

            foreach (var (prepend, minds) in summary)
            {
                if (!string.IsNullOrWhiteSpace(prepend))
                {
                    foreach (var prependLine in prepend.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    {
                        fragments.Add(new RoundEndTextFragment(
                            prependLine,
                            RoundEndTextFragmentSourceKind.RulePrepend,
                            paragraphIndex,
                            lineIndex++,
                            false,
                            agent));
                    }
                }

                foreach (var (mindId, name) in minds)
                {
                    if (!TryComp<MindComponent>(mindId, out var mind))
                        continue;

                    var title = _objectives.GetTitle((mindId, mind), name);
                    var custody = IsInCustody(mindId, mind) ? Loc.GetString("objectives-in-custody") : string.Empty;

                    var summaryLine = mind.Objectives.Count == 0
                        ? Loc.GetString("objectives-no-objectives", ("custody", custody), ("title", title), ("agent", agent))
                        : Loc.GetString("objectives-with-objectives", ("custody", custody), ("title", title), ("agent", agent));

                    fragments.Add(new RoundEndTextFragment(
                        summaryLine,
                        RoundEndTextFragmentSourceKind.ObjectiveSummary,
                        paragraphIndex,
                        lineIndex++,
                        false,
                        agent));

                    foreach (var objectiveGroup in mind.Objectives.GroupBy(o => Comp<ObjectiveComponent>(o).LocIssuer))
                    {
                        fragments.Add(new RoundEndTextFragment(
                            objectiveGroup.Key,
                            RoundEndTextFragmentSourceKind.ObjectiveIssuer,
                            paragraphIndex,
                            lineIndex++,
                            false,
                            agent));

                        foreach (var objective in objectiveGroup)
                        {
                            var info = _objectives.GetInfo(objective, mindId, mind);
                            if (info == null)
                                continue;

                            fragments.Add(new RoundEndTextFragment(
                                info.Value.Title,
                                RoundEndTextFragmentSourceKind.ObjectiveEntry,
                                paragraphIndex,
                                lineIndex++,
                                false,
                                agent));
                        }
                    }
                }
            }

            paragraphIndex++;
        }

        return fragments;
    }

    private Dictionary<string, Dictionary<string, List<(EntityUid MindId, string Name)>>> GatherRoundEndSummaries()
    {
        var summaries = new Dictionary<string, Dictionary<string, List<(EntityUid, string)>>>();
        var query = EntityQueryEnumerator<GameRuleComponent>();
        while (query.MoveNext(out var uid, out var gameRule))
        {
            if (!_gameTicker.IsGameRuleAdded(uid, gameRule))
                continue;

            var info = new ObjectivesTextGetInfoEvent(new List<(EntityUid, string)>(), string.Empty);
            RaiseLocalEvent(uid, ref info);
            if (info.Minds.Count == 0)
                continue;

            var agent = info.AgentName;
            if (!summaries.TryGetValue(agent, out var byPrepend))
            {
                byPrepend = new Dictionary<string, List<(EntityUid, string)>>();
                summaries[agent] = byPrepend;
            }

            var prepend = new ObjectivesTextPrependEvent(string.Empty);
            RaiseLocalEvent(uid, ref prepend);

            if (!byPrepend.TryGetValue(prepend.Text, out var minds))
            {
                minds = new List<(EntityUid, string)>();
                byPrepend[prepend.Text] = minds;
            }

            minds.AddRange(info.Minds);
        }

        return summaries;
    }

    private void AppendMindSummaries(StringBuilder result, string agent, List<(EntityUid MindId, string Name)> minds)
    {
        var agentSummaries = new List<(string Summary, float SuccessRate, int CompletedObjectives)>();

        foreach (var (mindId, name) in minds)
        {
            if (!TryComp<MindComponent>(mindId, out var mind))
                continue;

            var title = _objectives.GetTitle((mindId, mind), name);
            var custody = IsInCustody(mindId, mind) ? Loc.GetString("objectives-in-custody") : string.Empty;

            if (mind.Objectives.Count == 0)
            {
                agentSummaries.Add((Loc.GetString("objectives-no-objectives", ("custody", custody), ("title", title), ("agent", agent)), 0f, 0));
                continue;
            }

            var completedObjectives = 0;
            var totalObjectives = 0;
            var agentSummary = new StringBuilder();
            agentSummary.AppendLine(Loc.GetString("objectives-with-objectives", ("custody", custody), ("title", title), ("agent", agent)));

            foreach (var objectiveGroup in mind.Objectives.GroupBy(o => Comp<ObjectiveComponent>(o).LocIssuer))
            {
                agentSummary.AppendLine(objectiveGroup.Key);

                foreach (var objective in objectiveGroup)
                {
                    var info = _objectives.GetInfo(objective, mindId, mind);
                    if (info == null)
                        continue;

                    var objectiveTitle = info.Value.Title;
                    var progress = info.Value.Progress;
                    totalObjectives++;

                    agentSummary.Append("- ");
                    if (!_showGreentext)
                    {
                        agentSummary.AppendLine(objectiveTitle);
                    }
                    else if (progress > 0.99f)
                    {
                        agentSummary.AppendLine(Loc.GetString("objectives-objective-success", ("objective", objectiveTitle), ("progress", progress)));
                        completedObjectives++;
                    }
                    else if (progress >= 0.5f)
                    {
                        agentSummary.AppendLine(Loc.GetString("objectives-objective-partial-success", ("objective", objectiveTitle), ("progress", progress)));
                    }
                    else if (progress > 0f)
                    {
                        agentSummary.AppendLine(Loc.GetString("objectives-objective-partial-failure", ("objective", objectiveTitle), ("progress", progress)));
                    }
                    else
                    {
                        agentSummary.AppendLine(Loc.GetString("objectives-objective-fail", ("objective", objectiveTitle), ("progress", progress)));
                    }
                }
            }

            var successRate = totalObjectives > 0 ? (float) completedObjectives / totalObjectives : 0f;
            agentSummaries.Add((agentSummary.ToString(), successRate, completedObjectives));
        }

        foreach (var (summary, _, _) in agentSummaries.OrderByDescending(x => x.SuccessRate).ThenByDescending(x => x.CompletedObjectives))
        {
            result.AppendLine(summary);
        }
    }

    private bool IsInCustody(EntityUid uid, MindComponent? mind = null)
    {
        if (!Resolve(uid, ref mind, false))
            return false;

        if (mind.CurrentEntity == null)
            return false;

        return HasComp<HandcuffComponent>(mind.CurrentEntity);
    }

    public string WrapVanillaObjectivesInSection(string roundEndText, ref List<RoundEndSection> sections)
    {
        // Extract objectives text from vanilla round end text
        var objectivesStart = roundEndText.IndexOf("\n\n", StringComparison.Ordinal);
        if (objectivesStart == -1)
            return roundEndText;

        var objectivesText = roundEndText.Substring(objectivesStart).Trim();
        if (string.IsNullOrEmpty(objectivesText))
            return roundEndText;

        // If we don't have sections yet, create one for vanilla objectives
        if (sections.Count == 0)
        {
            var objectivesSection = new RoundEndSection(
                Loc.GetString("objectives-round-end-title"),
                objectivesText,
                true
            );
            sections.Add(objectivesSection);
        }
        // If we already have sections (Sunrise antags), objectives are already there
        // Just remove them from roundEndText to avoid duplication

        // Remove objectives from round end text in both cases
        return roundEndText.Substring(0, objectivesStart).TrimEnd();
    }
}
