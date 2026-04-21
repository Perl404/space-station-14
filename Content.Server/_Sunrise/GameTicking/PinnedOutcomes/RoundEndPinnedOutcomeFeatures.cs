using System.Text.RegularExpressions;

namespace Content.Server._Sunrise.GameTicking.PinnedOutcomes;

public readonly record struct RoundEndPinnedOutcomeFeatures(
    RoundEndTextFragment Fragment,
    string OriginalText,
    string Text,
    string NormalizedText,
    bool HasColorMarkup,
    bool StartsWithBullet,
    bool ContainsHandle,
    bool ContainsColon,
    bool ContainsRoundOutcomeKeyword,
    bool ContainsStatusKeyword,
    bool ContainsObjectiveKeyword,
    bool LooksLikeIssuerLabel,
    bool LooksLikePlayerHeader,
    bool LooksLikePlayerStatement,
    int Length)
{
    private static readonly Regex MarkupRegex = new("\\[[^\\]]+\\]", RegexOptions.Compiled);
    private static readonly Regex ColorTagRegex = new("\\[color=.*?\\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex UserHandleRegex = new("\\([^)]*@[^)]*\\)", RegexOptions.Compiled);

    private static readonly string[] RoundOutcomeKeywords =
    [
        "major victory", "разгромная побед", "крупная побед",
        "victory", "побед",
        "eradicated", "уничтож", "destroyed",
        "entire crew", "весь экипаж",
        "seized control", "захват"
    ];

    private static readonly string[] StatusKeywords =
    [
        "survived", "выж",
        "shuttle", "шаттл",
        "disk", "диск",
        "nuke", "ядер",
        "crew", "экипаж",
        "station", "станци",
        "zombi", "зомби",
        "arrested", "арест", "custody", "задерж"
    ];

    private static readonly string[] ObjectiveKeywords =
    [
        "objective", "objectives",
        "цель", "цели",
        "codewords", "кодовыми словами",
        "syndicate", "синдикат"
    ];

    public static RoundEndPinnedOutcomeFeatures FromFragment(RoundEndTextFragment fragment)
    {
        var stripped = MarkupRegex.Replace(fragment.Text, string.Empty).Trim();
        var normalized = stripped.ToLowerInvariant();

        return new RoundEndPinnedOutcomeFeatures(
            fragment,
            fragment.Text,
            stripped,
            normalized,
            ColorTagRegex.IsMatch(fragment.Text),
            normalized.StartsWith('-'),
            UserHandleRegex.IsMatch(stripped),
            stripped.Contains(':'),
            ContainsAny(normalized, RoundOutcomeKeywords),
            ContainsAny(normalized, StatusKeywords),
            ContainsAny(normalized, ObjectiveKeywords),
            !normalized.Contains(' ') && stripped.Length > 0,
            UserHandleRegex.IsMatch(stripped) || normalized.Contains(" — ") || normalized.Contains(" - "),
            normalized.Contains(" was ") || normalized.Contains(" был "),
            stripped.Length);
    }

    private static bool ContainsAny(string text, string[] fragments)
    {
        foreach (var fragment in fragments)
        {
            if (text.Contains(fragment))
                return true;
        }

        return false;
    }
}
