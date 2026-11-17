using TheLastGate.Map;

namespace TheLastGate.Tests;

public class MinimapViewTests
{
    private const int Pixels = MinimapView.Pixels;
    private const int BytesPerPixel = 4;
    private readonly ExploredMap map = new();
    private readonly byte[] rgba = new byte[Pixels * Pixels * BytesPerPixel];

    private readonly MinimapView view = new();

    [Fact]
    public void Magnification_StartsAtTwo()
    {
        Assert.Equal(2, view.Magnification);
    }

    [Fact]
    public void ZoomIn_StepsUpToFour()
    {
        view.ZoomIn(false);
        view.ZoomIn(false);
        view.ZoomIn(false);

        Assert.Equal(4, view.Magnification);
    }

    [Fact]
    public void ZoomOut_StepsDownToOne()
    {
        view.ZoomOut(false);
        view.ZoomOut(false);

        Assert.Equal(1, view.Magnification);
    }

    [Fact]
    public void ZoomingWithAModifierHeld_JumpsToTheEnd()
    {
        view.ZoomIn(true);
        Assert.Equal(4, view.Magnification);

        view.ZoomOut(true);
        Assert.Equal(1, view.Magnification);
    }

    [Theory]
    [InlineData(1, 3)]
    [InlineData(-1, 1)]
    [InlineData(5, 4)]
    [InlineData(-5, 1)]
    public void Scroll_ZoomsInOnTheWheelUpAndOutOnTheWheelDown(int notches, int magnification)
    {
        view.Scroll(notches);

        Assert.Equal(magnification, view.Magnification);
    }

    [Theory]
    [InlineData(2, 468, 668)]
    [InlineData(1, 436, 636)]
    [InlineData(4, 484, 684)]
    public void Origin_PutsThePlayerInTheMiddle(int magnification, int originX, int originY)
    {
        ZoomTo(magnification);

        Assert.Equal((originX, originY), view.Origin(500, 700));
    }

    [Fact]
    public void Origin_StopsAtTheTopLeftOfTheMap()
    {
        Assert.Equal((0, 0), view.Origin(10, 10));
    }

    [Theory]
    [InlineData(1, 896, 1920)]
    [InlineData(2, 960, 1984)]
    [InlineData(3, 981, 2005)]
    [InlineData(4, 992, 2016)]
    public void Origin_ShowsTheLastRowAndColumnOfTheWorld(int magnification, int originX, int originY)
    {
        ZoomTo(magnification);

        Assert.Equal((originX, originY), view.Origin(1023, 2047));
    }

    [Fact]
    public void Render_DrawsEachTileAsAMagnifiedBlockWithMapXRunningDown()
    {
        var (originX, originY) = view.Origin(500, 700);
        map.SetTile(originX + 1, originY + 2, 0x112233);

        view.Render(map, 500, 700, rgba);

        Assert.Equal(new byte[] { 0x11, 0x22, 0x33, 255 }, PixelAt(2, 4));
        Assert.Equal(new byte[] { 0x11, 0x22, 0x33, 255 }, PixelAt(3, 5));
        Assert.Equal(new byte[] { 0, 0, 0, 255 }, PixelAt(2, 6));
    }

    [Fact]
    public void Render_ShowsThePlayerInWhiteWithoutChangingTheMap()
    {
        map.SetTile(500, 700, 0x445566);

        view.Render(map, 500, 700, rgba);

        Assert.Equal(new byte[] { 255, 255, 255, 255 }, PixelAt(64, 64));
        Assert.Equal(0x445566u, map.ColourAt(500, 700));
    }

    [Fact]
    public void Render_ShowsUnexploredTilesInOpaqueBlack()
    {
        view.Render(map, 500, 700, rgba);

        Assert.Equal(new byte[] { 0, 0, 0, 255 }, PixelAt(0, 0));
    }

    [Theory]
    [InlineData(MinimapButton.ZoomIn, "Increase minimap magnification.")]
    [InlineData(MinimapButton.ZoomOut, "Decrease minimap magnification.")]
    [InlineData(MinimapButton.Map, "Open the world map.")]
    public void Help_UsesTheCurrentClientsWords(MinimapButton button, string help)
    {
        Assert.Equal(help, MinimapView.Help(button));
    }

    private void ZoomTo(int magnification)
    {
        view.ZoomOut(true);
        for (var step = 1; step < magnification; step++)
        {
            view.ZoomIn(false);
        }
    }

    private byte[] PixelAt(int row, int column) =>
        rgba.AsSpan((row * Pixels + column) * BytesPerPixel, BytesPerPixel).ToArray();
}
