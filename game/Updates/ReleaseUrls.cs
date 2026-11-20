using System;

namespace TheLastGate.Updates;

public static class ReleaseUrls
{
    public const string Repository = "https://github.com/jamesblack/thelastgate-godot";
    public const string TestChannelPrefix = "test-";
    public const string TestTagPrefix = "updater-test-v";

    private const string ProductionTagPrefix = "v";

    public static Uri TestReleases { get; } =
        new("https://api.github.com/repos/jamesblack/thelastgate-godot/releases?per_page=100");

    public static bool IsTestChannel(string channel) => channel.StartsWith(TestChannelPrefix, StringComparison.Ordinal);

    public static Uri Feed(string channel) => new($"{Repository}/releases/latest/download/releases.{channel}.json");

    public static Uri FeedIn(string tag, string channel) =>
        new($"{Repository}/releases/download/{tag}/releases.{channel}.json");

    public static Uri Package(string channel, string version, string fileName)
    {
        var tagPrefix = IsTestChannel(channel) ? TestTagPrefix : ProductionTagPrefix;
        return new Uri($"{Repository}/releases/download/{tagPrefix}{version}/{fileName}");
    }
}
