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

    [Fact]
    public void DescribeInGrave_SendsTheSlotPast100()
    {
        byte[] expected = [25, 177, 4, 150, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

        Assert.Equal(expected, ShopPackets.DescribeInGrave(ShopNumber, 50));
    }

    [Fact]
    public void PutInTrash_SendsTheBackpackSlotPast1000()
    {
        byte[] expected = [25, 12, 64, 0xEF, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

        Assert.Equal(expected, ShopPackets.PutInTrash(0x4000 | 12, 7));
    }

    [Fact]
    public void EmptyTrash_Sends2000()
    {
        byte[] expected = [25, 12, 64, 0xD0, 7, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

        Assert.Equal(expected, ShopPackets.EmptyTrash(0x4000 | 12));
    }

    [Fact]
    public void CloseTrash_Sends3000()
    {
        byte[] expected = [25, 12, 64, 0xB8, 11, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

        Assert.Equal(expected, ShopPackets.CloseTrash(0x4000 | 12));
    }
}
