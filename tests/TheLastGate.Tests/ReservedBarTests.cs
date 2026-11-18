using TheLastGate.Hud;

namespace TheLastGate.Tests;

public class ReservedBarTests
{
    private const int NameplateBar = 24;

    [Theory]
    [InlineData(0, 0)]
    [InlineData(10, 3)]
    [InlineData(50, 13)]
    [InlineData(100, 24)]
    public void Width_RoundsTheReservedPartUpByOnePixel(int reservedPercent, int width)
    {
        Assert.Equal(width, ReservedBar.Width(NameplateBar, reservedPercent));
    }

    [Fact]
    public void Width_NeverRunsPastTheBar()
    {
        Assert.Equal(NameplateBar, ReservedBar.Width(NameplateBar, 99));
    }
}
