using TheLastGate.Cursors;
using TheLastGate.Net;
using TheLastGate.World;

namespace TheLastGate.Items;

public static class TrashCan
{
    public const string Hint = "Dispose of items under your cursor here.";

    private const int TrashHeldItem = 0;

    public static byte[]? Packet(MapButton button, int selectedCharacter) =>
        button == MapButton.Left
            ? InventoryPackets.For(InventoryAction.Special, TrashHeldItem, selectedCharacter)
            : null;

    public static MouseCursor Cursor(bool holdingItem) => holdingItem ? MouseCursor.Trash : MouseCursor.None;
}
