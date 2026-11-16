using TheLastGate.Cursors;
using TheLastGate.Items;
using TheLastGate.Net;
using TheLastGate.State;
using TheLastGate.World;

namespace TheLastGate.Tests;

public class InventoryClicksTests
{
    private const int Slot = 17;
    private const int Selected = 42;
    private const MapKeys AllKeys = MapKeys.Shift | MapKeys.Ctrl | MapKeys.Alt;

    private static readonly ItemSlot Plain = new(700, 0, 0, 0);
    private static readonly ItemSlot Empty = new(0, 0, 0, 0);

    [Theory]
    [InlineData(MapKeys.None, InventoryAction.UseItem)]
    [InlineData(MapKeys.Shift, InventoryAction.SwapWithInventorySlot)]
    [InlineData(MapKeys.Ctrl, InventoryAction.MoveStack)]
    [InlineData(MapKeys.Alt, InventoryAction.MoveStack)]
    [InlineData(MapKeys.Shift | MapKeys.Ctrl, InventoryAction.MoveStack)]
    public void Packets_SendsTheInventoryActionForALeftClick(MapKeys keys, InventoryAction action)
    {
        Assert.Equal(
            [InventoryPackets.For(action, Slot, Selected)],
            InventoryClicks.Packets(Slot, Plain, false, MapButton.Left, keys, null, Selected));
    }

    [Fact]
    public void Packets_SellsIntoAnOpenShopOnAControlClick()
    {
        Assert.Equal(
            [ClientPackets.ThreeValues(ClientCommand.QuickShop, 5, Slot, 2)],
            InventoryClicks.Packets(Slot, Plain, false, MapButton.Left, MapKeys.Ctrl, new ShopSale(5, 2), Selected));
    }

    [Fact]
    public void Packets_TrashesAPlainItemWithAllThreeKeys()
    {
        Assert.Equal(
            [
                InventoryPackets.For(InventoryAction.SwapWithInventorySlot, Slot, Selected),
                InventoryPackets.For(InventoryAction.Special, 0, Selected),
            ],
            InventoryClicks.Packets(Slot, Plain, false, MapButton.Left, AllKeys, null, Selected));
    }

    [Theory]
    [InlineData(0, 2, false)]
    [InlineData(0, 4, false)]
    [InlineData(0, 8, false)]
    [InlineData(3, 0, false)]
    [InlineData(0, 0, true)]
    public void Packets_KeepsSpecialStackedOrSwappedItemsFromTheTrash(byte stack, byte flags, bool holdingItem)
    {
        var item = new ItemSlot(700, 0, stack, flags);

        Assert.Empty(InventoryClicks.Packets(Slot, item, holdingItem, MapButton.Left, AllKeys, null, Selected));
    }

    [Theory]
    [InlineData(MapKeys.None)]
    [InlineData(MapKeys.Shift)]
    public void Packets_LooksAtTheItemOnARightClick(MapKeys keys)
    {
        Assert.Equal(
            [ClientPackets.ThreeValues(ClientCommand.InventoryLook, Slot, 0, Selected)],
            InventoryClicks.Packets(Slot, Plain, false, MapButton.Right, keys, null, Selected));
    }

    [Theory]
    [InlineData(MapButton.Right, MapKeys.Ctrl)]
    [InlineData(MapButton.Right, MapKeys.Alt)]
    [InlineData(MapButton.None, MapKeys.None)]
    public void Packets_SendsNothingForOtherClicks(MapButton button, MapKeys keys)
    {
        Assert.Empty(InventoryClicks.Packets(Slot, Plain, false, button, keys, null, Selected));
    }

    [Theory]
    [InlineData(true, false, MapKeys.None, MouseCursor.Use)]
    [InlineData(false, true, MapKeys.None, MouseCursor.None)]
    [InlineData(true, true, MapKeys.Shift, MouseCursor.Swap)]
    [InlineData(true, false, MapKeys.Shift, MouseCursor.Take)]
    [InlineData(false, true, MapKeys.Shift, MouseCursor.Drop)]
    [InlineData(false, false, MapKeys.Shift, MouseCursor.None)]
    [InlineData(true, true, MapKeys.Ctrl, MouseCursor.None)]
    [InlineData(true, false, MapKeys.Ctrl, MouseCursor.Take)]
    [InlineData(false, true, MapKeys.Alt, MouseCursor.Drop)]
    [InlineData(true, false, AllKeys, MouseCursor.Trash)]
    [InlineData(true, true, AllKeys, MouseCursor.None)]
    [InlineData(false, false, AllKeys, MouseCursor.None)]
    public void Cursor_ShowsWhatALeftClickWouldDo(bool hasItem, bool holdingItem, MapKeys keys, MouseCursor cursor)
    {
        Assert.Equal(cursor, InventoryClicks.Cursor(hasItem ? Plain : Empty, holdingItem, keys));
    }

    [Fact]
    public void Cursor_ShowsNoTrashForAnItemThatCannotBeTrashed()
    {
        Assert.Equal(MouseCursor.None, InventoryClicks.Cursor(new ItemSlot(700, 0, 2, 0), false, AllKeys));
    }
}
