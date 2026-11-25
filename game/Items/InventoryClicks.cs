using System.Collections.Generic;
using TheLastGate.Cursors;
using TheLastGate.Net;
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
        int selectedCharacter,
        int? openTrash = null)
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

        if (keys == MapKeys.Shift && openTrash is { } trash)
        {
            return item.Sprite != 0 ? [ShopPackets.PutInTrash(trash, slot)] : [];
        }

        return keys switch
        {
            MapKeys.Ctrl => [InventoryPackets.For(InventoryAction.TakeOneFromStack, slot, selectedCharacter)],
            MapKeys.None => [InventoryPackets.For(InventoryAction.UseItem, slot, selectedCharacter)],
            MapKeys.Shift => [InventoryPackets.For(InventoryAction.SwapWithInventorySlot, slot, selectedCharacter)],
            _ => [],
        };
    }

    public static MouseCursor Cursor(ItemSlot item, bool holdingItem, MapKeys keys, bool trashOpen = false)
    {
        var hasItem = item.Sprite != 0;
        if (keys == MapKeys.Shift && trashOpen)
        {
            return hasItem ? MouseCursor.Trash : MouseCursor.None;
        }

        if (keys == MapKeys.None)
        {
            return hasItem ? MouseCursor.Use : MouseCursor.None;
        }

        if (keys == MapKeys.Ctrl)
        {
            return hasItem ? MouseCursor.Take : MouseCursor.None;
        }

        if (keys != MapKeys.Shift)
        {
            return MouseCursor.None;
        }

        return (hasItem, holdingItem) switch
        {
            (true, true) => MouseCursor.Swap,
            (true, false) => MouseCursor.Take,
            (false, true) => MouseCursor.Drop,
            (false, false) => MouseCursor.None,
        };
    }

    private static bool HoldsControlOrAlt(MapKeys keys) => keys.HasFlag(MapKeys.Ctrl) || keys.HasFlag(MapKeys.Alt);
}
