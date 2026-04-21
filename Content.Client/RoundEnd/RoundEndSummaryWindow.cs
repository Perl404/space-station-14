using System.Linq;
using System.Numerics;
using Content.Client._Sunrise.StatsBoard;
using Content.Client.Message;
using Content.Shared._Sunrise.StatsBoard;
using Content.Shared.GameTicking;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.Player;
using Robust.Shared.Utility;
using static Robust.Client.UserInterface.Controls.BoxContainer;

namespace Content.Client.RoundEnd
{
    public sealed class RoundEndSummaryWindow : DefaultWindow
    {
        private readonly IEntityManager _entityManager;
        private readonly ISharedPlayerManager _playerManager;
        public int RoundId;

        // Sunrise edit start - add sunrise round end stats dependencies
        public RoundEndSummaryWindow(string gm, string roundEnd, TimeSpan roundTimeSpan, int roundId,
            RoundEndMessageEvent.RoundEndPlayerInfo[] info, string roundEndStats, SharedStatisticEntry[] statisticEntries, RoundEndKeyOutcome[] roundEndKeyOutcomes, RoundEndSection[] roundEndSections, IEntityManager entityManager, ISharedPlayerManager playerManager)
        {
            _entityManager = entityManager;
            _playerManager = playerManager;

            MinSize = SetSize = new Vector2(700, 600);
            // Sunrise edit end

            Title = Loc.GetString("round-end-summary-window-title");

            // The round end window is split into two tabs, one about the round stats
            // and the other is a list of RoundEndPlayerInfo for each player.
            // This tab would be a good place for things like: "x many people died.",
            // "clown slipped the crew x times.", "x shots were fired this round.", etc.
            // Also good for serious info.

            RoundId = roundId;
            var roundEndTabs = new TabContainer();
            // Sunrise edit start - add sunrise round end stats tabs
            roundEndTabs.AddChild(MakeRoundEndStatsTab(roundEndStats));
            roundEndTabs.AddChild(MakeRoundEndMyStatsTab(statisticEntries));
            // Sunrise edit end
            roundEndTabs.AddChild(MakeRoundEndSummaryTab(gm, roundEnd, roundTimeSpan, roundId, roundEndKeyOutcomes, roundEndSections));
            roundEndTabs.AddChild(MakePlayerManifestTab(info));

            ContentsContainer.AddChild(roundEndTabs);

            OpenCenteredRight();
            MoveToFront();
        }

        private BoxContainer MakeRoundEndSummaryTab(string gamemode, string roundEnd, TimeSpan roundDuration, int roundId, RoundEndKeyOutcome[] roundEndKeyOutcomes, RoundEndSection[] roundEndSections)
        {
            var roundEndSummaryTab = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                Name = Loc.GetString("round-end-summary-window-round-end-summary-tab-title")
            };

            var roundEndSummaryContainerScrollbox = new ScrollContainer
            {
                VerticalExpand = true,
                Margin = new Thickness(10)
            };
            var roundEndSummaryContainer = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical
            };

            //Gamemode Name
            var gamemodeLabel = new RichTextLabel();
            var gamemodeMessage = new FormattedMessage();
            gamemodeMessage.AddMarkupOrThrow(Loc.GetString("round-end-summary-window-round-id-label", ("roundId", roundId)));
            gamemodeMessage.AddText(" ");
            gamemodeMessage.AddMarkupOrThrow(Loc.GetString("round-end-summary-window-gamemode-name-label", ("gamemode", gamemode)));
            gamemodeLabel.SetMessage(gamemodeMessage);
            roundEndSummaryContainer.AddChild(gamemodeLabel);

            //Duration
            var roundTimeLabel = new RichTextLabel();
            roundTimeLabel.SetMarkup(Loc.GetString("round-end-summary-window-duration-label",
                                                   ("hours", roundDuration.Hours),
                                                   ("minutes", roundDuration.Minutes),
                                                   ("seconds", roundDuration.Seconds)));
            roundEndSummaryContainer.AddChild(roundTimeLabel);

