using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using TheLastGate.Net;

namespace TheLastGate.Profiling;

public sealed class PacketCounter(double windowSeconds = 2)
{
    private const string TextType = "Input (text)";

    private readonly Queue<(long At, (string Sender, string Type) Key)> recent = new();
    private readonly HashSet<string> senders = [];
    private readonly Dictionary<(string Sender, string Type), long> totals = [];
    private readonly long windowTimestamp = (long)(windowSeconds * Stopwatch.Frequency);

    public IReadOnlyCollection<string> Senders => senders;

    public void Count(string sender, ReadOnlySpan<byte> packet, long nowTimestamp)
    {
        var key = (sender, TypeName(packet[0]));
        totals[key] = totals.GetValueOrDefault(key) + 1;
        senders.Add(sender);
        recent.Enqueue((nowTimestamp, key));
    }

    public IReadOnlyList<PacketTraffic> Snapshot(long nowTimestamp)
    {
        DropOld(nowTimestamp);
        var inWindow = recent.GroupBy(entry => entry.Key).ToDictionary(group => group.Key, group => group.Count());
        return totals
            .Select(entry => new PacketTraffic(
                entry.Key.Sender,
                entry.Key.Type,
                inWindow.GetValueOrDefault(entry.Key) / windowSeconds,
                entry.Value))
            .OrderByDescending(row => row.PerSecond)
            .ThenByDescending(row => row.Total)
            .ToList();
    }

    public double PerSecond(string sender, long nowTimestamp)
    {
        DropOld(nowTimestamp);
        return recent.Count(entry => entry.Key.Sender == sender) / windowSeconds;
    }

    public static string TypeName(byte command) => (ClientCommand)command switch
    {
        ClientCommand.Input1 or ClientCommand.Input2 or ClientCommand.Input3 or ClientCommand.Input4
            or ClientCommand.Input5 or ClientCommand.Input6 or ClientCommand.Input7 or ClientCommand.Input8 => TextType,
        var known when Enum.IsDefined(known) => known.ToString(),
        _ => string.Create(CultureInfo.InvariantCulture, $"Unknown 0x{command:X2}"),
    };

    private void DropOld(long nowTimestamp)
    {
        while (recent.TryPeek(out var oldest) && oldest.At <= nowTimestamp - windowTimestamp)
        {
            recent.Dequeue();
        }
    }
}
