using TheLastGate.Items;

namespace TheLastGate.Tests;

public class InventoryScrollTests
{
    [Fact]
    public void LastRow_LeavesTheFinalThreeRowsOfSixtySlotsInView()
    {
        Assert.Equal(3, InventoryScroll.LastRow);
    }

    [Fact]
    public void SlotShownIn_StartsAtTheTop()
    {
        var scroll = new InventoryScroll();

        Assert.Equal(0, scroll.Row);
        Assert.Equal(0, scroll.SlotShownIn(0));
        Assert.Equal(29, scroll.SlotShownIn(29));
    }

    [Fact]
    public void SlotShownIn_MovesTenSlotsForEachRowScrolled()
    {
        var scroll = new InventoryScroll();

        scroll.ScrollBy(2);

        Assert.Equal(20, scroll.SlotShownIn(0));
        Assert.Equal(49, scroll.SlotShownIn(29));
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(5, 3)]
    public void ScrollBy_StopsAtTheFirstAndLastRows(int rows, int row)
    {
        var scroll = new InventoryScroll();

        scroll.ScrollBy(rows);

        Assert.Equal(row, scroll.Row);
    }

    [Theory]
    [InlineData(2, 2)]
    [InlineData(-4, 0)]
    [InlineData(9, 3)]
    public void ScrollTo_KeepsTheRowInRange(int requested, int row)
    {
        var scroll = new InventoryScroll();

        scroll.ScrollTo(requested);

        Assert.Equal(row, scroll.Row);
    }
}
