using Robust.Shared.Serialization;

namespace Content.Shared.GameTicking;

[Serializable, NetSerializable]
public sealed class RoundEndSection
{
    public string Title { get; }
    public string? Text { get; }
    public bool StartCollapsed { get; }

    public RoundEndSection(string title, string? text, bool startCollapsed)
    {
        Title = title;
        Text = text;
        StartCollapsed = startCollapsed;
    }
}
