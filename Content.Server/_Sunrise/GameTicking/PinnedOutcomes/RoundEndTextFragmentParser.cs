namespace Content.Server._Sunrise.GameTicking.PinnedOutcomes;

public static class RoundEndTextFragmentParser
{
    private static readonly string[] ParagraphSeparators = ["\r\n\r\n", "\n\n"];

    public static IEnumerable<RoundEndTextFragment> ParseLegacyText(string roundEndText)
    {
        var paragraphs = roundEndText.Split(ParagraphSeparators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        for (var paragraphIndex = 0; paragraphIndex < paragraphs.Length; paragraphIndex++)
        {
            var paragraph = paragraphs[paragraphIndex];
            var lines = paragraph.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            for (var lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                yield return new RoundEndTextFragment(
                    lines[lineIndex],
                    RoundEndTextFragmentSourceKind.LegacyLine,
                    paragraphIndex,
                    lineIndex,
                    lineIndex == 0);
            }
        }
    }
}
