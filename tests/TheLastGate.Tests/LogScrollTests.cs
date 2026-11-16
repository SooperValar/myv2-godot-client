using TheLastGate.Hud;

namespace TheLastGate.Tests;

public class LogScrollTests
{
    private const int LongHistory = 500;

    [Fact]
    public void LinesBack_StartsAtTheNewestLine()
    {
        Assert.Equal(0, new LogScroll().LinesBack);
    }

    [Fact]
    public void Scroll_MovesBackAndForward()
    {
        var scroll = ScrollWithHistory(LongHistory);

        scroll.Scroll(LogScroll.PageLines);
        scroll.Scroll(LogScroll.WheelLines);
        scroll.Scroll(-LogScroll.WheelLines);

        Assert.Equal(LogScroll.PageLines, scroll.LinesBack);
    }

    [Fact]
    public void Scroll_StopsAtTheNewestLine()
    {
        var scroll = ScrollWithHistory(LongHistory);
        scroll.Scroll(LogScroll.WheelLines);

        scroll.Scroll(-LogScroll.PageLines);

        Assert.Equal(0, scroll.LinesBack);
    }

    [Fact]
    public void Scroll_GoesBackNoFurtherThanTheCurrentClientAllows()
    {
        var scroll = ScrollWithHistory(LongHistory);

        for (var page = 0; page < 30; page++)
        {
            scroll.Scroll(LogScroll.PageLines);
        }

        Assert.Equal(LogScroll.MaxLinesBack, scroll.LinesBack);
    }

    [Fact]
    public void Scroll_GoesBackNoFurtherThanTheOldestLine()
    {
        var scroll = ScrollWithHistory(20);

        scroll.Scroll(LogScroll.PageLines);
        scroll.Scroll(LogScroll.PageLines);

        Assert.Equal(20, scroll.LinesBack);
    }

    [Fact]
    public void Advance_ReturnsToTheNewestLineThirtySecondsAfterTheLastScroll()
    {
        var scroll = ScrollWithHistory(LongHistory);
        scroll.Scroll(LogScroll.PageLines);

        scroll.Advance(TimeSpan.FromSeconds(29.9));
        Assert.Equal(LogScroll.PageLines, scroll.LinesBack);

        scroll.Advance(TimeSpan.FromSeconds(0.1));
        Assert.Equal(0, scroll.LinesBack);
    }

    [Fact]
    public void Scroll_StartsTheThirtySecondsAgain()
    {
        var scroll = ScrollWithHistory(LongHistory);
        scroll.Scroll(LogScroll.PageLines);
        scroll.Advance(TimeSpan.FromSeconds(20));

        scroll.Scroll(LogScroll.WheelLines);
        scroll.Advance(TimeSpan.FromSeconds(20));

        Assert.Equal(LogScroll.PageLines + LogScroll.WheelLines, scroll.LinesBack);
    }

    [Fact]
    public void LinesChanged_KeepsTheSameLinesInViewWhileScrolledBack()
    {
        var scroll = ScrollWithHistory(LongHistory);
        scroll.Scroll(LogScroll.PageLines);

        scroll.LinesChanged(3, LongHistory + 3);

        Assert.Equal(LogScroll.PageLines + 3, scroll.LinesBack);
    }

    [Fact]
    public void LinesChanged_KeepsTheLatestLinesInViewAtTheNewestLine()
    {
        var scroll = ScrollWithHistory(LongHistory);

        scroll.LinesChanged(3, LongHistory + 3);

        Assert.Equal(0, scroll.LinesBack);
    }

    [Fact]
    public void LinesChanged_HoldsStillPastTheScrollLimit()
    {
        var scroll = ScrollWithHistory(LongHistory);
        scroll.Scroll(LogScroll.MaxLinesBack);

        scroll.LinesChanged(10, LongHistory + 10);

        Assert.Equal(LogScroll.MaxLinesBack + 10, scroll.LinesBack);
    }

    [Fact]
    public void LinesChanged_StopsAtTheOldestLineStillKept()
    {
        var scroll = ScrollWithHistory(30);
        scroll.Scroll(25);

        scroll.LinesChanged(10, 28);

        Assert.Equal(28, scroll.LinesBack);
    }

    private static LogScroll ScrollWithHistory(int oldestLinesBack)
    {
        var scroll = new LogScroll();
        scroll.LinesChanged(0, oldestLinesBack);
        return scroll;
    }
}
