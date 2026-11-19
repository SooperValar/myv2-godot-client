using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TheLastGate.Updates;

public sealed class PreviewUpdater(bool failDownload) : IUpdater
{
    private const int CheckDelayMilliseconds = 800;
    private const int DownloadStepMilliseconds = 100;
    private const int DownloadStepPercent = 5;
    private const int FailurePercent = 50;
    private const long FullSizeBytes = 102_538_042;

    private static readonly AvailableRelease[] Releases =
    [
        Release(
            "0.9.0",
            "2026-11-10",
            "### Features\n\n* **launcher:** Remember the last character you played ([1a2b3c4](https://x))"),
        Release("1.0.0", "2026-11-18", "### Features\n\n* Tell players when an update is out ([2b3c4d5](https://x))"),
        Release(
            "1.1.0",
            "2026-11-21",
            "### Features\n\n* **planner:** Add buff watch to the skill planner ([3c4d5e6](https://x))\n\n\n"
            + "### Bug Fixes\n\n* Show black behind tiles out of sight, as the current client does ([4d5e6f7](https://x))"),
        Release(
            "1.2.0",
            "2026-11-24",
            "### Features\n\n* **items:** Show the shop's buy price on item tooltips ([5e6f7a8](https://x))\n"
            + "* **map:** Remember the world map's zoom between sessions ([6f7a8b9](https://x))\n\n\n"
            + "### Bug Fixes\n\n* Stop the chat line losing focus after a server message ([7a8b9c0](https://x))"),
    ];

    public bool IsInstalled => true;

    public Version? CurrentVersion { get; } = new(1, 0, 0);

    public async Task<IReadOnlyList<AvailableRelease>> ListReleases(CancellationToken cancel)
    {
        await Task.Delay(CheckDelayMilliseconds, cancel);
        return Releases;
    }

    public async Task Download(Version target, Action<int> progress, CancellationToken cancel)
    {
        for (var percent = 0; percent <= 100; percent += DownloadStepPercent)
        {
            if (failDownload && percent == FailurePercent)
            {
                throw new HttpRequestException("The preview download failed on purpose.");
            }

            progress(percent);
            await Task.Delay(DownloadStepMilliseconds, cancel);
        }
    }

    public void ApplyAndRestart(Version target) => Applied?.Invoke(target);

    public event Action<Version>? Applied;

    private static AvailableRelease Release(string version, string date, string sections) =>
        new(
            Version.Parse(version),
            $"## [{version}](https://github.com/jamesblack/thelastgate-godot/compare/v0...v{version}) ({date})\n\n\n{sections}\n",
            FullSizeBytes);
}
