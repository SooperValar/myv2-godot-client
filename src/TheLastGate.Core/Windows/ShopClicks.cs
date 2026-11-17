using System;
using TheLastGate.Cursors;
using TheLastGate.Net;
using TheLastGate.World;

namespace TheLastGate.Windows;

public static class ShopClicks
{
    public const int Cells = 64;

    // The grid has 64 cells for 62 items; the current client sends the last slot for the two cells past the end.
    private const int LastSlot = 62;
    private const int DescribeOffset = 62;
    private const int BuyTenOffset = 124;

    public static int SlotFor(int cell) => Math.Min(LastSlot, cell);

    public static byte[]? Packet(MapButton button, MapKeys keys, int shopNumber, int slot)
    {
        var value = button switch
        {
            MapButton.Left when (keys & (MapKeys.Ctrl | MapKeys.Alt)) != 0 => slot + BuyTenOffset,
            MapButton.Left => slot,
            MapButton.Right => slot + DescribeOffset,
            _ => (int?)null,
        };
        return value is { } shopValue ? ClientPackets.ShortAndInt(ClientCommand.Shop, shopNumber, shopValue) : null;
    }

    public static MouseCursor Cursor(bool slotHasItem, bool holdingItem) => (slotHasItem, holdingItem) switch
    {
        (true, _) => MouseCursor.Take,
        (false, true) => MouseCursor.Drop,
        _ => MouseCursor.None,
    };
}
