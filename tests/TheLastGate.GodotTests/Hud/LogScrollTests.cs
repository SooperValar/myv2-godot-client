using GdUnit4;
using TheLastGate.Hud;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Hud;

[TestSuite]
public class LogScrollTests
{
    private const int LongHistory = 500;

    [TestCase]
    public void LinesBack_StartsAtTheNewestLine()
    {
        AssertThat(new LogScroll().LinesBack).IsEqual(0);
    }

    [TestCase]
    public void Scroll_MovesBackAndForward()
    {
        var scroll = ScrollWithHistory(LongHistory);

        scroll.Scroll(LogScroll.PageLines);
        scroll.Scroll(LogScroll.WheelLines);
        scroll.Scroll(-LogScroll.WheelLines);

        AssertThat(scroll.LinesBack).IsEqual(LogScroll.PageLines);
    }

    [TestCase]
    public void Scroll_StopsAtTheNewestLine()
    {
        var scroll = ScrollWithHistory(LongHistory);
        scroll.Scroll(LogScroll.WheelLines);

        scroll.Scroll(-LogScroll.PageLines);

        AssertThat(scroll.LinesBack).IsEqual(0);
    }

    [TestCase]
    public void Scroll_GoesBackNoFurtherThanTheCurrentClientAllows()
    {
        var scroll = ScrollWithHistory(LongHistory);

        for (var page = 0; page < 30; page++)
        {
            scroll.Scroll(LogScroll.PageLines);
        }

        AssertThat(scroll.LinesBack).IsEqual(LogScroll.MaxLinesBack);
    }

    [TestCase]
    public void Scroll_GoesBackNoFurtherThanTheOldestLine()
    {
        var scroll = ScrollWithHistory(20);

        scroll.Scroll(LogScroll.PageLines);
        scroll.Scroll(LogScroll.PageLines);

        AssertThat(scroll.LinesBack).IsEqual(20);
    }

    [TestCase]
    public void Advance_ReturnsToTheNewestLineThirtySecondsAfterTheLastScroll()
    {
        var scroll = ScrollWithHistory(LongHistory);
        scroll.Scroll(LogScroll.PageLines);

        scroll.Advance(TimeSpan.FromSeconds(29.9));
        AssertThat(scroll.LinesBack).IsEqual(LogScroll.PageLines);

        scroll.Advance(TimeSpan.FromSeconds(0.1));
        AssertThat(scroll.LinesBack).IsEqual(0);
    }

    [TestCase]
    public void Scroll_StartsTheThirtySecondsAgain()
    {
        var scroll = ScrollWithHistory(LongHistory);
        scroll.Scroll(LogScroll.PageLines);
        scroll.Advance(TimeSpan.FromSeconds(20));

        scroll.Scroll(LogScroll.WheelLines);
        scroll.Advance(TimeSpan.FromSeconds(20));

        AssertThat(scroll.LinesBack).IsEqual(LogScroll.PageLines + LogScroll.WheelLines);
    }

    [TestCase]
    public void LinesChanged_KeepsTheSameLinesInViewWhileScrolledBack()
    {
        var scroll = ScrollWithHistory(LongHistory);
        scroll.Scroll(LogScroll.PageLines);

        scroll.LinesChanged(3, LongHistory + 3);

        AssertThat(scroll.LinesBack).IsEqual(LogScroll.PageLines + 3);
    }

    [TestCase]
    public void LinesChanged_KeepsTheLatestLinesInViewAtTheNewestLine()
    {
        var scroll = ScrollWithHistory(LongHistory);

        scroll.LinesChanged(3, LongHistory + 3);

        AssertThat(scroll.LinesBack).IsEqual(0);
    }

    [TestCase]
    public void LinesChanged_HoldsStillPastTheScrollLimit()
    {
        var scroll = ScrollWithHistory(LongHistory);
        scroll.Scroll(LogScroll.MaxLinesBack);

        scroll.LinesChanged(10, LongHistory + 10);

        AssertThat(scroll.LinesBack).IsEqual(LogScroll.MaxLinesBack + 10);
    }

    [TestCase]
    public void LinesChanged_StopsAtTheOldestLineStillKept()
    {
        var scroll = ScrollWithHistory(30);
        scroll.Scroll(25);

        scroll.LinesChanged(10, 28);

        AssertThat(scroll.LinesBack).IsEqual(28);
    }

    private static LogScroll ScrollWithHistory(int oldestLinesBack)
    {
        var scroll = new LogScroll();
        scroll.LinesChanged(0, oldestLinesBack);
        return scroll;
    }
}
