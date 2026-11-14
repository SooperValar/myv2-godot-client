using System;
using TheLastGate.Characters;
using TheLastGate.State;

namespace TheLastGate.Net.Handlers;

internal static class LogCommands
{
    private const int ChunkLength = 15;
    private const int MaxPendingLength = 500;
    private const byte Newline = 10;

    public static void Apply(TextLog log, ReadOnlySpan<byte> command, int font)
    {
        var reader = new PacketReader(command);
        reader.Skip(1);
        var chunkStart = log.PendingLength;
        reader.ReadBytes(ChunkLength).CopyTo(log.Pending.AsSpan(chunkStart));

        var newline = log.Pending.AsSpan(chunkStart, ChunkLength).IndexOf(Newline);
        if (newline >= 0)
        {
            var text = NulTerminatedText.Read(log.Pending.AsSpan(0, chunkStart + newline));
            log.PendingLength = 0;
            if (text.Length > 0)
            {
                log.Add(new LogLine(text, font));
            }

            return;
        }

        log.PendingLength = chunkStart + ChunkLength;
        if (log.PendingLength > MaxPendingLength)
        {
            log.PendingLength = 0;
        }
    }
}