            if (roundEndKeyOutcomes.Length > 0)
            {
                roundEndSummaryContainer.AddChild(MakeKeyOutcomesBlock(roundEndKeyOutcomes));
            }

            //Round end text
            if (!string.IsNullOrEmpty(roundEnd))
            {
                var roundEndLabel = new RichTextLabel
                {
                    Margin = new Thickness(0, 6, 0, 0)
                };
                roundEndLabel.SetMarkup(roundEnd);
                roundEndSummaryContainer.AddChild(roundEndLabel);
            }

            // Sunrise added start - render round end sections
            foreach (var section in roundEndSections)
            {
                roundEndSummaryContainer.AddChild(MakeRoundEndSection(section));
            }
            // Sunrise added end

            roundEndSummaryContainerScrollbox.AddChild(roundEndSummaryContainer);
            roundEndSummaryTab.AddChild(roundEndSummaryContainerScrollbox);

            return roundEndSummaryTab;
        }

        private Control MakeKeyOutcomesBlock(RoundEndKeyOutcome[] outcomes)
        {
            var container = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                Margin = new Thickness(0, 8, 0, 4)
            };

            var title = new RichTextLabel();
            title.SetMarkup(Loc.GetString("round-end-summary-window-key-outcomes-title"));
            container.AddChild(title);

            foreach (var outcome in outcomes)
            {
                var line = new RichTextLabel
                {
                    Margin = new Thickness(8, 2, 0, 0)
                };

                var color = string.IsNullOrWhiteSpace(outcome.Color) ? "white" : outcome.Color;
                line.SetMarkup(Loc.GetString("round-end-summary-window-key-outcomes-entry", ("color", color), ("text", outcome.Text)));
                container.AddChild(line);
            }

            return container;
        }

        // Sunrise added start - helper to create round end sections
        private Control MakeRoundEndSection(RoundEndSection section)
        {
            var heading = new CollapsibleHeading(section.Title)
            {
                ChevronMargin = new Thickness(6, 0, 10, 0),
                MinHeight = 28,
                Margin = new Thickness(0, 6, 0, 0)
            };

            var body = new CollapsibleBody
            {
                Margin = new Thickness(12, 4, 0, 2)
            };

            var text = new RichTextLabel();
            if (!string.IsNullOrWhiteSpace(section.Text))
                text.SetMarkup(section.Text);

            body.AddChild(text);

            var collapsible = new Collapsible(heading, body)
            {
                BodyVisible = !section.StartCollapsed
            };

            heading.OnPressed += _ => collapsible.BodyVisible = !collapsible.BodyVisible;
            return collapsible;
        }
        // Sunrise added end

        private BoxContainer MakePlayerManifestTab(RoundEndMessageEvent.RoundEndPlayerInfo[] playersInfo)
        {
            var playerManifestTab = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                Name = Loc.GetString("round-end-summary-window-player-manifest-tab-title")
            };

            var playerInfoContainerScrollbox = new ScrollContainer
            {
                VerticalExpand = true,
                Margin = new Thickness(10)
            };
            var playerInfoContainer = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical
            };

            //Put observers at the bottom of the list. Put antags on top.
            var sortedPlayersInfo = playersInfo.OrderBy(p => p.Observer).ThenBy(p => !p.Antag);

            //Create labels for each player info.
            foreach (var playerInfo in sortedPlayersInfo)
            {
                var hBox = new BoxContainer
                {
                    Orientation = LayoutOrientation.Horizontal,
                };

                var playerInfoText = new RichTextLabel
                {
                    VerticalAlignment = VAlignment.Center,
                    VerticalExpand = true,
                };

                if (playerInfo.PlayerNetEntity != null)
                {
                    hBox.AddChild(new SpriteView(playerInfo.PlayerNetEntity.Value, _entityManager)
                        {
                            OverrideDirection = Direction.South,
                            VerticalAlignment = VAlignment.Center,
                            SetSize = new Vector2(32, 32),
                            VerticalExpand = true,
                        });
                }

                if (playerInfo.PlayerICName != null)
                {
                    if (playerInfo.Observer)
                    {
                        playerInfoText.SetMarkup(
                            Loc.GetString("round-end-summary-window-player-info-if-observer-text",
                                          ("playerOOCName", playerInfo.PlayerOOCName),
                                          ("playerICName", playerInfo.PlayerICName)));
                    }
                    else
                    {
                        //TODO: On Hover display a popup detailing more play info.
                        //For example: their antag goals and if they completed them sucessfully.
                        var icNameColor = playerInfo.Antag ? "red" : "white";
                        playerInfoText.SetMarkup(
                            Loc.GetString("round-end-summary-window-player-info-if-not-observer-text",
                                ("playerOOCName", playerInfo.PlayerOOCName),
                                ("icNameColor", icNameColor),
                                ("playerICName", playerInfo.PlayerICName),
                                ("playerRole", Loc.GetString(playerInfo.Role))));
                    }
                }
                hBox.AddChild(playerInfoText);
                playerInfoContainer.AddChild(hBox);
            }

            playerInfoContainerScrollbox.AddChild(playerInfoContainer);
            playerManifestTab.AddChild(playerInfoContainerScrollbox);

            return playerManifestTab;
        }

        // Sunrise added start - round end stats tabs
        private BoxContainer MakeRoundEndStatsTab(string stats)
        {
            var roundEndSummaryTab = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                Name = Loc.GetString("round-end-summary-window-stats-tab-title")
            };

            var roundEndSummaryContainerScrollbox = new ScrollContainer
            {
                VerticalExpand = true,
                Margin = new Thickness(10)
            };
            var roundEndSummaryContainer = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical
            };

            //Round end text
            if (!string.IsNullOrEmpty(stats))
            {
                var statsLabel = new RichTextLabel();
                statsLabel.SetMarkup(stats);
                roundEndSummaryContainer.AddChild(statsLabel);
            }

            roundEndSummaryContainerScrollbox.AddChild(roundEndSummaryContainer);
            roundEndSummaryTab.AddChild(roundEndSummaryContainerScrollbox);

            return roundEndSummaryTab;
        }

        private BoxContainer MakeRoundEndMyStatsTab(SharedStatisticEntry[] statisticEntries)
        {
            var roundEndSummaryTab = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                Name = Loc.GetString("round-end-summary-window-my-stats-tab-title")
            };

            var roundEndSummaryContainerScrollbox = new ScrollContainer
            {
                VerticalExpand = true,
                Margin = new Thickness(10),
            };

            var statsEntries = new StatsEntries();
            foreach (var statisticEntry in statisticEntries)
            {
                if (statisticEntry.FirstActor != _playerManager.LocalSession!.UserId)
                    continue;

                var statsEntry = new StatsEntry(statisticEntry.Name, statisticEntry.TotalTakeDamage,
                    statisticEntry.TotalTakeHeal, statisticEntry.TotalInflictedDamage,
                    statisticEntry.TotalInflictedHeal, statisticEntry.SlippedCount,
                    statisticEntry.CreamedCount, statisticEntry.DoorEmagedCount, statisticEntry.ElectrocutedCount,
                    statisticEntry.CuffedCount, statisticEntry.AbsorbedPuddleCount, statisticEntry.SpentTk ?? 0,
                    statisticEntry.DeadCount, statisticEntry.HumanoidKillCount, statisticEntry.KilledMouseCount,
                    statisticEntry.CuffedTime, statisticEntry.SpaceTime, statisticEntry.SleepTime,
                    statisticEntry.IsInteractedCaptainCard ? Loc.GetString("accept-cloning-window-accept-button") : Loc.GetString("accept-cloning-window-deny-button"));
                statsEntries.AddEntry(statsEntry);
            }

            roundEndSummaryContainerScrollbox.AddChild(statsEntries);
            roundEndSummaryTab.AddChild(roundEndSummaryContainerScrollbox);

            return roundEndSummaryTab;
        }
        // Sunrise added end
    }

}
