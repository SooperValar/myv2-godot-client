using TheLastGate.Cursors;
using TheLastGate.Items;
using TheLastGate.Net;
using TheLastGate.State;
using TheLastGate.Windows;
using TheLastGate.World;

namespace TheLastGate.Tests;

public class ShopClicksTests
{
    private const int ShopNumber = 1201;

    [Theory]
    [InlineData(0, 0)]
    [InlineData(61, 61)]
    [InlineData(62, 62)]
    [InlineData(63, 62)]
    public void SlotFor_GivesTheBottomRightCellsTheLastSlot(int cell, int slot)
    {
        Assert.Equal(slot, ShopClicks.SlotFor(cell));
    }

    [Theory]
    [InlineData(MapButton.Left, MapKeys.None, 5)]
    [InlineData(MapButton.Left, MapKeys.Ctrl, 129)]
    [InlineData(MapButton.Left, MapKeys.Alt, 129)]
    [InlineData(MapButton.Left, MapKeys.Shift, 5)]
    [InlineData(MapButton.Right, MapKeys.None, 67)]
    [InlineData(MapButton.Right, MapKeys.Ctrl, 67)]
    public void Packet_BuysTenWithControlOrAltAndDescribesOnARightClick(MapButton button, MapKeys keys, int value)
    {
        Assert.Equal(
            ClientPackets.ShortAndInt(ClientCommand.Shop, ShopNumber, value),
            ShopClicks.Packet(button, keys, ShopNumber, 5));
    }

    [Fact]
    public void Packet_SendsNothingForOtherButtons()
    {
        Assert.Null(ShopClicks.Packet(MapButton.None, MapKeys.None, ShopNumber, 5));
    }

    [Theory]
    [InlineData(true, false, MouseCursor.Take)]
    [InlineData(true, true, MouseCursor.Take)]
    [InlineData(false, true, MouseCursor.Drop)]
    [InlineData(false, false, MouseCursor.None)]
    public void Cursor_ShowsTakeOverAnItemAndDropWhenHoldingOne(bool slotHasItem, bool holding, MouseCursor cursor)
    {
        Assert.Equal(cursor, ShopClicks.Cursor(slotHasItem, holding));
    }

    [Theory]
    [InlineData(ShopKind.Merchant, 12345, false, "   Buy for:       123G 45S")]
    [InlineData(ShopKind.Merchant, 12345, true, "Buy 10 for:      1234G 50S")]
    [InlineData(ShopKind.BlackStronghold, 500, false, "    Take reward for:       500 Stronghold Pts")]
    [InlineData(ShopKind.Casino, 75, false, "Take reward for:        75 Tokens")]
    [InlineData(ShopKind.Contract, 1200, false, "  Take reward for:      1200 Contract Pts")]
    [InlineData(ShopKind.Experience, 999999, false, "         Buy for:    999999 Exp")]
    public void Price_WritesWhatTheHoveredItemCosts(ShopKind kind, int price, bool buyingTen, string text)
    {
        Assert.Equal(text, ShopText.Price(kind, price, buyingTen));
    }

    [Theory]
    [InlineData(ShopKind.BlackStronghold)]
    [InlineData(ShopKind.Experience)]
    public void Price_BuysOnlyOneOutsideGoldShops(ShopKind kind)
    {
        Assert.Equal(ShopText.Price(kind, 500, false), ShopText.Price(kind, 500, true));
    }

    [Fact]
    public void Price_ShowsNothingForAFreeItem()
    {
        Assert.Null(ShopText.Price(ShopKind.Merchant, 0, false));
    }

    [Fact]
    public void Sell_WritesWhatTheMerchantPaysForTheHeldItem()
    {
        Assert.Equal("  Sell for:        25G  0S", ShopText.Sell(ShopKind.Merchant, 2500));
    }

    [Theory]
    [InlineData(ShopKind.Merchant, 0)]
    [InlineData(ShopKind.BlackStronghold, 2500)]
    public void Sell_ShowsNothingWithoutAGoldPrice(ShopKind kind, int heldItemPrice)
    {
        Assert.Null(ShopText.Sell(kind, heldItemPrice));
    }

    [Theory]
    [InlineData(ShopKind.Merchant, "Your Money:     12345G 67S")]
    [InlineData(ShopKind.BlackStronghold, "Your Stronghold Pts:       176")]
    [InlineData(ShopKind.Casino, "    Your Tokens:      4896")]
    [InlineData(ShopKind.Contract, "Your Contract Pts:     20018")]
    [InlineData(ShopKind.Experience, "Your Unspent Exp:       187")]
    public void Balance_ShowsWhatTheShopTakes(ShopKind kind, string text)
    {
        var balances = new ShopBalances(1234567, 176, 4896, 20018, 187);

        Assert.Equal(text, ShopText.Balance(kind, [new ShopItem(1000, 10, false, false, 0)], balances));
    }

    [Fact]
    public void Balance_ShowsNothingWhenNothingHasAPrice()
    {
        var balances = new ShopBalances(1234567, 176, 4896, 20018, 187);

        Assert.Null(ShopText.Balance(ShopKind.Merchant, [new ShopItem(1000, 0, false, false, 0)], balances));
    }

    [Fact]
    public void Shop_OverlaysSoulstoneAndTalismanMarks()
    {
        Assert.Equal(
            [new ItemOverlay(4496, 0), new ItemOverlay(4497, 0)],
            ItemOverlays.Shop(new ShopItem(1000, 10, true, true, 0)));
    }

    [Fact]
    public void Shop_OverlaysACatalystsKind()
    {
        Assert.Equal([new ItemOverlay(6999 + 3, 0)], ItemOverlays.Shop(new ShopItem(16971, 10, false, false, 3)));
    }

    [Fact]
    public void Shop_OverlaysACorruptersNumber()
    {
        Assert.Equal(
            [new ItemOverlay(7799, 0), new ItemOverlay(7800 + 2, 5), new ItemOverlay(7800 + 5, 10)],
            ItemOverlays.Shop(new ShopItem(6880, 10, false, false, 0x25)));
    }

    [Fact]
    public void Shop_LeavesAnEmptySlotBare()
    {
        Assert.Empty(ItemOverlays.Shop(new ShopItem(0, 0, true, true, 3)));
    }
}
