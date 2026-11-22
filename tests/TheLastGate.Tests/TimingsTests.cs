using System.Diagnostics;
using TheLastGate.Profiling;

namespace TheLastGate.Tests;

[CollectionDefinition(nameof(InstalledTimings), DisableParallelization = true)]
public sealed class InstalledTimings;

[Collection(nameof(InstalledTimings))]
public sealed class TimingsTests : IDisposable
{
    public void Dispose() => Timings.Recorder = null;

    [Fact]
    public void MeasureWithNoRecorder_RecordsNothing()
    {
        using (Timings.Measure("anything"))
        {
        }

        var recorder = new TimingRecorder();
        Timings.Recorder = recorder;
        recorder.EndFrame(Stopwatch.GetTimestamp());

        Assert.Empty(recorder.Snapshot());
    }

    [Fact]
    public void Measure_RecordsIntoTheInstalledRecorder()
    {
        var recorder = new TimingRecorder();
        Timings.Recorder = recorder;
        using (Timings.Measure("anything"))
        {
            Thread.Sleep(1);
        }

        recorder.EndFrame(Stopwatch.GetTimestamp());

        var row = Assert.Single(recorder.Snapshot());
        Assert.Equal(1, row.CallsPerFrame);
        Assert.True(row.AverageFrameMilliseconds > 0);
    }
}
