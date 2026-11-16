using TheLastGate.Cursors;
using TheLastGate.Items;
using TheLastGate.Net;
using TheLastGate.World;

namespace TheLastGate.Tests;

public class EquipmentClicksTests
{
    private const int Selected = 42;

    [Theory]
    [InlineData(MapButton.Left, MapKeys.None, InventoryAction.UseGear)]
    [InlineData(MapButton.Left, MapKeys.Shift, InventoryAction.SwapWithGearSlot)]
    [InlineData(MapButton.Right, MapKeys.None, InventoryAction.LookAtGear)]
    [InlineData(MapButton.Right, MapKeys.Shift, InventoryAction.LookAtGear)]
    public void Packet_SendsTheInventoryActionForTheClick(MapButton button, MapKeys keys, InventoryAction action)
    {
        Assert.Equal(
            InventoryPackets.For(action, (int)WornSlot.Belt, Selected),
            EquipmentClicks.Packet(WornSlot.Belt, button, keys, Selected));
    }

    [Theory]
    [InlineData(MapButton.Left, MapKeys.Ctrl)]
    [InlineData(MapButton.Left, MapKeys.Alt)]
    [InlineData(MapButton.Left, MapKeys.Shift | MapKeys.Ctrl)]
    [InlineData(MapButton.Right, MapKeys.Ctrl)]
    [InlineData(MapButton.None, MapKeys.None)]
    public void Packet_SendsNothingForControlOrAltClicks(MapButton button, MapKeys keys)
    {
        Assert.Null(EquipmentClicks.Packet(WornSlot.Belt, button, keys, Selected));
    }

    [Theory]
    [InlineData(true, false, MapKeys.None, MouseCursor.Use)]
    [InlineData(true, true, MapKeys.None, MouseCursor.Use)]
    [InlineData(false, true, MapKeys.None, MouseCursor.None)]
    [InlineData(true, true, MapKeys.Shift, MouseCursor.Swap)]
    [InlineData(true, false, MapKeys.Shift, MouseCursor.Take)]
    [InlineData(false, true, MapKeys.Shift, MouseCursor.Drop)]
    [InlineData(false, false, MapKeys.Shift, MouseCursor.None)]
    [InlineData(true, true, MapKeys.Ctrl, MouseCursor.None)]
    [InlineData(true, true, MapKeys.Alt, MouseCursor.None)]
    public void Cursor_ShowsWhatAClickWouldDo(bool slotHasItem, bool holdingItem, MapKeys keys, MouseCursor cursor)
    {
        Assert.Equal(cursor, EquipmentClicks.Cursor(slotHasItem, holdingItem, keys));
    }

    [Fact]
    public void SwapSetPacket_SwapsToTheStoredSetOnALeftClick()
    {
        Assert.Equal(
            InventoryPackets.For(InventoryAction.Special, 1, Selected),
            EquipmentClicks.SwapSetPacket(MapButton.Left, Selected));
        Assert.Null(EquipmentClicks.SwapSetPacket(MapButton.Right, Selected));
    }
}
