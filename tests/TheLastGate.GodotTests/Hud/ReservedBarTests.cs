using GdUnit4;
using TheLastGate.Hud;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Hud;

[TestSuite]
public class ReservedBarTests
{
    private const int NameplateBar = 24;

    [TestCase(0, 0)]
    [TestCase(10, 3)]
    [TestCase(50, 13)]
    [TestCase(100, 24)]
    public void Width_RoundsTheReservedPartUpByOnePixel(int reservedPercent, int width)
    {
        AssertThat(ReservedBar.Width(NameplateBar, reservedPercent)).IsEqual(width);
    }

    [TestCase]
    public void Width_NeverRunsPastTheBar()
    {
        AssertThat(ReservedBar.Width(NameplateBar, 99)).IsEqual(NameplateBar);
    }
}
