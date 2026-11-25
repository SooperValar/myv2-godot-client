using GdUnit4;
using TheLastGate.Cursors;
using TheLastGate.Items;
using TheLastGate.Net;
using TheLastGate.State;
using TheLastGate.World;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Items;

[TestSuite]
public class InventoryClicksTests
{
    private const int Slot = 17;
    private const int Selected = 42;
    private const MapKeys AllKeys = MapKeys.Shift | MapKeys.Ctrl | MapKeys.Alt;

    private static readonly ItemSlot Plain = new(700, 0, 0, 0);
    private static readonly ItemSlot Empty = new(0, 0, 0, 0);

    [TestCase(MapKeys.None, InventoryAction.UseItem)]
    [TestCase(MapKeys.Shift, InventoryAction.SwapWithInventorySlot)]
    [TestCase(MapKeys.Ctrl, InventoryAction.TakeOneFromStack)]
    public void Packets_SendsTheInventoryActionForALeftClick(MapKeys keys, InventoryAction action)
    {
        AssertThat(InventoryClicks.Packets(Slot, Plain, false, MapButton.Left, keys, Selected))
            .ContainsExactly(InventoryPackets.For(action, Slot, Selected));
    }

    [TestCase(MapKeys.Alt)]
    [TestCase(MapKeys.Shift | MapKeys.Ctrl)]
    [TestCase(MapKeys.Ctrl | MapKeys.Alt)]
    [TestCase(AllKeys)]
    public void Packets_SendsNothingForKeysMyV2HasNoUseFor(MapKeys keys)
    {
        AssertThat(InventoryClicks.Packets(Slot, Plain, false, MapButton.Left, keys, Selected)).IsEmpty();
    }

    [TestCase(MapKeys.None)]
    [TestCase(MapKeys.Shift)]
    public void Packets_LooksAtTheItemOnARightClick(MapKeys keys)
    {
        AssertThat(InventoryClicks.Packets(Slot, Plain, false, MapButton.Right, keys, Selected))
            .ContainsExactly(ClientPackets.ThreeValues(ClientCommand.InventoryLook, Slot, 0, Selected));
    }

    [TestCase(MapButton.Right, MapKeys.Ctrl)]
    [TestCase(MapButton.Right, MapKeys.Alt)]
    [TestCase(MapButton.None, MapKeys.None)]
    public void Packets_SendsNothingForOtherClicks(MapButton button, MapKeys keys)
    {
        AssertThat(InventoryClicks.Packets(Slot, Plain, false, button, keys, Selected)).IsEmpty();
    }

    [TestCase(true, false, MapKeys.None, MouseCursor.Use)]
    [TestCase(false, true, MapKeys.None, MouseCursor.None)]
    [TestCase(true, true, MapKeys.Shift, MouseCursor.Swap)]
    [TestCase(true, false, MapKeys.Shift, MouseCursor.Take)]
    [TestCase(false, true, MapKeys.Shift, MouseCursor.Drop)]
    [TestCase(false, false, MapKeys.Shift, MouseCursor.None)]
    [TestCase(true, true, MapKeys.Ctrl, MouseCursor.Take)]
    [TestCase(true, false, MapKeys.Ctrl, MouseCursor.Take)]
    [TestCase(false, false, MapKeys.Ctrl, MouseCursor.None)]
    [TestCase(false, true, MapKeys.Alt, MouseCursor.None)]
    [TestCase(true, false, AllKeys, MouseCursor.None)]
    [TestCase(true, true, AllKeys, MouseCursor.None)]
    [TestCase(false, false, AllKeys, MouseCursor.None)]
    public void Cursor_ShowsWhatALeftClickWouldDo(bool hasItem, bool holdingItem, MapKeys keys, MouseCursor cursor)
    {
        AssertThat(InventoryClicks.Cursor(hasItem ? Plain : Empty, holdingItem, keys)).IsEqual(cursor);
    }
}
