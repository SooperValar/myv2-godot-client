using System.Diagnostics;

namespace TheLastGate.Profiling;

public readonly ref struct TimingScope(TimingRecorder recorder, string section, long startedAt)
{
    public void Dispose() => recorder?.Record(section, Stopwatch.GetTimestamp() - startedAt);
}
