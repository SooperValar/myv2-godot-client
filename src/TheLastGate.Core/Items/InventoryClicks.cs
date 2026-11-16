using System;
using System.Collections.Generic;
using TheLastGate.Cursors;
using TheLastGate.State;
using TheLastGate.World;

namespace TheLastGate.Items;

public static class InventoryClicks
{
    public static IReadOnlyList<byte[]> Packets(
        int slot,
        ItemSlot item,
        bool holdingItem,
        MapButton button,
        MapKeys keys,
        ShopSale? sale,
        int selectedCharacter) => throw new NotImplementedException();

    public static MouseCursor Cursor(ItemSlot item, bool holdingItem, MapKeys keys) =>
        throw new NotImplementedException();
}
