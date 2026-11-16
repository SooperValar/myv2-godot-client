using TheLastGate.Hud;

namespace TheLastGate.Tests;

public class PortraitBarsTests
{
    [Theory]
    [InlineData(100, 100, 62)]
    [InlineData(50, 100, 31)]
    [InlineData(1, 3, 20)]
    [InlineData(0, 100, 0)]
    public void For_FillsTheBarInProportion(int current, int maximum, int width)
    {
        Assert.Equal(new BarWidths(62, 0, width), PortraitBars.For(current, maximum, 0));
    }

    [Theory]
    [InlineData(150, 100, 93)]
    [InlineData(200, 100, 124)]
    [InlineData(300, 100, 124)]
    public void For_LetsTheCurrentValueRunPastTheBarUpToTwiceItsLength(int current, int maximum, int width)
    {
        Assert.Equal(width, PortraitBars.For(current, maximum, 0).Current);
    }

    [Fact]
    public void For_DrawsNothingWithoutAMaximum()
    {
        Assert.Equal(new BarWidths(0, 0, 0), PortraitBars.For(5, 0, 0));
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(10, 7)]
    [InlineData(50, 32)]
    [InlineData(99, 62)]
    [InlineData(100, 62)]
    public void For_RoundsTheReservedPartUpByOnePixel(int reservedPercent, int width)
    {
        Assert.Equal(width, PortraitBars.For(100, 100, reservedPercent).Reserved);
    }

    [Fact]
    public void For_DrawsNoNegativeWidths()
    {
        Assert.Equal(new BarWidths(62, 0, 0), PortraitBars.For(-10, 100, -5));
    }
}
