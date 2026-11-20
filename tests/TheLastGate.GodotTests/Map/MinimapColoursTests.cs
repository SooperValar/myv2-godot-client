using GdUnit4;
using TheLastGate.Map;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Map;

[TestSuite]
public class MinimapColoursTests
{
    [TestCase]
    public void FromRgb_PacksRedGreenAndBlue()
    {
        AssertThat(MinimapColours.FromRgb(0x12, 0x34, 0x56)).IsEqual(0x123456u);
    }

    [TestCase((ushort)(31 << 10), 0xFF0000u)]
    [TestCase((ushort)(31 << 5), 0x00FF00u)]
    [TestCase((ushort)31, 0x0000FFu)]
    [TestCase((ushort)(16 << 10), 0x840000u)]
    [TestCase((ushort)1, 0x000008u)]
    public void FromRgb555_SpreadsEachFiveBitsOverAByte(ushort packed, uint colour)
    {
        AssertThat(MinimapColours.FromRgb555(packed)).IsEqual(colour);
    }
}
