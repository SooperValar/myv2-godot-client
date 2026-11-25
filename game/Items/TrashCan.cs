using System.Collections.Generic;
using TheLastGate.Cursors;
using TheLastGate.Net;
using TheLastGate.World;

namespace TheLastGate.Items;

// With an item in the hand a click destroys it; with an empty hand it opens the trash window.
public static class TrashCan
{
    public const string Hint = "Click with an item in your hand to destroy it, or with an empty hand to open the trash window.";

    // MyV2 opens its trash window anywhere for this command.
    private const string OpenCommand = "#trash";

    public static IReadOnlyList<byte[]> Packets(MapButton button, bool holdingItem, int selectedCharacter)
    {
        if (button != MapButton.Left)
        {
            return [];
        }

        return holdingItem
            ? [InventoryPackets.For(InventoryAction.TrashHeldItem, 0, selectedCharacter)]
            : [.. ChatPackets.Say(OpenCommand)];
    }

    public static MouseCursor Cursor(bool holdingItem) => holdingItem ? MouseCursor.Trash : MouseCursor.None;
}
