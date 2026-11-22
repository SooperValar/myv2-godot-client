using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TheLastGate.Profiling;

public sealed class TimingRecorder(double windowSeconds = 2)
{
    public const string WholeFrameName = "frame";
    private readonly Queue<ClosedFrame> frames = new();
    private readonly Dictionary<string, SectionTotal> openFrame = [];
    private readonly Dictionary<string, double> openTick = [];
    private readonly int ownerThread = Environment.CurrentManagedThreadId;
    private readonly List<Dictionary<string, double>> ticksThisFrame = [];
    private readonly long windowTimestamp = (long)(windowSeconds * Stopwatch.Frequency);
    private bool inTick;
    private long? lastFrameAt;
    private IReadOnlyList<SectionTiming>? snapshot;

    public void Record(string section, long elapsedTimestamp)
    {
        if (!OnOwnerThread())
        {
            return;
        }

        var milliseconds = ToMilliseconds(elapsedTimestamp);
        var total = openFrame.GetValueOrDefault(section);
        openFrame[section] = new SectionTotal(total.Milliseconds + milliseconds, total.Calls + 1);
        if (inTick)
        {
            openTick[section] = openTick.GetValueOrDefault(section) + milliseconds;
        }
    }

    public void BeginTick()
    {
        if (OnOwnerThread())
        {
            inTick = true;
        }
    }

    public void EndTick()
    {
        if (!OnOwnerThread() || !inTick)
        {
            return;
        }

        inTick = false;
        ticksThisFrame.Add(new Dictionary<string, double>(openTick));
        openTick.Clear();
    }

    public FrameSummary EndFrame(long nowTimestamp)
    {
        var frameMilliseconds = lastFrameAt is { } last ? ToMilliseconds(nowTimestamp - last) : 0;
        lastFrameAt = nowTimestamp;
        var sections = openFrame
            .OrderByDescending(entry => entry.Value.Milliseconds)
            .Select(entry => (entry.Key, entry.Value.Milliseconds))
            .ToList();
        frames.Enqueue(
            new ClosedFrame(
                nowTimestamp,
                frameMilliseconds,
                new Dictionary<string, SectionTotal>(openFrame),
                [.. ticksThisFrame]));
        openFrame.Clear();
        ticksThisFrame.Clear();
        while (frames.Peek().At <= nowTimestamp - windowTimestamp)
        {
            frames.Dequeue();
        }

        snapshot = null;
        return new FrameSummary(frameMilliseconds, sections);
    }

    public SectionTiming WholeFrame()
    {
        var times = frames.Select(frame => frame.Milliseconds).DefaultIfEmpty().ToList();
        return new SectionTiming(WholeFrameName, 1, times.Average(), times.Max(), null, null);
    }

    public IReadOnlyList<SectionTiming> Snapshot() => snapshot ??= BuildSnapshot();

    public double AverageFrameMilliseconds(string section) =>
        Snapshot().FirstOrDefault(row => row.Name == section)?.AverageFrameMilliseconds ?? 0;

    private IReadOnlyList<SectionTiming> BuildSnapshot()
    {
        var frameCount = frames.Count;
        var ticks = frames.SelectMany(frame => frame.Ticks).ToList();
        return frames
            .SelectMany(frame => frame.Sections.Keys)
            .Distinct()
            .Select(name => Timing(name, frameCount, ticks))
            .OrderByDescending(row => row.PeakFrameMilliseconds)
            .ToList();
    }

    private SectionTiming Timing(string name, int frameCount, List<Dictionary<string, double>> ticks)
    {
        var totals = frames.Select(frame => frame.Sections.GetValueOrDefault(name)).ToList();
        var tickTimes = ticks.Select(tick => tick.GetValueOrDefault(name, -1)).ToList();
        var ranInATick = tickTimes.Any(milliseconds => milliseconds >= 0);
        return new SectionTiming(
            name,
            (double)totals.Sum(total => total.Calls) / frameCount,
            totals.Sum(total => total.Milliseconds) / frameCount,
            totals.Max(total => total.Milliseconds),
            ranInATick ? tickTimes.Sum(milliseconds => Math.Max(milliseconds, 0)) / tickTimes.Count : null,
            ranInATick ? tickTimes.Max() : null);
    }

    private bool OnOwnerThread() => Environment.CurrentManagedThreadId == ownerThread;

    private static double ToMilliseconds(long timestamp) => timestamp * 1000.0 / Stopwatch.Frequency;

    private readonly record struct SectionTotal(double Milliseconds, int Calls);

    private sealed record ClosedFrame(
        long At,
        double Milliseconds,
        Dictionary<string, SectionTotal> Sections,
        List<Dictionary<string, double>> Ticks);
}
