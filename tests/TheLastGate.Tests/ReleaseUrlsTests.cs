using TheLastGate.Updates;

namespace TheLastGate.Tests;

public class ReleaseUrlsTests
{
    private const string Repository = "https://github.com/jamesblack/thelastgate-godot";
    private const string FullPackage = "TheLastGateGodot-1.2.0-full.nupkg";

    [Theory]
    [InlineData("win")]
    [InlineData("linux")]
    public void Feed_ProductionUsesTheLatestRelease(string channel)
    {
        Assert.Equal(
            new Uri($"{Repository}/releases/latest/download/releases.{channel}.json"),
            ReleaseUrls.Feed(channel));
    }

    [Fact]
    public void FeedIn_UsesTheGivenRelease()
    {
        Assert.Equal(
            new Uri($"{Repository}/releases/download/updater-test-v0.1.1/releases.test-win.json"),
            ReleaseUrls.FeedIn("updater-test-v0.1.1", "test-win"));
    }

    [Fact]
    public void TestReleases_ListsReleasesThroughTheApi()
    {
        Assert.Equal(
            new Uri("https://api.github.com/repos/jamesblack/thelastgate-godot/releases?per_page=100"),
            ReleaseUrls.TestReleases);
    }

    [Fact]
    public void Package_ProductionUsesTheVersionsTag()
    {
        Assert.Equal(
            new Uri($"{Repository}/releases/download/v1.2.0/{FullPackage}"),
            ReleaseUrls.Package("win", "1.2.0", FullPackage));
    }

    [Fact]
    public void Package_TestUsesTheTestVersionsTag()
    {
        Assert.Equal(
            new Uri($"{Repository}/releases/download/updater-test-v1.2.0/{FullPackage}"),
            ReleaseUrls.Package("test-win", "1.2.0", FullPackage));
    }

    [Theory]
    [InlineData("test-win", true)]
    [InlineData("test-linux", true)]
    [InlineData("win", false)]
    [InlineData("linux", false)]
    public void IsTestChannel_IsRecognisedByPrefix(string channel, bool isTest)
    {
        Assert.Equal(isTest, ReleaseUrls.IsTestChannel(channel));
    }
}
