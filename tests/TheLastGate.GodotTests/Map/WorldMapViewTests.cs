using GdUnit4;
using TheLastGate.Map;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Map;

[TestSuite]
public class WorldMapViewTests
{
    private const double Width = 800;
    private const double Height = 600;
    private const int Precision = 9;

    private WorldMapView view = new();

    [BeforeTest]
    public void CentreTheView() => view = new WorldMapView { CentreTileX = 512, CentreTileY = 1024, PixelsPerTile = 4 };

    [TestCase]
    public void MapY_RunsAcrossTheScreenAndMapXDown()
    {
        var across = view.TileToPanel(Width, Height, 512, 1034);
        var down = view.TileToPanel(Width, Height, 522, 1024);

        AssertThat(Math.Round(across.X, Precision)).IsEqual(440.0);
        AssertThat(Math.Round(across.Y, Precision)).IsEqual(300.0);
        AssertThat(Math.Round(down.X, Precision)).IsEqual(400.0);
        AssertThat(Math.Round(down.Y, Precision)).IsEqual(340.0);
    }

    [TestCase(0, 0)]
    [TestCase(123.25, 456.5)]
    [TestCase(799, 599)]
    public void PanelToTile_UndoesTileToPanel(double panelX, double panelY)
    {
        var tile = view.PanelToTile(Width, Height, panelX, panelY);
        var back = view.TileToPanel(Width, Height, tile.X, tile.Y);

        AssertThat(Math.Round(back.X, Precision)).IsEqual(Math.Round(panelX, Precision));
        AssertThat(Math.Round(back.Y, Precision)).IsEqual(Math.Round(panelY, Precision));
    }

    [TestCase(512, 512)]
    [TestCase(1024, 512)]
    public void TheSmallestZoom_FitsTheWholeWorld(double panelWidth, double panelHeight)
    {
        AssertThat(Math.Round(WorldMapView.MinPixelsPerTile(panelWidth, panelHeight), Precision))
            .IsEqual(Math.Round(0.5, Precision));
    }

    [TestCase]
    public void Clamp_KeepsTheZoomAndCentreInRange()
    {
        view.PixelsPerTile = 100;
        view.Clamp(Width, Height);
        AssertThat(Math.Round(view.PixelsPerTile, Precision)).IsEqual(16.0);

        view.PixelsPerTile = 0.01;
        view.Clamp(Width, Height);
        AssertThat(Math.Round(view.PixelsPerTile, Precision))
            .IsEqual(Math.Round(WorldMapView.MinPixelsPerTile(Width, Height), Precision));

        view.CentreTileX = -50;
        view.CentreTileY = 5000;
        view.Clamp(Width, Height);
        AssertThat(Math.Round(view.CentreTileX, Precision)).IsEqual(0.0);
        AssertThat(Math.Round(view.CentreTileY, Precision)).IsEqual(1024.0);
    }

    [TestCase]
    public void ZoomAt_KeepsTheTileUnderTheCursor()
    {
        var before = view.PanelToTile(Width, Height, 100, 50);

        view.ZoomAt(Width, Height, 100, 50, 2);

        AssertThat(Math.Round(view.PixelsPerTile, Precision)).IsEqual(8.0);
        var after = view.PanelToTile(Width, Height, 100, 50);
        AssertThat(Math.Round(after.X, Precision)).IsEqual(Math.Round(before.X, Precision));
        AssertThat(Math.Round(after.Y, Precision)).IsEqual(Math.Round(before.Y, Precision));
    }

    [TestCase]
    public void ZoomAt_TheLimits_DoesNotDrift()
    {
        view.PixelsPerTile = WorldMapView.MaxPixelsPerTile;
        view.ZoomAt(Width, Height, 100, 50, 2);
        AssertThat(Math.Round(view.PixelsPerTile, Precision))
            .IsEqual(Math.Round(WorldMapView.MaxPixelsPerTile, Precision));
        AssertThat((view.CentreTileX, view.CentreTileY)).IsEqual((512.0, 1024.0));

        view.PixelsPerTile = WorldMapView.MinPixelsPerTile(Width, Height);
        view.ZoomAt(Width, Height, 100, 50, 0.5);
        AssertThat(Math.Round(view.PixelsPerTile, Precision))
            .IsEqual(Math.Round(WorldMapView.MinPixelsPerTile(Width, Height), Precision));
        AssertThat(Math.Round(view.CentreTileX, Precision)).IsEqual(512.0);
        AssertThat(Math.Round(view.CentreTileY, Precision)).IsEqual(1024.0);
    }

    [TestCase]
    public void AnEmptyPanel_StaysFinite()
    {
        view.PixelsPerTile = 0;

        view.Clamp(0, 0);

        AssertThat(view.PixelsPerTile > 0).IsTrue();
        AssertThat(double.IsFinite(view.PixelsPerTile)).IsTrue();
        var tile = view.PanelToTile(0, 0, 10, 10);
        AssertThat(double.IsFinite(tile.X) && double.IsFinite(tile.Y)).IsTrue();
    }

    [TestCase]
    public void Pan_MovesTheCentreAgainstTheDrag()
    {
        view.Pan(Width, Height, 40, 20);

        AssertThat(Math.Round(view.CentreTileY, Precision)).IsEqual(1014.0);
        AssertThat(Math.Round(view.CentreTileX, Precision)).IsEqual(507.0);
    }

    [TestCase]
    public void CentreOn_PutsTheMiddleOfATileInTheMiddleAtTheOpeningZoom()
    {
        view.CentreOn(300, 700);

        AssertThat((view.CentreTileX, view.CentreTileY, view.PixelsPerTile)).IsEqual((300.5, 700.5, 8.0));
    }

    [TestCase]
    public void MoveTo_FollowsATileKeepingTheZoom()
    {
        view.MoveTo(300, 700);

        AssertThat((view.CentreTileX, view.CentreTileY, view.PixelsPerTile)).IsEqual((300.5, 700.5, 4.0));
    }

    [TestCase]
    public void TileUnder_FindsTheTileUnderAPanelPoint()
    {
        AssertThat(view.TileUnder(Width, Height, 400, 300)).IsEqual((512, 1024));
        AssertThat(view.TileUnder(Width, Height, 399, 299)).IsEqual((511, 1023));
    }

    [TestCase]
    public void TileUnder_IsNothingOffTheWorld()
    {
        view.CentreOn(0, 0);

        AssertThat(view.TileUnder(Width, Height, 0, 0)).IsNull();
    }
}
