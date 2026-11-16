using TheLastGate.Stats;

namespace TheLastGate.Tests;

public class SkillScrollTests
{
    [Fact]
    public void LastRow_LeavesTenOfTheFiftyFiveSkillsInView()
    {
        Assert.Equal(45, SkillScroll.LastRow);
    }

    [Theory]
    [InlineData(-5, 0)]
    [InlineData(3, 3)]
    [InlineData(60, 45)]
    public void ScrollTo_StaysWithinTheList(int row, int shownRow)
    {
        var scroll = new SkillScroll();
        scroll.ScrollTo(row);

        Assert.Equal(shownRow, scroll.Row);
    }

    [Fact]
    public void ScrollBy_MovesFromTheCurrentRow()
    {
        var scroll = new SkillScroll();
        scroll.ScrollTo(10);
        scroll.ScrollBy(-1);

        Assert.Equal(9, scroll.Row);
    }

    [Fact]
    public void PositionShownIn_CountsFromTheTopRow()
    {
        var scroll = new SkillScroll();
        scroll.ScrollTo(4);

        Assert.Equal(6, scroll.PositionShownIn(2));
    }
}
