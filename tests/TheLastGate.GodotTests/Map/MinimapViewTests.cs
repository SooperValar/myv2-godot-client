using GdUnit4;
using TheLastGate.Map;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Map;

[TestSuite]
public class MinimapViewTests
{
    private const int Pixels = MinimapView.Pixels;
    private const int BytesPerPixel = 4;
    private ExploredMap map = new();
    private byte[] rgba = [];
    private MinimapView view = new();

    [BeforeTest]
    public void StartWithABlankMap()
    {
        map = new ExploredMap();
        rgba = new byte[Pixels * Pixels * BytesPerPixel];
        view = new MinimapView();
    }

    [TestCase]
    public void Magnification_StartsAtTwo()
    {
        AssertThat(view.Magnification).IsEqual(2);
    }

    [TestCase]
    public void ZoomIn_StepsUpToFour()
    {
        view.ZoomIn(false);
        view.ZoomIn(false);
        view.ZoomIn(false);

        AssertThat(view.Magnification).IsEqual(4);
    }

    [TestCase]
    public void ZoomOut_StepsDownToOne()
    {
        view.ZoomOut(false);
        view.ZoomOut(false);

        AssertThat(view.Magnification).IsEqual(1);
    }

    [TestCase]
    public void ZoomingWithAModifierHeld_JumpsToTheEnd()
    {
        view.ZoomIn(true);
        AssertThat(view.Magnification).IsEqual(4);

        view.ZoomOut(true);
        AssertThat(view.Magnification).IsEqual(1);
    }

    [TestCase(1, 3)]
    [TestCase(-1, 1)]
    [TestCase(5, 4)]
    [TestCase(-5, 1)]
    public void Scroll_ZoomsInOnTheWheelUpAndOutOnTheWheelDown(int notches, int magnification)
    {
        view.Scroll(notches);

        AssertThat(view.Magnification).IsEqual(magnification);
    }

    [TestCase(2, 468, 668)]
    [TestCase(1, 436, 636)]
    [TestCase(4, 484, 684)]
    public void Origin_PutsThePlayerInTheMiddle(int magnification, int originX, int originY)
    {
        ZoomTo(magnification);

        AssertThat(view.Origin(500, 700)).IsEqual((originX, originY));
    }

    [TestCase]
    public void Origin_StopsAtTheTopLeftOfTheMap()
    {
        AssertThat(view.Origin(10, 10)).IsEqual((0, 0));
    }

    [TestCase(1, 896, 896)]
    [TestCase(2, 960, 960)]
    [TestCase(3, 981, 981)]
    [TestCase(4, 992, 992)]
    public void Origin_ShowsTheLastRowAndColumnOfTheWorld(int magnification, int originX, int originY)
    {
        ZoomTo(magnification);

        AssertThat(view.Origin(1023, 1023)).IsEqual((originX, originY));
    }

    [TestCase]
    public void Render_DrawsEachTileAsAMagnifiedBlockWithMapXRunningDown()
    {
        var (originX, originY) = view.Origin(500, 700);
        map.SetTile(originX + 1, originY + 2, 0x112233);

        view.Render(map, 500, 700, rgba);

        AssertThat(PixelAt(2, 4)).IsEqual(new byte[] { 0x11, 0x22, 0x33, 255 });
        AssertThat(PixelAt(3, 5)).IsEqual(new byte[] { 0x11, 0x22, 0x33, 255 });
        AssertThat(PixelAt(2, 6)).IsEqual(new byte[] { 0, 0, 0, 255 });
    }

    [TestCase]
    public void Render_ShowsThePlayerInWhiteWithoutChangingTheMap()
    {
        map.SetTile(500, 700, 0x445566);

        view.Render(map, 500, 700, rgba);

        AssertThat(PixelAt(64, 64)).IsEqual(new byte[] { 255, 255, 255, 255 });
        AssertThat(map.ColourAt(500, 700)).IsEqual(0x445566u);
    }

    [TestCase]
    public void Render_ShowsUnexploredTilesInOpaqueBlack()
    {
        view.Render(map, 500, 700, rgba);

        AssertThat(PixelAt(0, 0)).IsEqual(new byte[] { 0, 0, 0, 255 });
    }

    [TestCase(MinimapButton.ZoomIn, "Increase minimap magnification.")]
    [TestCase(MinimapButton.ZoomOut, "Decrease minimap magnification.")]
    [TestCase(MinimapButton.Map, "Open the world map.")]
    public void Help_UsesTheCurrentClientsWords(MinimapButton button, string help)
    {
        AssertThat(MinimapView.Help(button)).IsEqual(help);
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
