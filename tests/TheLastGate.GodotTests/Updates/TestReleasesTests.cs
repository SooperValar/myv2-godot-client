using GdUnit4;
using TheLastGate.Updates;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Updates;

[TestSuite]
public class TestReleasesTests
{
    private static string Release(string tag, bool draft, params string[] assets) =>
        $$"""{"tag_name":"{{tag}}","draft":{{(draft ? "true" : "false")}},"prerelease":true,"assets":[{{string.Join(",", assets.Select(asset => $$"""{"name":"{{asset}}"}"""))}}]}""";

    private static string Releases(params string[] releases) => $"[{string.Join(",", releases)}]";

    [TestCase]
    public void NewestWithFeed_PicksTheHighestVersion()
    {
        var json = Releases(
            Release("updater-test-v0.1.9", false, "releases.test-win.json"),
            Release("updater-test-v0.1.10", false, "releases.test-win.json"),
            Release("updater-test-v0.1.2", false, "releases.test-win.json"));

        AssertThat(TestReleases.NewestWithFeed(json, "test-win")).IsEqual("updater-test-v0.1.10");
    }

    [TestCase]
    public void NewestWithFeed_IgnoresReleasesWithoutTheChannelsFeed()
    {
        var json = Releases(
            Release("updater-test-v0.1.1", false, "releases.test-linux.json"),
            Release("updater-test-v0.1.0", false, "releases.test-win.json"));

        AssertThat(TestReleases.NewestWithFeed(json, "test-win")).IsEqual("updater-test-v0.1.0");
    }

    [TestCase]
    public void NewestWithFeed_IgnoresDraftsAndOtherTags()
    {
        var json = Releases(
            Release("updater-test-v0.2.0", true, "releases.test-win.json"),
            Release("v1.0.0", false, "releases.test-win.json"),
            Release("updater-test-v0.1.0", false, "releases.test-win.json"));

        AssertThat(TestReleases.NewestWithFeed(json, "test-win")).IsEqual("updater-test-v0.1.0");
    }

    [TestCase]
    public void NewestWithFeed_IsNullWithoutAnyTestRelease() =>
        AssertThat(TestReleases.NewestWithFeed(Releases(Release("v1.0.0", false, "releases.win.json")), "test-win"))
            .IsNull();
}
