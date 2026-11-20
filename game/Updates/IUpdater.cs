using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TheLastGate.Updates;

public interface IUpdater
{
    bool IsInstalled { get; }

    Version? CurrentVersion { get; }

    Task<IReadOnlyList<AvailableRelease>> ListReleases(CancellationToken cancel);

    Task Download(Version target, Action<int> progress, CancellationToken cancel);

    // Starts applying once this process has exited; the caller quits straight after.
    void ApplyAndRestart(Version target);
}
