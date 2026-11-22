using System.Diagnostics;
using TheLastGate.Profiling;

namespace TheLastGate.Tests;

public class TimingRecorderTests
{
    private static readonly long Millisecond = Stopwatch.Frequency / 1000;
    private readonly TimingRecorder recorder = new();
    private long now;

    [Fact]
    public void AFramesSections_AddUpPerFrame()
    {
        recorder.Record("tooltip.update", 2 * Millisecond);
        recorder.Record("tooltip.update", 2 * Millisecond);
        EndFrame();

        var row = Assert.Single(recorder.Snapshot());
        Assert.Equal("tooltip.update", row.Name);
        Assert.Equal(2, row.CallsPerFrame);
        Assert.Equal(4, row.AverageFrameMilliseconds, 3);
        Assert.Equal(4, row.PeakFrameMilliseconds, 3);
        Assert.Null(row.AverageTickMilliseconds);
        Assert.Null(row.PeakTickMilliseconds);
    }

    [Fact]
    public void AverageAndPeak_CoverTheWindow()
    {
        foreach (var milliseconds in (int[])[2, 6, 4])
        {
            recorder.Record("world.refresh", milliseconds * Millisecond);
            EndFrame();
        }

        var row = Assert.Single(recorder.Snapshot());
        Assert.Equal(4, row.AverageFrameMilliseconds, 3);
        Assert.Equal(6, row.PeakFrameMilliseconds, 3);
    }

    [Fact]
    public void FramesOlderThanTheWindow_DropOut()
    {
        recorder.Record("world.refresh", 10 * Millisecond);
        EndFrame();
        while (now < 2500 * Millisecond)
        {
            EndFrame();
        }

        Assert.Empty(recorder.Snapshot());
    }

    [Fact]
    public void ASectionThatStops_FallsToAZeroAverage()
    {
        recorder.Record("tooltip.update", 6 * Millisecond);
        EndFrame();
        EndFrame();
        EndFrame();
        EndFrame();

        var row = Assert.Single(recorder.Snapshot());
        Assert.Equal(1.5, row.AverageFrameMilliseconds, 3);
        Assert.Equal(6, row.PeakFrameMilliseconds, 3);
    }

    [Fact]
    public void SectionsInsideATick_GetTickTimings()
    {
        for (var tick = 0; tick < 2; tick++)
        {
            recorder.BeginTick();
            recorder.Record("tick.process", 3 * Millisecond);
            recorder.EndTick();
        }

        EndFrame();

        var row = Assert.Single(recorder.Snapshot());
        Assert.Equal(3, row.AverageTickMilliseconds!.Value, 3);
        Assert.Equal(3, row.PeakTickMilliseconds!.Value, 3);
        Assert.Equal(6, row.AverageFrameMilliseconds, 3);
    }

    [Fact]
    public void NestedSections_KeepTheirOwnTime()
    {
        recorder.Record("tick.itemlooks", 2 * Millisecond);
        recorder.Record("tick.process", 5 * Millisecond);
        EndFrame();

        var rows = recorder.Snapshot().ToDictionary(row => row.Name);
        Assert.Equal(5, rows["tick.process"].AverageFrameMilliseconds, 3);
        Assert.Equal(2, rows["tick.itemlooks"].AverageFrameMilliseconds, 3);
    }

    [Fact]
    public void EndFrame_ReportsTheTimeSinceTheLastFrame()
    {
        recorder.EndFrame(10 * Millisecond);
        recorder.Record("world.refresh", 9 * Millisecond);
        recorder.Record("tick.process", 41 * Millisecond);

        var frame = recorder.EndFrame(70 * Millisecond);

        Assert.Equal(60, frame.FrameMilliseconds, 3);
        Assert.Equal(["tick.process", "world.refresh"], frame.Sections.Select(section => section.Name));
        Assert.Equal(41, frame.Sections[0].Milliseconds, 3);
    }

    [Fact]
    public void RecordsFromAnotherThread_AreIgnored()
    {
        var background = new Thread(() => recorder.Record("planner.focus", 5 * Millisecond));
        background.Start();
        background.Join();
        EndFrame();

        Assert.Empty(recorder.Snapshot());
    }

    [Fact]
    public void AverageFrameMilliseconds_ReadsOneSection()
    {
        recorder.Record("chat.draw", 3 * Millisecond);
        EndFrame();

        Assert.Equal(3, recorder.AverageFrameMilliseconds("chat.draw"), 3);
        Assert.Equal(0, recorder.AverageFrameMilliseconds("never.ran"));
    }

    [Fact]
    public void WholeFrame_CoversTheTimeBetweenFramesInTheWindow()
    {
        recorder.EndFrame(0);
        recorder.EndFrame(10 * Millisecond);
        recorder.EndFrame(40 * Millisecond);

        var frame = recorder.WholeFrame();

        Assert.Equal("frame", frame.Name);
        Assert.Equal(40.0 / 3, frame.AverageFrameMilliseconds, 3);
        Assert.Equal(30, frame.PeakFrameMilliseconds, 3);
    }

    private void EndFrame()
    {
        now += 16 * Millisecond;
        recorder.EndFrame(now);
    }
}
