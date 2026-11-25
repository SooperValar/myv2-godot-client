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
    public void Packets_DestroyTheHeldItem()
    {
        var packets = TrashCan.Packets(MapButton.Left, true, Selected);

        AssertThat(packets.Count).IsEqual(1);
        AssertThat(packets[0]).IsEqual(InventoryPackets.For(InventoryAction.TrashHeldItem, 0, Selected));
    }

    [TestCase]
    public void Packets_OpenTheTrashWindowWithAnEmptyHand()
    {
        var expected = ChatPackets.Say("#trash");
        var packets = TrashCan.Packets(MapButton.Left, false, Selected);

        AssertThat(packets.Count).IsEqual(expected.Count);
        AssertThat(packets[0]).IsEqual(expected[0]);
    }

    [TestCase]
    public void Packets_SendNothingOnARightClick()
    {
        AssertThat(TrashCan.Packets(MapButton.Right, true, Selected)).IsEmpty();
    }

    [TestCase(true, MouseCursor.Trash)]
    [TestCase(false, MouseCursor.None)]
    public void Cursor_ShowsTheTrashWhileHoldingAnItem(bool holdingItem, MouseCursor cursor)
    {
        AssertThat(TrashCan.Cursor(holdingItem)).IsEqual(cursor);
    }
}
