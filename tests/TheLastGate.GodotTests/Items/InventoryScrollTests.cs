using GdUnit4;
using TheLastGate.Items;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Items;

[TestSuite]
public class InventoryScrollTests
{
    [TestCase]
    public void LastRow_LeavesTheFinalThreeRowsOfSixtySlotsInView()
    {
        AssertThat(InventoryScroll.LastRow).IsEqual(3);
    }

    [TestCase]
    public void SlotShownIn_StartsAtTheTop()
    {
        var scroll = new InventoryScroll();

        AssertThat(scroll.Row).IsEqual(0);
        AssertThat(scroll.SlotShownIn(0)).IsEqual(0);
        AssertThat(scroll.SlotShownIn(29)).IsEqual(29);
    }

    [TestCase]
    public void SlotShownIn_MovesTenSlotsForEachRowScrolled()
    {
        var scroll = new InventoryScroll();

        scroll.ScrollBy(2);

        AssertThat(scroll.SlotShownIn(0)).IsEqual(20);
        AssertThat(scroll.SlotShownIn(29)).IsEqual(49);
    }

    [TestCase(-1, 0)]
    [TestCase(5, 3)]
    public void ScrollBy_StopsAtTheFirstAndLastRows(int rows, int row)
    {
        var scroll = new InventoryScroll();

        scroll.ScrollBy(rows);

        AssertThat(scroll.Row).IsEqual(row);
    }

    [TestCase(2, 2)]
    [TestCase(-4, 0)]
    [TestCase(9, 3)]
    public void ScrollTo_KeepsTheRowInRange(int requested, int row)
    {
        var scroll = new InventoryScroll();

        scroll.ScrollTo(requested);

        AssertThat(scroll.Row).IsEqual(row);
    }
}
