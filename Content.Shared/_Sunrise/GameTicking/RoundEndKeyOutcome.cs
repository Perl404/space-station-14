using Robust.Shared.Serialization;

namespace Content.Shared.GameTicking;

[Serializable, NetSerializable]
public sealed class RoundEndKeyOutcome
{
    public string Text { get; set; }
    public string? Color { get; set; }

    public RoundEndKeyOutcome(string text, string? color = null)
    {
        Text = text;
        Color = color;
    }

    public RoundEndKeyOutcome()
    {
        Text = string.Empty;
    }
}
