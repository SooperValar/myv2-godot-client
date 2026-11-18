using TheLastGate.Map;
using TheLastGate.Notes;

namespace TheLastGate.Tests;

public class WorldMapHitTests
{
    private const double Width = 800;
    private const double Height = 600;
    private const double Radius = 6;

    private readonly NoteSet notes = new();
    private readonly WorldMapView view = new() { CentreTileX = 512, CentreTileY = 1024, PixelsPerTile = 4 };

    [Fact]
    public void HitTest_PrefersAPinToAPathUnderIt()
    {
        var category = notes.AddCategory("Mixed", 0);
        AddRoad(category);
        var pin = notes.AddPoint(category, NoteKind.Pin, 500, 1015, "pin");

        Assert.Equal(new NoteHit(pin, -1), HitAt(500, 1015));
    }

    [Fact]
    public void HitTest_PrefersTextToAPathUnderIt()
    {
        var category = notes.AddCategory("Mixed", 0);
        AddRoad(category);
        var text = notes.AddPoint(category, NoteKind.Text, 500, 1015, "note");

        Assert.Equal(text, HitAt(500, 1015)?.NoteId);
    }

    [Fact]
    public void HitTest_SaysWhichSegmentOfAPathIsUnderTheCursor()
    {
        var road = AddRoad(notes.AddCategory("Roads", 0));

        Assert.Equal(new NoteHit(road, 1), HitAt(500, 1015, offsetY: 2));
    }

    [Fact]
    public void HitSegment_FindsTheSegmentOfOnePathOrNone()
    {
        var road = notes.FindNote(AddRoad(notes.AddCategory("Roads", 0)))!;
        var (panelX, panelY) = view.TileToPanel(Width, Height, 500.5, 1015.5);

        Assert.Equal(1, view.HitSegment(road, Width, Height, panelX, panelY, Radius));
        Assert.Equal(-1, view.HitSegment(road, Width, Height, panelX, panelY + 100, Radius));
    }

    [Fact]
    public void HitTest_SkipsAHiddenCategory()
    {
        var category = notes.AddCategory("Hidden", 0);
        notes.AddPoint(category, NoteKind.Pin, 500, 1015, "pin");
        notes.FindCategory(category)!.Visible = false;

        Assert.Null(HitAt(500, 1015));
    }

    [Fact]
    public void HitTest_SkipsTheChildOfAHiddenCategory()
    {
        var parent = notes.AddCategory("Parent", 0);
        var child = notes.AddCategory("Child", 0, parent);
        notes.AddPoint(child, NoteKind.Pin, 500, 1015, "pin");
        notes.FindCategory(parent)!.Visible = false;

        Assert.Null(HitAt(500, 1015));
    }

    [Fact]
    public void HitTest_MissesBeyondTheRadius()
    {
        notes.AddPoint(notes.AddCategory("Pins", 0), NoteKind.Pin, 500, 1015, "pin");

        Assert.Null(HitAt(500, 1015, 20));
    }

    private int AddRoad(int categoryId) =>
        notes.AddPath(
            categoryId,
            [
                new PathSegment(500, 1000, 500, 1010), new PathSegment(500, 1010, 500, 1020),
                new PathSegment(500, 1020, 500, 1030),
            ],
            "road");

    private NoteHit? HitAt(int tileX, int tileY, double offsetX = 0, double offsetY = 0)
    {
        var (panelX, panelY) = view.TileToPanel(Width, Height, tileX + 0.5, tileY + 0.5);
        return view.HitTest(notes, Width, Height, panelX + offsetX, panelY + offsetY, Radius);
    }
}
