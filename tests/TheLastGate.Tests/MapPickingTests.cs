using TheLastGate.World;

namespace TheLastGate.Tests;

public class MapPickingTests
{
    [Fact]
    public void TileAt_FindsThePlayersTileAtTheMiddleOfItsDiamond()
    {
        Assert.Equal((27, 27), MapPicking.TileAt(638, 452));
    }

    [Fact]
    public void TileAt_FindsTheNextTileAHalfTileAcrossAndAQuarterDown()
    {
        Assert.Equal((28, 27), MapPicking.TileAt(654, 460));
    }

    [Fact]
    public void TileAt_GivesAPixelOnAnEdgeToTheTileAfterIt()
    {
        Assert.Equal((27, 27), MapPicking.TileAt(643, 457));
        Assert.Equal((28, 27), MapPicking.TileAt(644, 457));
    }

    [Fact]
    public void TileAt_IncludesTheOuterRowsOfTheClickableArea()
    {
        Assert.Equal((6, 27), MapPicking.TileAt(304, 284));
        Assert.Equal((48, 27), MapPicking.TileAt(973, 619));
    }

    [Fact]
    public void TileAt_CutsOffTheOuterRowsWhereTheCurrentClientDoes()
    {
        Assert.Null(MapPicking.TileAt(298, 282));
        Assert.Null(MapPicking.TileAt(978, 622));
    }

    [Fact]
    public void TileAt_FindsNothingAwayFromTheMap()
    {
        Assert.Null(MapPicking.TileAt(0, 0));
        Assert.Null(MapPicking.TileAt(1279, 719));
    }

    [Fact]
    public void TileAt_CanPickTheTileDrawnUnderTheMouseWhileThePlayerIsMidStep()
    {
        Assert.Equal((28, 27), MapPicking.TileAt(638, 452, 16, 8));
    }
}
