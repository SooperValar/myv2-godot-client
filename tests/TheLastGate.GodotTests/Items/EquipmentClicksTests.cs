using GdUnit4;
using TheLastGate.Cursors;
using TheLastGate.Items;
using TheLastGate.Net;
using TheLastGate.State;
using TheLastGate.World;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Items;

[TestSuite]
public class EquipmentClicksTests
{
    private const int Selected = 42;

    [TestCase(MapButton.Left, MapKeys.None, InventoryAction.UseGear)]
    [TestCase(MapButton.Left, MapKeys.Shift, InventoryAction.SwapWithGearSlot)]
    [TestCase(MapButton.Right, MapKeys.None, InventoryAction.LookAtGear)]
    [TestCase(MapButton.Right, MapKeys.Shift, InventoryAction.LookAtGear)]
    public void Packet_SendsTheInventoryActionForTheClick(MapButton button, MapKeys keys, InventoryAction action)
    {
        AssertThat(EquipmentClicks.Packet(WornSlot.Belt, button, keys, Selected))
            .IsEqual(InventoryPackets.For(action, (int)WornSlot.Belt, Selected));
    }

    [TestCase(MapButton.Left, MapKeys.Ctrl)]
    [TestCase(MapButton.Left, MapKeys.Alt)]
    [TestCase(MapButton.Left, MapKeys.Shift | MapKeys.Ctrl)]
    [TestCase(MapButton.Right, MapKeys.Ctrl)]
    [TestCase(MapButton.None, MapKeys.None)]
    public void Packet_SendsNothingForControlOrAltClicks(MapButton button, MapKeys keys)
    {
        AssertThat(EquipmentClicks.Packet(WornSlot.Belt, button, keys, Selected)).IsNull();
    }

    [TestCase(true, false, MapKeys.None, MouseCursor.Use)]
    [TestCase(true, true, MapKeys.None, MouseCursor.Use)]
    [TestCase(false, true, MapKeys.None, MouseCursor.None)]
    [TestCase(true, true, MapKeys.Shift, MouseCursor.Swap)]
    [TestCase(true, false, MapKeys.Shift, MouseCursor.Take)]
    [TestCase(false, true, MapKeys.Shift, MouseCursor.Drop)]
    [TestCase(false, false, MapKeys.Shift, MouseCursor.None)]
    [TestCase(true, true, MapKeys.Ctrl, MouseCursor.None)]
    [TestCase(true, true, MapKeys.Alt, MouseCursor.None)]
    public void Cursor_ShowsWhatAClickWouldDo(bool slotHasItem, bool holdingItem, MapKeys keys, MouseCursor cursor)
    {
        AssertThat(EquipmentClicks.Cursor(slotHasItem, holdingItem, keys)).IsEqual(cursor);
    }
}
