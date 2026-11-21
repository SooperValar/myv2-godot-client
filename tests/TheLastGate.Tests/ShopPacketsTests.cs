using TheLastGate.Net;

namespace TheLastGate.Tests;

public class ShopPacketsTests
{
    private const int ShopNumber = 1201;

    [Fact]
    public void Buy_SendsTheShopAndTheSlot()
    {
        byte[] expected = [25, 177, 4, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

        Assert.Equal(expected, ShopPackets.Buy(ShopNumber, 5));
    }

    [Fact]
    public void BuyTen_SendsTheSlotPast124()
    {
        byte[] expected = [25, 177, 4, 129, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

        Assert.Equal(expected, ShopPackets.BuyTen(ShopNumber, 5));
    }

    [Fact]
    public void Describe_SendsTheSlotPast62()
    {
        byte[] expected = [25, 177, 4, 67, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

        Assert.Equal(expected, ShopPackets.Describe(ShopNumber, 5));
    }
}
