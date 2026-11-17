using System;
using TheLastGate.Cursors;
using TheLastGate.World;

namespace TheLastGate.Windows;

public static class ShopClicks
{
    public const int Cells = 64;

    public static int SlotFor(int cell) => throw new NotImplementedException();

    public static byte[]? Packet(MapButton button, MapKeys keys, int shopNumber, int slot) =>
        throw new NotImplementedException();

    public static MouseCursor Cursor(bool slotHasItem, bool holdingItem) => throw new NotImplementedException();
}
