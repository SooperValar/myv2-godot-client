using System;
using System.Collections.Generic;

namespace TheLastGate.Net;

public static class StatPackets
{
    public const int FirstSkillSlot = 8;

    // The MyV2 server ignores a command asking for more than 99 raises.
    private const int MostRaisesPerCommand = 99;

    public static IReadOnlyList<byte[]> Raise(int slot, int points)
    {
        var packets = new List<byte[]>();
        for (var left = points; left > 0; left -= MostRaisesPerCommand)
        {
            packets.Add(ClientPackets.ShortAndInt(ClientCommand.Stat, slot, Math.Min(left, MostRaisesPerCommand)));
        }

        return packets;
    }
}
