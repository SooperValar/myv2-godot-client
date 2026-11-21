using TheLastGate.Cursors;
using TheLastGate.Net;
using TheLastGate.State;
using TheLastGate.World;

namespace TheLastGate.Items;

public static class EquipmentClicks
{
    private const int SwapToStoredSet = 1;

    public static byte[]? Packet(WornSlot slot, MapButton button, MapKeys keys, int selectedCharacter)
    {
        if (keys.HasFlag(MapKeys.Ctrl) || keys.HasFlag(MapKeys.Alt))
        {
            return null;
        }

        return button switch
        {
            MapButton.Left => InventoryPackets.For(
                keys.HasFlag(MapKeys.Shift) ? InventoryAction.SwapWithGearSlot : InventoryAction.UseGear,
                (int)slot,
                selectedCharacter),
            MapButton.Right => InventoryPackets.For(InventoryAction.LookAtGear, (int)slot, selectedCharacter),
            _ => null,
        };
    }

    public static MouseCursor Cursor(bool slotHasItem, bool holdingItem, MapKeys keys)
    {
        if (keys.HasFlag(MapKeys.Ctrl) || keys.HasFlag(MapKeys.Alt))
        {
            return MouseCursor.None;
        }

        if (!keys.HasFlag(MapKeys.Shift))
        {
            return slotHasItem ? MouseCursor.Use : MouseCursor.None;
        }

        return (slotHasItem, holdingItem) switch
        {
            (true, true) => MouseCursor.Swap,
            (true, false) => MouseCursor.Take,
            (false, true) => MouseCursor.Drop,
            (false, false) => MouseCursor.None,
        };
    }

    public static byte[]? SwapSetPacket(MapButton button, int selectedCharacter) =>
        button == MapButton.Left
            ? InventoryPackets.For(InventoryAction.Special, SwapToStoredSet, selectedCharacter)
            : null;
}
