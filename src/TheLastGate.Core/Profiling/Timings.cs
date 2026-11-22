using System.Diagnostics;

namespace TheLastGate.Profiling;

public static class Timings
{
    public static TimingRecorder? Recorder { get; set; }

    public static TimingScope Measure(string section) =>
        Recorder is { } recorder ? new TimingScope(recorder, section, Stopwatch.GetTimestamp()) : default;
}
