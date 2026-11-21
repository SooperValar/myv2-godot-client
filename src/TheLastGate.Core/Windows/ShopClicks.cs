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

    public static int SlotFor(int cell) => Math.Min(LastSlot, cell);

    public static byte[]? Packet(MapButton button, MapKeys keys, int shopNumber, int slot) => button switch
    {
        MapButton.Left when (keys & (MapKeys.Ctrl | MapKeys.Alt)) != 0 => ShopPackets.BuyTen(shopNumber, slot),
        MapButton.Left => ShopPackets.Buy(shopNumber, slot),
        MapButton.Right => ShopPackets.Describe(shopNumber, slot),
        _ => null,
    };

    public static MouseCursor Cursor(bool slotHasItem, bool holdingItem) => (slotHasItem, holdingItem) switch
    {
        (true, _) => MouseCursor.Take,
        (false, true) => MouseCursor.Drop,
        _ => MouseCursor.None,
    };
}
