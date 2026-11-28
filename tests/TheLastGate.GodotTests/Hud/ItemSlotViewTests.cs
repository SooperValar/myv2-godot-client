using GdUnit4;
using Godot;
using TheLastGate.Hud;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Hud;

[TestSuite]
[RequireGodotRuntime]
public class ItemSlotViewTests
{
    private const int SpriteSide = 32;

    [TestCase(22, 7, -10)]
    [TestCase(1, 30, 0)]
    [TestCase(4, 6, 9)]
    public void ContentLift_MovesTheDrawnRowsToTheMiddle(int firstRow, int rowCount, int lift)
    {
        AssertThat(ItemSlotView.ContentLift(SpriteWithRows(firstRow, rowCount))).IsEqual(lift);
    }

    [TestCase]
    public void ContentLift_LeavesAnEmptySpriteWhereItIs()
    {
        AssertThat(ItemSlotView.ContentLift(SpriteWithRows(0, 0))).IsEqual(0);
    }

    private static ImageTexture SpriteWithRows(int firstRow, int rowCount)
    {
        var image = Image.CreateEmpty(SpriteSide, SpriteSide, false, Image.Format.Rgba8);
        for (var row = firstRow; row < firstRow + rowCount; row++)
        {
            for (var column = 0; column < SpriteSide; column++)
            {
                image.SetPixel(column, row, Colors.White);
            }
        }

        return ImageTexture.CreateFromImage(image);
    }
}
