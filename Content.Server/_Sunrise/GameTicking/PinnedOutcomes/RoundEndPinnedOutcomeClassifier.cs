namespace Content.Server._Sunrise.GameTicking.PinnedOutcomes;

public static class RoundEndPinnedOutcomeClassifier
{
    public static IEnumerable<(string Text, int Priority)> EnumerateRankedCandidates(string roundEndText)
    {
        return EnumerateRankedCandidates(RoundEndTextFragmentParser.ParseLegacyText(roundEndText));
    }

    public static IEnumerable<(string Text, int Priority)> EnumerateRankedCandidates(IEnumerable<RoundEndTextFragment> fragments)
    {
        foreach (var candidate in SelectPrimaryFragments(fragments))
        {
            var features = RoundEndPinnedOutcomeFeatures.FromFragment(candidate);
            var kind = Classify(features);
            var priority = GetPriority(features, kind);
            if (priority <= 0)
                continue;

            yield return (features.Text, priority);
        }
    }

    public static string? GetColor(int priority)
    {
        return priority switch
        {
            >= 100 => "#d7b84c",
            >= 90 => "#8fd18f",
            >= 70 => "#5e9cff",
            _ => null
        };
    }

    public static RoundEndPinnedOutcomeLineKind Classify(RoundEndPinnedOutcomeFeatures features)
    {
        if (string.IsNullOrWhiteSpace(features.Text))
            return RoundEndPinnedOutcomeLineKind.Noise;

        if (features.Fragment.SourceKind == RoundEndTextFragmentSourceKind.ExplicitOutcome)
            return features.ContainsRoundOutcomeKeyword
                ? RoundEndPinnedOutcomeLineKind.RoundOutcome
                : RoundEndPinnedOutcomeLineKind.StatusSummary;

        if (features.Fragment.SourceKind == RoundEndTextFragmentSourceKind.ObjectiveIssuer)
            return RoundEndPinnedOutcomeLineKind.IssuerLabel;

        if (features.Fragment.SourceKind == RoundEndTextFragmentSourceKind.ObjectiveEntry)
            return RoundEndPinnedOutcomeLineKind.ObjectiveBullet;

        if (features.Fragment.SourceKind == RoundEndTextFragmentSourceKind.ObjectiveSummary)
        {
            if (features.Fragment.IsFirstLineInParagraph && (features.ContainsRoundOutcomeKeyword || features.ContainsStatusKeyword))
                return features.ContainsRoundOutcomeKeyword
                    ? RoundEndPinnedOutcomeLineKind.RoundOutcome
                    : RoundEndPinnedOutcomeLineKind.StatusSummary;

            return RoundEndPinnedOutcomeLineKind.PlayerHeader;
        }

        if (features.StartsWithBullet)
            return RoundEndPinnedOutcomeLineKind.ObjectiveBullet;

        if (features.LooksLikeIssuerLabel && !features.ContainsRoundOutcomeKeyword && !features.ContainsStatusKeyword)
            return RoundEndPinnedOutcomeLineKind.IssuerLabel;

        if ((features.Fragment.SourceKind == RoundEndTextFragmentSourceKind.RulePrepend || features.Fragment.SourceKind == RoundEndTextFragmentSourceKind.LegacyLine)
            && features.ContainsObjectiveKeyword && features.ContainsColon)
            return RoundEndPinnedOutcomeLineKind.ObjectiveIntro;

        if (features.LooksLikePlayerHeader || features.ContainsHandle || features.LooksLikePlayerStatement)
            return RoundEndPinnedOutcomeLineKind.PlayerHeader;

        if (features.ContainsRoundOutcomeKeyword)
            return RoundEndPinnedOutcomeLineKind.RoundOutcome;

        if (features.ContainsStatusKeyword)
            return RoundEndPinnedOutcomeLineKind.StatusSummary;

        if (features.HasColorMarkup || features.Length <= 140)
            return RoundEndPinnedOutcomeLineKind.RoundSummary;

        return RoundEndPinnedOutcomeLineKind.Unknown;
    }

    private static IEnumerable<RoundEndTextFragment> SelectPrimaryFragments(IEnumerable<RoundEndTextFragment> fragments)
    {
        foreach (var fragmentGroup in fragments.GroupBy(fragment => fragment.ParagraphIndex).OrderBy(group => group.Key))
        {
            RoundEndTextFragment? fallback = null;

            foreach (var fragment in fragmentGroup.OrderBy(fragment => fragment.LineIndex))
            {
                var features = RoundEndPinnedOutcomeFeatures.FromFragment(fragment);
                var kind = Classify(features);
                if (!IsPinnedKind(kind))
                    continue;

                if (features.HasColorMarkup)
                {
                    yield return fragment;
                    goto NextParagraph;
                }

                fallback ??= fragment;
            }

            if (fallback is { } selected)
                yield return selected;

            NextParagraph: ;
        }
    }

    private static int GetPriority(RoundEndPinnedOutcomeFeatures features, RoundEndPinnedOutcomeLineKind kind)
    {
        return kind switch
        {
            RoundEndPinnedOutcomeLineKind.RoundOutcome when features.NormalizedText.Contains("major victory") || features.NormalizedText.Contains("разгромная побед") || features.NormalizedText.Contains("крупная побед") => 100,
            RoundEndPinnedOutcomeLineKind.RoundOutcome when features.Fragment.SourceKind == RoundEndTextFragmentSourceKind.ExplicitOutcome => 98,
            RoundEndPinnedOutcomeLineKind.RoundOutcome when features.Fragment.SourceKind == RoundEndTextFragmentSourceKind.ObjectiveSummary && features.Fragment.IsFirstLineInParagraph => 95,
            RoundEndPinnedOutcomeLineKind.RoundOutcome => 90,
            RoundEndPinnedOutcomeLineKind.StatusSummary when features.Fragment.SourceKind == RoundEndTextFragmentSourceKind.ExplicitOutcome => 85,
            RoundEndPinnedOutcomeLineKind.StatusSummary when features.Fragment.SourceKind == RoundEndTextFragmentSourceKind.RulePrepend => 80,
            RoundEndPinnedOutcomeLineKind.StatusSummary when features.Fragment.SourceKind == RoundEndTextFragmentSourceKind.ObjectiveSummary && features.Fragment.IsFirstLineInParagraph => 78,
            RoundEndPinnedOutcomeLineKind.StatusSummary => 75,
            RoundEndPinnedOutcomeLineKind.RoundSummary when features.HasColorMarkup => 65,
            RoundEndPinnedOutcomeLineKind.RoundSummary => 40,
            _ => 0,
        };
    }

    private static bool IsPinnedKind(RoundEndPinnedOutcomeLineKind kind)
    {
        return kind is RoundEndPinnedOutcomeLineKind.RoundOutcome
            or RoundEndPinnedOutcomeLineKind.StatusSummary
            or RoundEndPinnedOutcomeLineKind.RoundSummary;
    }
}
