using System;
using System.Threading;
using System.Threading.Tasks;
using Velopack;
using Velopack.Logging;
using Velopack.Sources;

namespace TheLastGate.Updates;

// Plain release download URLs, unlike Velopack's GithubSource, so a check never counts against GitHub's API rate limit.
public sealed class GithubReleaseSource(IFileDownloader downloader) : IUpdateSource
{
    private string channel = "";

    public async Task<VelopackAssetFeed> GetReleaseFeed(
        IVelopackLogger logger,
        string? appId,
        string channel,
        Guid? stagingId = null,
        VelopackAsset? latestLocalRelease = null)
    {
        this.channel = channel;
        var feedUrl = ReleaseUrls.IsTestChannel(channel) ? await NewestTestFeed(channel) : ReleaseUrls.Feed(channel);
        var json = await downloader.DownloadString(feedUrl.AbsoluteUri);
        return VelopackAssetFeed.FromJson(json);
    }

    public Task DownloadReleaseEntry(
        IVelopackLogger logger,
        VelopackAsset releaseEntry,
        string localFile,
        Action<int> progress,
        CancellationToken cancelToken = default)
    {
        var url = ReleaseUrls.Package(channel, releaseEntry.Version.ToString(), releaseEntry.FileName);
        return downloader.DownloadFile(url.AbsoluteUri, localFile, progress, cancelToken: cancelToken);
    }

    // Published releases can't be changed, so there's no rolling test feed; a test build asks the API for the newest
    // test release instead. Real builds never call the API.
    private async Task<Uri> NewestTestFeed(string channel)
    {
        var releases = await downloader.DownloadString(ReleaseUrls.TestReleases.AbsoluteUri);
        var tag = TestReleases.NewestWithFeed(releases, channel)
            ?? throw new InvalidOperationException($"No published test release has a feed for {channel}.");
        return ReleaseUrls.FeedIn(tag, channel);
    }
}
