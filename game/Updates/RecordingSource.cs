using System;
using System.Threading;
using System.Threading.Tasks;
using Velopack;
using Velopack.Logging;
using Velopack.Sources;

namespace TheLastGate.Updates;

public sealed class RecordingSource(IUpdateSource inner) : IUpdateSource
{
    public VelopackAssetFeed? LastFeed { get; private set; }

    public async Task<VelopackAssetFeed> GetReleaseFeed(
        IVelopackLogger logger,
        string? appId,
        string channel,
        Guid? stagingId = null,
        VelopackAsset? latestLocalRelease = null)
    {
        LastFeed = await inner.GetReleaseFeed(logger, appId, channel, stagingId, latestLocalRelease);
        return LastFeed;
    }

    public Task DownloadReleaseEntry(
        IVelopackLogger logger,
        VelopackAsset releaseEntry,
        string localFile,
        Action<int> progress,
        CancellationToken cancelToken = default) =>
        inner.DownloadReleaseEntry(logger, releaseEntry, localFile, progress, cancelToken);
}
