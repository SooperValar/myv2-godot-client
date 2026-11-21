using System;
using System.Collections.Generic;

namespace TheLastGate.Net;

public static class StatPackets
{
    public const int FirstSkillSlot = 8;

    private const int MostSkillPointsPerCommand = 135;
    private const int MostPointsPerCommand = 900;

    public static IReadOnlyList<byte[]> Raise(int slot, int points)
    {
        var mostPerCommand = slot >= FirstSkillSlot ? MostSkillPointsPerCommand : MostPointsPerCommand;
        var packets = new List<byte[]>();
        for (var left = points; left > 0; left -= mostPerCommand)
        {
            packets.Add(ClientPackets.ShortAndInt(ClientCommand.Stat, slot, Math.Min(left, mostPerCommand)));
        }

        return packets;
    }
}
