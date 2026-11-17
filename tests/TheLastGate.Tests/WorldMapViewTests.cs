using TheLastGate.Map;

namespace TheLastGate.Tests;

public class WorldMapViewTests
{
    private const double Width = 800;
    private const double Height = 600;
    private const int Precision = 9;

    private readonly WorldMapView view = new() { CentreTileX = 512, CentreTileY = 1024, PixelsPerTile = 4 };

    [Fact]
    public void MapY_RunsAcrossTheScreenAndMapXDown()
    {
        var across = view.TileToPanel(Width, Height, 512, 1034);
        var down = view.TileToPanel(Width, Height, 522, 1024);

        Assert.Equal(440, across.X, Precision);
        Assert.Equal(300, across.Y, Precision);
        Assert.Equal(400, down.X, Precision);
        Assert.Equal(340, down.Y, Precision);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(123.25, 456.5)]
    [InlineData(799, 599)]
    public void PanelToTile_UndoesTileToPanel(double panelX, double panelY)
    {
        var tile = view.PanelToTile(Width, Height, panelX, panelY);
        var back = view.TileToPanel(Width, Height, tile.X, tile.Y);

        Assert.Equal(panelX, back.X, Precision);
        Assert.Equal(panelY, back.Y, Precision);
    }

    [Theory]
    [InlineData(1024, 1024)]
    [InlineData(2048, 512)]
    public void TheSmallestZoom_FitsTheWholeWorld(double panelWidth, double panelHeight)
    {
        Assert.Equal(0.5, WorldMapView.MinPixelsPerTile(panelWidth, panelHeight), Precision);
    }

    [Fact]
    public void Clamp_KeepsTheZoomAndCentreInRange()
    {
        view.PixelsPerTile = 100;
        view.Clamp(Width, Height);
        Assert.Equal(16, view.PixelsPerTile, Precision);

        view.PixelsPerTile = 0.01;
        view.Clamp(Width, Height);
        Assert.Equal(WorldMapView.MinPixelsPerTile(Width, Height), view.PixelsPerTile, Precision);

        view.CentreTileX = -50;
        view.CentreTileY = 5000;
        view.Clamp(Width, Height);
        Assert.Equal(0, view.CentreTileX, Precision);
        Assert.Equal(2048, view.CentreTileY, Precision);
    }

    [Fact]
    public void ZoomAt_KeepsTheTileUnderTheCursor()
    {
        var before = view.PanelToTile(Width, Height, 100, 50);

        view.ZoomAt(Width, Height, 100, 50, 2);

        Assert.Equal(8, view.PixelsPerTile, Precision);
        var after = view.PanelToTile(Width, Height, 100, 50);
        Assert.Equal(before.X, after.X, Precision);
        Assert.Equal(before.Y, after.Y, Precision);
    }

    [Fact]
    public void ZoomAt_TheLimits_DoesNotDrift()
    {
        view.PixelsPerTile = WorldMapView.MaxPixelsPerTile;
        view.ZoomAt(Width, Height, 100, 50, 2);
        Assert.Equal(WorldMapView.MaxPixelsPerTile, view.PixelsPerTile, Precision);
        Assert.Equal((512.0, 1024.0), (view.CentreTileX, view.CentreTileY));

        view.PixelsPerTile = WorldMapView.MinPixelsPerTile(Width, Height);
        view.ZoomAt(Width, Height, 100, 50, 0.5);
        Assert.Equal(WorldMapView.MinPixelsPerTile(Width, Height), view.PixelsPerTile, Precision);
        Assert.Equal(512, view.CentreTileX, Precision);
        Assert.Equal(1024, view.CentreTileY, Precision);
    }

    [Fact]
    public void AnEmptyPanel_StaysFinite()
    {
        view.PixelsPerTile = 0;

        view.Clamp(0, 0);

        Assert.True(view.PixelsPerTile > 0);
        Assert.True(double.IsFinite(view.PixelsPerTile));
        var tile = view.PanelToTile(0, 0, 10, 10);
        Assert.True(double.IsFinite(tile.X) && double.IsFinite(tile.Y));
    }

    [Fact]
    public void Pan_MovesTheCentreAgainstTheDrag()
    {
        view.Pan(Width, Height, 40, 20);

        Assert.Equal(1014, view.CentreTileY, Precision);
        Assert.Equal(507, view.CentreTileX, Precision);
    }

    [Fact]
    public void CentreOn_PutsTheMiddleOfATileInTheMiddleAtTheOpeningZoom()
    {
        view.CentreOn(300, 700);

        Assert.Equal((300.5, 700.5, 8.0), (view.CentreTileX, view.CentreTileY, view.PixelsPerTile));
    }

    [Fact]
    public void MoveTo_FollowsATileKeepingTheZoom()
    {
        view.MoveTo(300, 700);

        Assert.Equal((300.5, 700.5, 4.0), (view.CentreTileX, view.CentreTileY, view.PixelsPerTile));
    }

    [Fact]
    public void TileUnder_FindsTheTileUnderAPanelPoint()
    {
        Assert.Equal((512, 1024), view.TileUnder(Width, Height, 400, 300));
        Assert.Equal((511, 1023), view.TileUnder(Width, Height, 399, 299));
    }

    [Fact]
    public void TileUnder_IsNothingOffTheWorld()
    {
        view.CentreOn(0, 0);

        Assert.Null(view.TileUnder(Width, Height, 0, 0));
    }
}
