using GdUnit4;
using TheLastGate.Updates;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Updates;

[TestSuite]
public class ReleaseUrlsTests
{
    private const string Repository = "https://github.com/jamesblack/thelastgate-godot";
    private const string FullPackage = "TheLastGateGodot-1.2.0-full.nupkg";

    [TestCase("win")]
    [TestCase("linux")]
    public void Feed_ProductionUsesTheLatestRelease(string channel) =>
        AssertThat(ReleaseUrls.Feed(channel).AbsoluteUri)
            .IsEqual($"{Repository}/releases/latest/download/releases.{channel}.json");

    [TestCase]
    public void FeedIn_UsesTheGivenRelease() =>
        AssertThat(ReleaseUrls.FeedIn("updater-test-v0.1.1", "test-win").AbsoluteUri)
            .IsEqual($"{Repository}/releases/download/updater-test-v0.1.1/releases.test-win.json");

    [TestCase]
    public void TestReleases_ListsReleasesThroughTheApi() =>
        AssertThat(ReleaseUrls.TestReleases.AbsoluteUri)
            .IsEqual("https://api.github.com/repos/jamesblack/thelastgate-godot/releases?per_page=100");

    [TestCase]
    public void Package_ProductionUsesTheVersionsTag() =>
        AssertThat(ReleaseUrls.Package("win", "1.2.0", FullPackage).AbsoluteUri)
            .IsEqual($"{Repository}/releases/download/v1.2.0/{FullPackage}");

    [TestCase]
    public void Package_TestUsesTheTestVersionsTag() =>
        AssertThat(ReleaseUrls.Package("test-win", "1.2.0", FullPackage).AbsoluteUri)
            .IsEqual($"{Repository}/releases/download/updater-test-v1.2.0/{FullPackage}");

    [TestCase("test-win", true)]
    [TestCase("test-linux", true)]
    [TestCase("win", false)]
    [TestCase("linux", false)]
    public void IsTestChannel_IsRecognisedByPrefix(string channel, bool isTest) =>
        AssertThat(ReleaseUrls.IsTestChannel(channel)).IsEqual(isTest);
}
