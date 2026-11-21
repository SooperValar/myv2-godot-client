using GdUnit4;
using TheLastGate.Items;
using TheLastGate.Net;
using TheLastGate.State;
using TheLastGate.Windows;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Windows;

[TestSuite]
public class DepotTests
{
    private const int DepotNumber = 0x8000 | 1201;

    private DepotPage page = new();

    [BeforeTest]
    public void ShowTheFirstPage() => page = new DepotPage();

    [TestCase]
    public void Current_StartsAtTheFirstPage()
    {
        AssertThat(page.Current).IsEqual(0);
    }

    [TestCase]
    public void Show_GoesToThePageClicked()
    {
        page.Show(5);

        AssertThat(page.Current).IsEqual(5);
    }

    [TestCase]
    public void Turn_MovesAPageAtATime()
    {
        page.Show(3);

        page.Turn(1);
        AssertThat(page.Current).IsEqual(4);
        page.Turn(-1);
        AssertThat(page.Current).IsEqual(3);
    }

    [TestCase]
    public void Turn_StopsAtTheFirstAndLastPage()
    {
        page.Turn(-1);
        AssertThat(page.Current).IsEqual(0);

        page.Show(DepotPackets.Pages - 1);
        page.Turn(1);
        AssertThat(page.Current).IsEqual(DepotPackets.Pages - 1);
    }

    [TestCase]
    public void Stored_OverlaysEachMarkAndTheStack()
    {
        AssertThat(ItemOverlays.Stored(new DepotItem(1000, 7, 1 | 2 | 4, 0)))
            .ContainsExactly(
                new ItemOverlay(4496, 0),
                new ItemOverlay(4497, 0),
                new ItemOverlay(6881, 0),
                new ItemOverlay(4000 + 7, 0));
    }

    [TestCase]
    public void Stored_ShowsStacksOfOneToTen()
    {
        AssertThat(ItemOverlays.Stored(new DepotItem(1000, 0, 0, 0))).IsEmpty();
        AssertThat(ItemOverlays.Stored(new DepotItem(1000, 11, 0, 0))).IsEmpty();
        AssertThat(ItemOverlays.Stored(new DepotItem(1000, 10, 0, 0))).ContainsExactly(new ItemOverlay(4010, 0));
    }

    [TestCase]
    public void Stored_OverlaysCatalystsAndCorrupters()
    {
        AssertThat(ItemOverlays.Stored(new DepotItem(16971, 0, 0, 3))).ContainsExactly(new ItemOverlay(6999 + 3, 0));
        AssertThat(ItemOverlays.Stored(new DepotItem(6880, 0, 0, 0x25)))
            .ContainsExactly(new ItemOverlay(7799, 0), new ItemOverlay(7802, 5), new ItemOverlay(7805, 10));
    }

    [TestCase]
    public void Stored_LeavesAnEmptySlotBare()
    {
        AssertThat(ItemOverlays.Stored(new DepotItem(0, 7, 7, 3))).IsEmpty();
    }
}
