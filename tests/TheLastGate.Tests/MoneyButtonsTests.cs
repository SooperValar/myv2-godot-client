using TheLastGate.Items;
using TheLastGate.Net;
using TheLastGate.World;

namespace TheLastGate.Tests;

public class MoneyButtonsTests
{
    private const int Selected = 42;

    [Theory]
    [InlineData(1000000)]
    [InlineData(100000)]
    [InlineData(10000)]
    public void Packet_TakesThatMuchMoneyOnALeftClick(int silver)
    {
        Assert.Equal(
            InventoryPackets.For(InventoryAction.TakeMoney, silver, Selected),
            MoneyButtons.Packet(silver, MapButton.Left, Selected));
    }

    [Fact]
    public void Packet_SendsNothingOnARightClick()
    {
        Assert.Null(MoneyButtons.Packet(10000, MapButton.Right, Selected));
    }

    [Theory]
    [InlineData(1000000, "10,000 gold coins.")]
    [InlineData(100000, "1,000 gold coins.")]
    [InlineData(10000, "100 gold coins.")]
    public void Hint_NamesTheGoldTheButtonTakes(int silver, string hint)
    {
        Assert.Equal(hint, MoneyButtons.Hint(silver));
    }
}
