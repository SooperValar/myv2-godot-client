using TheLastGate.Map;

namespace TheLastGate.Tests;

public class SpriteColoursTests
{
    [Fact]
    public void Average_TakesTheMeanOfEachChannelRoundingDown()
    {
        byte[] pixels = [10, 20, 30, 255, 11, 21, 31, 255];

        Assert.Equal(MinimapColours.FromRgb(10, 20, 30), SpriteColours.Average(pixels));
    }

    [Fact]
    public void Average_SkipsMagentaAndTransparentPixels()
    {
        byte[] pixels = [255, 0, 255, 255, 1, 2, 3, 0, 40, 50, 60, 255];

        Assert.Equal(MinimapColours.FromRgb(40, 50, 60), SpriteColours.Average(pixels));
    }

    [Fact]
    public void Average_OfANothingButSeeThroughSprite_IsBlack()
    {
        byte[] pixels = [255, 0, 255, 255, 9, 9, 9, 0];

        Assert.Equal(0u, SpriteColours.Average(pixels));
    }
}
