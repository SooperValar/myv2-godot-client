using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TheLastGate.Net;

public sealed class TickPlayback(TickPacer pacer)
{
    private readonly Queue<byte[]> queue = new();

    public int QueuedTicks => queue.Count;

    public void Receive(ReceivedTick tick)
    {
        queue.Enqueue(tick.Payload);
        pacer.OnArrival(tick.ReceivedAtTimestamp * 1000 / Stopwatch.Frequency);
    }

    public void PlayDueTicks(long nowMilliseconds, Action<byte[]> playTick, Action<byte[]> sendCommand)
    {
        while (pacer.IsTickDue(nowMilliseconds))
        {
            if (!queue.TryDequeue(out var payload))
            {
                pacer.TickSkipped();
                continue;
            }

            playTick(payload);
            pacer.TickDone(queue.Count);
            if (TickReport.IsDue(pacer.TicksPlayed))
            {
                sendCommand(TickReport.Create(pacer.TicksPlayed));
            }
        }
    }
}
