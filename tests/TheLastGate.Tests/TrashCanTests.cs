using TheLastGate.Cursors;
using TheLastGate.Items;
using TheLastGate.Net;
using TheLastGate.World;

namespace TheLastGate.Tests;

public class TrashCanTests
{
    private const int Selected = 42;

    [Fact]
    public void Packet_TrashesTheHeldItemOnALeftClick()
    {
        Assert.Equal(
            InventoryPackets.For(InventoryAction.Special, 0, Selected),
            TrashCan.Packet(MapButton.Left, Selected));
        Assert.Null(TrashCan.Packet(MapButton.Right, Selected));
    }

    [Theory]
    [InlineData(true, MouseCursor.Trash)]
    [InlineData(false, MouseCursor.None)]
    public void Cursor_ShowsTheTrashOnlyWhileHoldingAnItem(bool holdingItem, MouseCursor cursor)
    {
        Assert.Equal(cursor, TrashCan.Cursor(holdingItem));
    }
}
