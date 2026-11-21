using GdUnit4;
using TheLastGate.Cursors;
using TheLastGate.Items;
using TheLastGate.Net;
using TheLastGate.World;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Items;

[TestSuite]
public class TrashCanTests
{
    private const int Selected = 42;

    [TestCase]
    public void Packet_TrashesTheHeldItemOnALeftClick()
    {
        AssertThat(TrashCan.Packet(MapButton.Left, Selected))
            .IsEqual(InventoryPackets.For(InventoryAction.Special, 0, Selected));
        AssertThat(TrashCan.Packet(MapButton.Right, Selected)).IsNull();
    }

    [TestCase(true, MouseCursor.Trash)]
    [TestCase(false, MouseCursor.None)]
    public void Cursor_ShowsTheTrashOnlyWhileHoldingAnItem(bool holdingItem, MouseCursor cursor)
    {
        AssertThat(TrashCan.Cursor(holdingItem)).IsEqual(cursor);
    }
}
