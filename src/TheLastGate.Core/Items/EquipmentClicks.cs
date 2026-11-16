using System;
using TheLastGate.Cursors;
using TheLastGate.World;

namespace TheLastGate.Items;

public static class EquipmentClicks
{
    public static byte[]? Packet(WornSlot slot, MapButton button, MapKeys keys, int selectedCharacter) =>
        throw new NotImplementedException();

    public static MouseCursor Cursor(bool slotHasItem, bool holdingItem, MapKeys keys) =>
        throw new NotImplementedException();

    public static byte[]? SwapSetPacket(MapButton button, int selectedCharacter) =>
        throw new NotImplementedException();
}
