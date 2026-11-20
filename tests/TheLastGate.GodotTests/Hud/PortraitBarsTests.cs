using GdUnit4;
using TheLastGate.Hud;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Hud;

[TestSuite]
public class PortraitBarsTests
{
    [TestCase(100, 100, 62)]
    [TestCase(50, 100, 31)]
    [TestCase(1, 3, 20)]
    [TestCase(0, 100, 0)]
    public void For_FillsTheBarInProportion(int current, int maximum, int width)
    {
        AssertThat(PortraitBars.For(current, maximum, 0)).IsEqual(new BarWidths(62, 0, width));
    }

    [TestCase(150, 100, 93)]
    [TestCase(200, 100, 124)]
    [TestCase(300, 100, 124)]
    public void For_LetsTheCurrentValueRunPastTheBarUpToTwiceItsLength(int current, int maximum, int width)
    {
        AssertThat(PortraitBars.For(current, maximum, 0).Current).IsEqual(width);
    }

    [TestCase]
    public void For_DrawsNothingWithoutAMaximum()
    {
        AssertThat(PortraitBars.For(5, 0, 0)).IsEqual(new BarWidths(0, 0, 0));
    }

    [TestCase(1, 1)]
    [TestCase(10, 7)]
    [TestCase(50, 32)]
    [TestCase(99, 62)]
    [TestCase(100, 62)]
    public void For_RoundsTheReservedPartUpByOnePixel(int reservedPercent, int width)
    {
        AssertThat(PortraitBars.For(100, 100, reservedPercent).Reserved).IsEqual(width);
    }

    [TestCase]
    public void For_DrawsNoNegativeWidths()
    {
        AssertThat(PortraitBars.For(-10, 100, -5)).IsEqual(new BarWidths(62, 0, 0));
    }
}
