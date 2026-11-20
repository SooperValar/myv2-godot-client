using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Velopack;
using Velopack.Sources;

namespace TheLastGate.Updates;

public sealed class VelopackUpdater : IUpdater
{
    private readonly UpdateManager manager;
    private readonly RecordingSource source;
    private UpdateInfo? update;

    public VelopackUpdater(IUpdateSource source)
    {
        this.source = new RecordingSource(source);
        manager = new UpdateManager(this.source, new UpdateOptions { AllowVersionDowngrade = true });
    }

    public bool IsInstalled => manager.IsInstalled;

    public Version? CurrentVersion => manager.CurrentVersion is { } version ? AsVersion(version) : null;

    public async Task<IReadOnlyList<AvailableRelease>> ListReleases(CancellationToken cancel)
    {
        update = await manager.CheckForUpdatesAsync();
        return FullReleases()
            .Select(asset => new AvailableRelease(AsVersion(asset.Version), asset.NotesMarkdown ?? "", asset.Size))
            .ToArray();
    }

    public Task Download(Version target, Action<int> progress, CancellationToken cancel)
    {
        var download = update is not null && AsVersion(update.TargetFullRelease.Version) == target
            ? update
            : new UpdateInfo(FullRelease(target), true, null, []);
        return manager.DownloadUpdatesAsync(download, progress, cancel);
    }

    // Velopack's ApplyUpdatesAndRestart exits the process itself, which crashes Godot, so the caller quits instead.
    public void ApplyAndRestart(Version target) =>
        manager.WaitExitThenApplyUpdates(FullRelease(target), false, true, []);

    private IEnumerable<VelopackAsset> FullReleases() =>
        source.LastFeed?.Assets.Where(asset => asset.Type == VelopackAssetType.Full) ?? [];

    private VelopackAsset FullRelease(Version target) =>
        FullReleases().First(asset => AsVersion(asset.Version) == target);

    private static Version AsVersion(SemanticVersion version) =>
        new(version.Major, version.Minor, version.Patch);
}
