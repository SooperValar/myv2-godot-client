using System.Diagnostics;
using TheLastGate.Net;
using TheLastGate.Profiling;
using TheLastGate.State;

namespace TheLastGate.Tests;

[Collection(nameof(InstalledTimings))]
public sealed class TimingsWiringTests : IDisposable
{
    private const int ChunkLength = 15;

    public void Dispose() => Timings.Recorder = null;

    [Fact]
    public void ProcessingATick_RecordsTheCoreSections()
    {
        var recorder = new TimingRecorder();
        Timings.Recorder = recorder;
        var processor = new TickProcessor(new GameState());

        processor.Process(TestCommand.Payload(TestCommand.Of(ServerCommand.Log1).Text("Hello.\n", ChunkLength)));
        recorder.EndFrame(Stopwatch.GetTimestamp());

        var names = recorder.Snapshot().Select(row => row.Name).ToList();
        Assert.Contains("tick.process", names);
        Assert.Contains("tick.itemlooks", names);
        Assert.Contains("tick.lookcapture", names);
        Assert.Contains("tick.buffwatch", names);
    }
}
