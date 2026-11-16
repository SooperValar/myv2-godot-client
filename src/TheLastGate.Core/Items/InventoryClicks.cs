using System.Collections.Generic;
using TheLastGate.Cursors;
using TheLastGate.Net;
using TheLastGate.State;
using TheLastGate.World;

namespace TheLastGate.Items;

public static class InventoryClicks
{
    private const MapKeys AllKeys = MapKeys.Shift | MapKeys.Ctrl | MapKeys.Alt;
    private const int TrashHeldItem = 0;

    public static IReadOnlyList<byte[]> Packets(
        int slot,
        ItemSlot item,
        bool holdingItem,
        MapButton button,
        MapKeys keys,
        ShopSale? sale,
        int selectedCharacter)
    {
        if (button == MapButton.Right)
        {
            return HoldsControlOrAlt(keys)
                ? []
                : [ClientPackets.ThreeValues(ClientCommand.InventoryLook, slot, 0, selectedCharacter)];
        }

        if (button != MapButton.Left)
        {
            return [];
        }

        if (keys == AllKeys)
        {
            return CanTrash(item, holdingItem)
                ?
                [
                    InventoryPackets.For(InventoryAction.SwapWithInventorySlot, slot, selectedCharacter),
                    InventoryPackets.For(InventoryAction.Special, TrashHeldItem, selectedCharacter),
                ]
                : [];
        }

        if (HoldsControlOrAlt(keys))
        {
            return sale is { } shop
                ? [ClientPackets.ThreeValues(ClientCommand.QuickShop, shop.Shop, slot, shop.DepotPage)]
                : [InventoryPackets.For(InventoryAction.MoveStack, slot, selectedCharacter)];
        }

        var action = keys.HasFlag(MapKeys.Shift) ? InventoryAction.SwapWithInventorySlot : InventoryAction.UseItem;
        return [InventoryPackets.For(action, slot, selectedCharacter)];
    }

    public static MouseCursor Cursor(ItemSlot item, bool holdingItem, MapKeys keys)
    {
        var hasItem = item.Sprite != 0;
        if (keys == AllKeys)
        {
            return hasItem && CanTrash(item, holdingItem) ? MouseCursor.Trash : MouseCursor.None;
        }

        if (keys == MapKeys.None)
        {
            return hasItem ? MouseCursor.Use : MouseCursor.None;
        }

        return (hasItem, holdingItem) switch
        {
            (true, true) => keys == MapKeys.Shift ? MouseCursor.Swap : MouseCursor.None,
            (true, false) => MouseCursor.Take,
            (false, true) => MouseCursor.Drop,
            (false, false) => MouseCursor.None,
        };
    }

    private static bool HoldsControlOrAlt(MapKeys keys) => keys.HasFlag(MapKeys.Ctrl) || keys.HasFlag(MapKeys.Alt);

    private static bool CanTrash(ItemSlot item, bool holdingItem)
    {
        var flags = (CarriedItemFlags)item.Flags;
        return !holdingItem && item.Stack == 0 && !flags.HasFlag(CarriedItemFlags.Soulstoned)
            && !flags.HasFlag(CarriedItemFlags.Enchanted) && !flags.HasFlag(CarriedItemFlags.Corrupted);
    }
}
