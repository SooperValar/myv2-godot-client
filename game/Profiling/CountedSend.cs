using System;
using System.Diagnostics;

namespace TheLastGate.Profiling;

public static class CountedSend
{
    public static Action<byte[]> For(PacketCounter? counter, string sender, Action<byte[]> send)
    {
        if (counter == null)
        {
            return send;
        }

        return packet =>
        {
            counter.Count(sender, packet, Stopwatch.GetTimestamp());
            send(packet);
        };
    }
}
