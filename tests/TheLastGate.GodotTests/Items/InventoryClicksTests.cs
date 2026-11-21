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
    [TestCase(MapKeys.Ctrl, InventoryAction.MoveStack)]
    [TestCase(MapKeys.Alt, InventoryAction.MoveStack)]
    [TestCase(MapKeys.Shift | MapKeys.Ctrl, InventoryAction.MoveStack)]
    public void Packets_SendsTheInventoryActionForALeftClick(MapKeys keys, InventoryAction action)
    {
        AssertThat(InventoryClicks.Packets(Slot, Plain, false, MapButton.Left, keys, null, Selected))
            .ContainsExactly(InventoryPackets.For(action, Slot, Selected));
    }

    [TestCase]
    public void Packets_SellsIntoAnOpenShopOnAControlClick()
    {
        AssertThat(
                InventoryClicks.Packets(Slot, Plain, false, MapButton.Left, MapKeys.Ctrl, new ShopSale(5, 2), Selected))
            .ContainsExactly(ClientPackets.ThreeValues(ClientCommand.QuickShop, 5, Slot, 2));
    }

    [TestCase]
    public void Packets_TrashesAPlainItemWithAllThreeKeys()
    {
        AssertThat(InventoryClicks.Packets(Slot, Plain, false, MapButton.Left, AllKeys, null, Selected))
            .ContainsExactly(
                InventoryPackets.For(InventoryAction.SwapWithInventorySlot, Slot, Selected),
                InventoryPackets.For(InventoryAction.Special, 0, Selected));
    }

    [TestCase(0, 2, false)]
    [TestCase(0, 4, false)]
    [TestCase(0, 8, false)]
    [TestCase(3, 0, false)]
    [TestCase(0, 0, true)]
    public void Packets_KeepsSpecialStackedOrSwappedItemsFromTheTrash(int stack, int flags, bool holdingItem)
    {
        var item = new ItemSlot(700, 0, (byte)stack, (byte)flags);

        AssertThat(InventoryClicks.Packets(Slot, item, holdingItem, MapButton.Left, AllKeys, null, Selected)).IsEmpty();
    }

    [TestCase(MapKeys.None)]
    [TestCase(MapKeys.Shift)]
    public void Packets_LooksAtTheItemOnARightClick(MapKeys keys)
    {
        AssertThat(InventoryClicks.Packets(Slot, Plain, false, MapButton.Right, keys, null, Selected))
            .ContainsExactly(ClientPackets.ThreeValues(ClientCommand.InventoryLook, Slot, 0, Selected));
    }

    [TestCase(MapButton.Right, MapKeys.Ctrl)]
    [TestCase(MapButton.Right, MapKeys.Alt)]
    [TestCase(MapButton.None, MapKeys.None)]
    public void Packets_SendsNothingForOtherClicks(MapButton button, MapKeys keys)
    {
        AssertThat(InventoryClicks.Packets(Slot, Plain, false, button, keys, null, Selected)).IsEmpty();
    }

    [TestCase(true, false, MapKeys.None, MouseCursor.Use)]
    [TestCase(false, true, MapKeys.None, MouseCursor.None)]
    [TestCase(true, true, MapKeys.Shift, MouseCursor.Swap)]
    [TestCase(true, false, MapKeys.Shift, MouseCursor.Take)]
    [TestCase(false, true, MapKeys.Shift, MouseCursor.Drop)]
    [TestCase(false, false, MapKeys.Shift, MouseCursor.None)]
    [TestCase(true, true, MapKeys.Ctrl, MouseCursor.None)]
    [TestCase(true, false, MapKeys.Ctrl, MouseCursor.Take)]
    [TestCase(false, true, MapKeys.Alt, MouseCursor.Drop)]
    [TestCase(true, false, AllKeys, MouseCursor.Trash)]
    [TestCase(true, true, AllKeys, MouseCursor.None)]
    [TestCase(false, false, AllKeys, MouseCursor.None)]
    public void Cursor_ShowsWhatALeftClickWouldDo(bool hasItem, bool holdingItem, MapKeys keys, MouseCursor cursor)
    {
        AssertThat(InventoryClicks.Cursor(hasItem ? Plain : Empty, holdingItem, keys)).IsEqual(cursor);
    }

    [TestCase]
    public void Cursor_ShowsNoTrashForAnItemThatCannotBeTrashed()
    {
        AssertThat(InventoryClicks.Cursor(new ItemSlot(700, 0, 2, 0), false, AllKeys)).IsEqual(MouseCursor.None);
    }
}
