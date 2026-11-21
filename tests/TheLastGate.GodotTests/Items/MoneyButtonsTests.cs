using GdUnit4;
using TheLastGate.Items;
using TheLastGate.Net;
using TheLastGate.World;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Items;

[TestSuite]
public class MoneyButtonsTests
{
    private const int Selected = 42;

    [TestCase(1000000)]
    [TestCase(100000)]
    [TestCase(10000)]
    public void Packet_TakesThatMuchMoneyOnALeftClick(int silver)
    {
        AssertThat(MoneyButtons.Packet(silver, MapButton.Left, Selected))
            .IsEqual(InventoryPackets.For(InventoryAction.TakeMoney, silver, Selected));
    }

    [TestCase]
    public void Packet_SendsNothingOnARightClick()
    {
        AssertThat(MoneyButtons.Packet(10000, MapButton.Right, Selected)).IsNull();
    }

    [TestCase(1000000, "10,000 gold coins.")]
    [TestCase(100000, "1,000 gold coins.")]
    [TestCase(10000, "100 gold coins.")]
    public void Hint_NamesTheGoldTheButtonTakes(int silver, string hint)
    {
        AssertThat(MoneyButtons.Hint(silver)).IsEqual(hint);
    }
}
