using System;

namespace TheLastGate.Net;

public sealed class TickPlayback
{
    public TickPlayback(TickPacer pacer) => throw new NotImplementedException();

    public int QueuedTicks => throw new NotImplementedException();

    public void Receive(ReceivedTick tick) => throw new NotImplementedException();

    public void PlayDueTicks(long nowMilliseconds, Action<byte[]> playTick, Action<byte[]> sendCommand) =>
        throw new NotImplementedException();
}
