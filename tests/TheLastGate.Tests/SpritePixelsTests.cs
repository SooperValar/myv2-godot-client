using TheLastGate.Assets;

namespace TheLastGate.Tests;

public class SpritePixelsTests
{
    [Fact]
    public void Rgb565ToRgba8_WidensChannelsByRepeatingTheirHighBits()
    {
        byte[] source = [0x1F, 0x00, 0xE0, 0x07, 0x00, 0xF8, 0x10, 0x84];
        var destination = new byte[16];

        SpritePixels.Rgb565ToRgba8(source, destination);

        Assert.Equal(
            new byte[] { 0, 0, 255, 255, 0, 255, 0, 255, 255, 0, 0, 255, 132, 130, 132, 255 },
            destination);
    }

    [Fact]
    public void Rgb565ToRgba8_MakesMagentaTransparent()
    {
        byte[] source = [0x1F, 0xF8, 0x1E, 0xF8];
        var destination = new byte[8];

        SpritePixels.Rgb565ToRgba8(source, destination);

        Assert.Equal(0, destination[3]);
        Assert.Equal(255, destination[7]);
    }

    [Fact]
    public void Rgb565ToRgba8_RejectsTooSmallDestination()
    {
        Assert.Throws<ArgumentException>(() => SpritePixels.Rgb565ToRgba8(new byte[4], new byte[7]));
    }

    [Fact]
    public void ApplyMagentaKey_ClearsOnlyOpaqueMagenta()
    {
        byte[] rgba = [255, 0, 255, 255, 255, 0, 255, 128, 255, 1, 255, 255];

        SpritePixels.ApplyMagentaKey(rgba);

        Assert.Equal(new byte[] { 255, 0, 255, 0, 255, 0, 255, 128, 255, 1, 255, 255 }, rgba);
    }
}
