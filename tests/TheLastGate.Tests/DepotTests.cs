using TheLastGate.Items;
using TheLastGate.Net;
using TheLastGate.State;
using TheLastGate.Windows;
using TheLastGate.World;

namespace TheLastGate.Tests;

public class DepotTests
{
    private const int DepotNumber = 0x8000 | 1201;

    private readonly DepotPage page = new();

    [Fact]
    public void Current_StartsAtTheFirstPage()
    {
        Assert.Equal(0, page.Current);
    }

    [Fact]
    public void Show_GoesToThePageClicked()
    {
        page.Show(5);

        Assert.Equal(5, page.Current);
    }

    [Fact]
    public void Turn_MovesAPageAtATime()
    {
        page.Show(3);

        page.Turn(1);
        Assert.Equal(4, page.Current);
        page.Turn(-1);
        Assert.Equal(3, page.Current);
    }

    [Fact]
    public void Turn_StopsAtTheFirstAndLastPage()
    {
        page.Turn(-1);
        Assert.Equal(0, page.Current);

        page.Show(DepotPage.Pages - 1);
        page.Turn(1);
        Assert.Equal(DepotPage.Pages - 1, page.Current);
    }

    [Theory]
    [InlineData(MapButton.Left, 2, 10, 138)]
    [InlineData(MapButton.Left, 0, 0, 0)]
    [InlineData(MapButton.Right, 2, 10, 650)]
    [InlineData(MapButton.Right, 7, 63, 1023)]
    public void Packet_TakesOrStoresOnALeftClickAndDescribesOnARightClick(
        MapButton button,
        int depotPage,
        int cell,
        int value)
    {
        Assert.Equal(
            ClientPackets.ShortAndInt(ClientCommand.Shop, DepotNumber, value),
            DepotClicks.Packet(button, DepotNumber, depotPage, cell));
    }

    [Fact]
    public void Packet_SendsNothingForOtherButtons()
    {
        Assert.Null(DepotClicks.Packet(MapButton.None, DepotNumber, 0, 0));
    }

    [Fact]
    public void Stored_OverlaysEachMarkAndTheStack()
    {
        Assert.Equal(
            [
                new ItemOverlay(4496, 0),
                new ItemOverlay(4497, 0),
                new ItemOverlay(6881, 0),
                new ItemOverlay(4000 + 7, 0),
            ],
            ItemOverlays.Stored(new DepotItem(1000, 7, 1 | 2 | 4, 0)));
    }

    [Fact]
    public void Stored_ShowsStacksOfOneToTen()
    {
        Assert.Empty(ItemOverlays.Stored(new DepotItem(1000, 0, 0, 0)));
        Assert.Empty(ItemOverlays.Stored(new DepotItem(1000, 11, 0, 0)));
        Assert.Equal([new ItemOverlay(4010, 0)], ItemOverlays.Stored(new DepotItem(1000, 10, 0, 0)));
    }

    [Fact]
    public void Stored_OverlaysCatalystsAndCorrupters()
    {
        Assert.Equal([new ItemOverlay(6999 + 3, 0)], ItemOverlays.Stored(new DepotItem(16971, 0, 0, 3)));
        Assert.Equal(
            [new ItemOverlay(7799, 0), new ItemOverlay(7802, 5), new ItemOverlay(7805, 10)],
            ItemOverlays.Stored(new DepotItem(6880, 0, 0, 0x25)));
    }

    [Fact]
    public void Stored_LeavesAnEmptySlotBare()
    {
        Assert.Empty(ItemOverlays.Stored(new DepotItem(0, 7, 7, 3)));
    }
}
