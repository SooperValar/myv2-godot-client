using System;
using System.Buffers.Binary;

namespace TheLastGate.Net;

public static class TickReport
{
    private const int Interval = 16;

    public static bool IsDue(uint ticksPlayed) => ticksPlayed > 0 && ticksPlayed % Interval == 0;

    // The server turns a character to stone while this count lags too far behind the ticks it has
    // sent, so it must count ticks played, not ticks received.
    public static byte[] Create(uint ticksPlayed)
    {
        var report = new byte[Protocol.PacketSize];
        report[0] = (byte)ClientCommand.TickReport;
        BinaryPrimitives.WriteUInt32LittleEndian(report.AsSpan(1), ticksPlayed);
        return report;
    }
}
