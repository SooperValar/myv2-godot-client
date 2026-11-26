using System.Net;
using GdUnit4;
using TheLastGate.Updates;
using Velopack.Logging;
using Velopack.Sources;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Updates;

[TestSuite]
public class GithubReleaseSourceTests
{
    private const string FeedJson = """
        {"Assets":[{"PackageId":"TheLastGateGodot","Version":"1.2.0","Type":"Full",
        "FileName":"TheLastGateGodot-1.2.0-full.nupkg","SHA1":"AB","SHA256":"CD","Size":5,
        "NotesMarkdown":"## 1.2.0"}]}
        """;

    [TestCase]
    public async Task GetReleaseFeed_FetchesTheChannelsFeed()
    {
        var downloader = new RecordingDownloader(FeedJson);

        var feed = await new GithubReleaseSource(downloader)
            .GetReleaseFeed(new NullVelopackLogger(), "TheLastGateGodot", "win");

        AssertThat(downloader.Urls)
            .ContainsExactly(
                "https://github.com/SooperValar/myv2-godot-client/releases/latest/download/releases.win.json");
        AssertThat(feed.Assets).HasSize(1);
        AssertThat(feed.Assets[0].FileName).IsEqual("TheLastGateGodot-1.2.0-full.nupkg");
    }

    [TestCase]
    public async Task GetReleaseFeed_TestChannelReadsTheNewestTestReleasesFeed()
    {
        var downloader = new RecordingDownloader(
            FeedJson,
            """[{"tag_name":"updater-test-v0.1.1","draft":false,"assets":[{"name":"releases.test-win.json"}]}]""");

        await new GithubReleaseSource(downloader).GetReleaseFeed(
            new NullVelopackLogger(),
            "TheLastGateGodot",
            "test-win");

        AssertThat(downloader.Urls)
            .ContainsExactly(
                "https://api.github.com/repos/SooperValar/myv2-godot-client/releases?per_page=100",
                "https://github.com/SooperValar/myv2-godot-client/releases/download/updater-test-v0.1.1/releases.test-win.json");
    }

    [TestCase]
    public async Task DownloadReleaseEntry_DownloadsFromTheVersionsTag()
    {
        var downloader = new RecordingDownloader(FeedJson);
        var source = new GithubReleaseSource(downloader);
        var feed = await source.GetReleaseFeed(new NullVelopackLogger(), "TheLastGateGodot", "win");

        await source.DownloadReleaseEntry(
            new NullVelopackLogger(),
            feed.Assets[0],
            "local.nupkg",
            _ => { },
            CancellationToken.None);

        AssertThat(downloader.Urls[^1])
            .IsEqual(
                "https://github.com/SooperValar/myv2-godot-client/releases/download/v1.2.0/TheLastGateGodot-1.2.0-full.nupkg");
    }

    [TestCase]
    public async Task GetReleaseFeed_Feed404IsACheckFailure()
    {
        var downloader = new RecordingDownloader(new HttpRequestException("Not found", null, HttpStatusCode.NotFound));

        var failure = await AssertThrown(
            new GithubReleaseSource(downloader).GetReleaseFeed(new NullVelopackLogger(), "TheLastGateGodot", "win"));

        failure!.IsInstanceOf<HttpRequestException>().HasPropertyValue("StatusCode", HttpStatusCode.NotFound);
    }

    [TestCase]
    public async Task RecordingSource_KeepsTheLastFeed()
    {
        var source = new RecordingSource(new GithubReleaseSource(new RecordingDownloader(FeedJson)));

        await source.GetReleaseFeed(new NullVelopackLogger(), "TheLastGateGodot", "win");

        AssertThat(source.LastFeed!.Assets).HasSize(1);
        AssertThat(source.LastFeed.Assets[0].FileName).IsEqual("TheLastGateGodot-1.2.0-full.nupkg");
    }

    private sealed class RecordingDownloader : IFileDownloader
    {
        private readonly HttpRequestException? failure;
        private readonly string releasesJson = "[]";
        private readonly string text = "";

        public RecordingDownloader(string text, string releasesJson = "[]")
        {
            this.text = text;
            this.releasesJson = releasesJson;
        }

        public RecordingDownloader(HttpRequestException failure) => this.failure = failure;

        public List<string> Urls { get; } = [];

        public Task DownloadFile(
            string url,
            string targetFile,
            Action<int> progress,
            IDictionary<string, string>? headers = null,
            double timeout = 30,
            CancellationToken cancelToken = default)
        {
            Urls.Add(url);
            return failure is null ? Task.CompletedTask : Task.FromException(failure);
        }

        public Task<byte[]> DownloadBytes(string url, IDictionary<string, string>? headers = null, double timeout = 30)
        {
            Urls.Add(url);
            return failure is null ? Task.FromResult(Array.Empty<byte>()) : Task.FromException<byte[]>(failure);
        }

        public Task<string> DownloadString(string url, IDictionary<string, string>? headers = null, double timeout = 30)
        {
            Urls.Add(url);
            var reply = url.StartsWith("https://api.github.com/", StringComparison.Ordinal) ? releasesJson : text;
            return failure is null ? Task.FromResult(reply) : Task.FromException<string>(failure);
        }
    }
}
