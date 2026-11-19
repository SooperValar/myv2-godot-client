using System;
using System.Linq;
using System.Text.Json;

namespace TheLastGate.Updates;

public static class TestReleases
{
    public static string? NewestWithFeed(string releasesJson, string channel)
    {
        var feedName = $"releases.{channel}.json";
        using var releases = JsonDocument.Parse(releasesJson);
        string? newestTag = null;
        Version? newestVersion = null;

        foreach (var release in releases.RootElement.EnumerateArray())
        {
            var tag = release.GetProperty("tag_name").GetString() ?? "";
            var isPublishedTest = !release.GetProperty("draft").GetBoolean()
                && tag.StartsWith(ReleaseUrls.TestTagPrefix, StringComparison.Ordinal);
            if (!isPublishedTest
                || !Version.TryParse(tag[ReleaseUrls.TestTagPrefix.Length..], out var version)
                || !HasAsset(release, feedName)
                || version <= newestVersion)
            {
                continue;
            }

            newestTag = tag;
            newestVersion = version;
        }

        return newestTag;
    }

    private static bool HasAsset(JsonElement release, string name) =>
        release.GetProperty("assets").EnumerateArray().Any(asset => asset.GetProperty("name").GetString() == name);
}
