// © SUNRISE, An EULA/CLA with a hosting restriction, full text: https://github.com/space-sunrise/space-station-14/blob/master/CLA.txt

using Content.Shared._Sunrise.Disease;
using Content.Server._Sunrise.GameTicking.PinnedOutcomes;
using Content.Server.Objectives;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Shared.GameTicking.Components;
using Content.Server.Chat.Systems;
using Robust.Shared.Timing;

namespace Content.Server._Sunrise.Disease;

public sealed class DiseaseRuleSystem : GameRuleSystem<DiseaseRuleComponent>
{
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DiseaseRuleComponent, ObjectivesTextGetInfoEvent>(OnObjectivesTextGetInfo);
        SubscribeLocalEvent<DiseaseRuleComponent, RoundEndPinnedOutcomeBuildEvent>(OnBuildPinnedOutcome);
    }

    protected override void Started(EntityUid uid, DiseaseRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);
        
        Timer.Spawn(TimeSpan.FromMinutes(6), () => 
        {
            var message = Loc.GetString("disease-biohazard-announcement");
            var sender = Loc.GetString("disease-biohazard-announcement-sender");
            
            _chatSystem.DispatchGlobalAnnouncement(message, sender, playDefault: true, colorOverride: Color.Red);
        });
    }

    private void OnObjectivesTextGetInfo(EntityUid uid, DiseaseRuleComponent comp, ref ObjectivesTextGetInfoEvent args)
    {
        args.Minds = comp.DiseasesMinds;
        args.AgentName = Loc.GetString("disease-agent-name");
    }

    private void OnBuildPinnedOutcome(EntityUid uid, DiseaseRuleComponent component, RoundEndPinnedOutcomeBuildEvent ev)
    {
        var sick = EntityQueryEnumerator<SickComponent>();
        var immune = EntityQueryEnumerator<DiseaseImmuneComponent>();
        var disease = EntityQueryEnumerator<DiseaseRoleComponent>();
        int infected = 0;
        int immuned = 0;
        int infects = 0;
        while (sick.MoveNext(out _))
        {
            infects++;
        }
        while (immune.MoveNext(out _))
        {
            immuned++;
        }
        while (disease.MoveNext(out var comp))
        {
            infected = comp.SickOfAllTime;
        }

        var result = string.Join("\n",
            Loc.GetString("disease-round-end-result"),
            Loc.GetString("disease-round-end-result-infected", ("count", infected)),
            Loc.GetString("disease-round-end-result-infects", ("count", infects)),
            Loc.GetString("disease-round-end-result-immuned", ("count", immuned)));

        ev.AddFragment(RoundEndPinnedOutcomeBuilder.Explicit(result.Trim(), -2, 0, "disease"));
    }
}
