using TheLastGate.Map;

namespace TheLastGate.Tests;

public class MinimapColoursTests
{
    [Fact]
    public void FromRgb_PacksRedGreenAndBlue()
    {
        Assert.Equal(0x123456u, MinimapColours.FromRgb(0x12, 0x34, 0x56));
    }

    [Theory]
    [InlineData((ushort)(31 << 10), 0xFF0000u)]
    [InlineData((ushort)(31 << 5), 0x00FF00u)]
    [InlineData((ushort)31, 0x0000FFu)]
    [InlineData((ushort)(16 << 10), 0x840000u)]
    [InlineData((ushort)1, 0x000008u)]
    public void FromRgb555_SpreadsEachFiveBitsOverAByte(ushort packed, uint colour)
    {
        Assert.Equal(colour, MinimapColours.FromRgb555(packed));
    }
}
