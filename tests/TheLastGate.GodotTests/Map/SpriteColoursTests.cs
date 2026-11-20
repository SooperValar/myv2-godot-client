using GdUnit4;
using TheLastGate.Map;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Map;

[TestSuite]
public class SpriteColoursTests
{
    [TestCase]
    public void Average_TakesTheMeanOfEachChannelRoundingDown()
    {
        byte[] pixels = [10, 20, 30, 255, 11, 21, 31, 255];

        AssertThat(SpriteColours.Average(pixels)).IsEqual(MinimapColours.FromRgb(10, 20, 30));
    }

    [TestCase]
    public void Average_SkipsMagentaAndTransparentPixels()
    {
        byte[] pixels = [255, 0, 255, 255, 1, 2, 3, 0, 40, 50, 60, 255];

        AssertThat(SpriteColours.Average(pixels)).IsEqual(MinimapColours.FromRgb(40, 50, 60));
    }

    [TestCase]
    public void Average_OfANothingButSeeThroughSprite_IsBlack()
    {
        byte[] pixels = [255, 0, 255, 255, 9, 9, 9, 0];

        AssertThat(SpriteColours.Average(pixels)).IsEqual(0u);
    }
}
