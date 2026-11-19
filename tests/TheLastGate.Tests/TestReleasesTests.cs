using TheLastGate.Updates;

namespace TheLastGate.Tests;

public class TestReleasesTests
{
    private static string Release(string tag, bool draft, params string[] assets) =>
        $$"""{"tag_name":"{{tag}}","draft":{{(draft ? "true" : "false")}},"prerelease":true,"assets":[{{string.Join(",", assets.Select(asset => $$"""{"name":"{{asset}}"}"""))}}]}""";

    private static string Releases(params string[] releases) => $"[{string.Join(",", releases)}]";

    [Fact]
    public void NewestWithFeed_PicksTheHighestVersion()
    {
        var json = Releases(
            Release("updater-test-v0.1.9", false, "releases.test-win.json"),
            Release("updater-test-v0.1.10", false, "releases.test-win.json"),
            Release("updater-test-v0.1.2", false, "releases.test-win.json"));

        Assert.Equal("updater-test-v0.1.10", TestReleases.NewestWithFeed(json, "test-win"));
    }

    [Fact]
    public void NewestWithFeed_IgnoresReleasesWithoutTheChannelsFeed()
    {
        var json = Releases(
            Release("updater-test-v0.1.1", false, "releases.test-linux.json"),
            Release("updater-test-v0.1.0", false, "releases.test-win.json"));

        Assert.Equal("updater-test-v0.1.0", TestReleases.NewestWithFeed(json, "test-win"));
    }

    [Fact]
    public void NewestWithFeed_IgnoresDraftsAndOtherTags()
    {
        var json = Releases(
            Release("updater-test-v0.2.0", true, "releases.test-win.json"),
            Release("v1.0.0", false, "releases.test-win.json"),
            Release("updater-test-v0.1.0", false, "releases.test-win.json"));

        Assert.Equal("updater-test-v0.1.0", TestReleases.NewestWithFeed(json, "test-win"));
    }

    [Fact]
    public void NewestWithFeed_IsNullWithoutAnyTestRelease()
    {
        Assert.Null(TestReleases.NewestWithFeed(Releases(Release("v1.0.0", false, "releases.win.json")), "test-win"));
    }
}
