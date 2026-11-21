using GdUnit4;
using TheLastGate.Net;
using TheLastGate.Windows;
using TheLastGate.World;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Windows;

[TestSuite]
public class DepotClicksTests
{
    private const int DepotNumber = 0x8000 | 1201;

    [TestCase(MapButton.Left, 2, 10, 138)]
    [TestCase(MapButton.Left, 0, 0, 0)]
    [TestCase(MapButton.Right, 2, 10, 650)]
    [TestCase(MapButton.Right, 7, 63, 1023)]
    public void Packet_TakesOrStoresOnALeftClickAndDescribesOnARightClick(
        MapButton button,
        int depotPage,
        int cell,
        int value)
    {
        AssertThat(DepotClicks.Packet(button, DepotNumber, depotPage, cell))
            .IsEqual(ClientPackets.ShortAndInt(ClientCommand.Shop, DepotNumber, value));
    }

    [TestCase]
    public void Packet_SendsNothingForOtherButtons()
    {
        AssertThat(DepotClicks.Packet(MapButton.None, DepotNumber, 0, 0)).IsNull();
    }
}
