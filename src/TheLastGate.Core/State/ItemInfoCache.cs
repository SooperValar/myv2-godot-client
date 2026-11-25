using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using TheLastGate.Net;

namespace TheLastGate.State;

// The MyV2 server describes an item when asked: where it is (the backpack, worn, or a shop or grave), its slot and, for
// a shop, the merchant. Answers are kept until the slot holds something else, and asked for again now and then so
// values like the condition stay current, as merc.exe does.
public sealed class ItemInfoCache
{
    public const int Backpack = 0;
    public const int Worn = 1;
    public const int Shop = 2;

    private const long RefreshAfterMilliseconds = 5000;
    private const long RetryAfterMilliseconds = 3000;
    private const int SlotOffset = 2;
    private const int MerchantOffset = 4;

    private readonly Dictionary<(int Where, int Slot), Entry> entries = [];
    private readonly Dictionary<(int Where, int Slot), List<LogLine>> arriving = [];

    public Action<byte[]> Send { get; set; } = _ => { };

    public IReadOnlyList<LogLine>? For(int where, int slot, int sprite, int merchant, long nowMilliseconds)
    {
        var key = (where, slot);
        entries.TryGetValue(key, out var entry);
        var sameItem = entry != null && entry.Sprite == sprite && entry.Merchant == merchant;
        var wait = entry?.Lines != null ? RefreshAfterMilliseconds : RetryAfterMilliseconds;
        if (!sameItem || nowMilliseconds - entry!.AskedAt > wait)
        {
            Send(Request(where, slot, merchant));
            entry = new Entry(sprite, merchant, nowMilliseconds, sameItem ? entry!.Lines : null);
            entries[key] = entry;
        }

        return entry.Lines;
    }

    internal void Receive(int where, int slot, int line, int font, string text)
    {
        var key = (where, slot);
        if (line == 0)
        {
            arriving[key] = [];
        }

        if (arriving.TryGetValue(key, out var lines) && line == lines.Count)
        {
            lines.Add(new LogLine(text, font));
        }
    }

    internal void Complete(int where, int slot)
    {
        var key = (where, slot);
        if (arriving.Remove(key, out var lines) && entries.TryGetValue(key, out var entry))
        {
            entries[key] = entry with { Lines = lines };
        }
    }

    private static byte[] Request(int where, int slot, int merchant)
    {
        var packet = new byte[Protocol.PacketSize];
        packet[0] = (byte)ClientCommand.ItemInfo;
        packet[1] = (byte)where;
        BinaryPrimitives.WriteUInt16LittleEndian(packet.AsSpan(SlotOffset), (ushort)slot);
        BinaryPrimitives.WriteInt32LittleEndian(packet.AsSpan(MerchantOffset), merchant);
        return packet;
    }

    private sealed record Entry(int Sprite, int Merchant, long AskedAt, IReadOnlyList<LogLine>? Lines);
}
