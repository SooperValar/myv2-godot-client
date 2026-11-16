using System;
using System.Collections.Generic;
using System.Text;

namespace TheLastGate.Net;

public static class ChatPackets
{
    private const int ChunkLength = 15;

    private static readonly ClientCommand[] Chunks =
    [
        ClientCommand.Input1, ClientCommand.Input2, ClientCommand.Input3, ClientCommand.Input4,
        ClientCommand.Input5, ClientCommand.Input6, ClientCommand.Input7, ClientCommand.Input8,
    ];

    public static IReadOnlyList<byte[]> Say(string text)
    {
        var letters = Encoding.Latin1.GetBytes(text);
        var packets = new byte[Chunks.Length][];
        for (var chunk = 0; chunk < Chunks.Length; chunk++)
        {
            var packet = new byte[Protocol.PacketSize];
            packet[0] = (byte)Chunks[chunk];
            var start = chunk * ChunkLength;
            if (start < letters.Length)
            {
                letters.AsSpan(start, Math.Min(ChunkLength, letters.Length - start)).CopyTo(packet.AsSpan(1));
            }

            packets[chunk] = packet;
        }

        return packets;
    }
}
