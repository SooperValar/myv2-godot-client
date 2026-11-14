using System;

namespace TheLastGate.Net;

public sealed class TickPacer
{
    public const int NominalTickMilliseconds = 20;

    public bool Adaptive
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public int BufferTarget => throw new NotImplementedException();

    public double MeasuredPeriodMilliseconds => throw new NotImplementedException();

    public uint TicksPlayed => throw new NotImplementedException();

    public void OnArrival(long nowMilliseconds) => throw new NotImplementedException();

    public int NextTickMilliseconds(int queuedTicks) => throw new NotImplementedException();

    public bool IsTickDue(long nowMilliseconds) => throw new NotImplementedException();

    public void TickDone(int ticksStillQueued) => throw new NotImplementedException();
}
