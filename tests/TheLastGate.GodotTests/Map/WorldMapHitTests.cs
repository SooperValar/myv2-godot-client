using GdUnit4;
using TheLastGate.Map;
using TheLastGate.Notes;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Map;

[TestSuite]
public class WorldMapHitTests
{
    private const double Width = 800;
    private const double Height = 600;
    private const double Radius = 6;

    private NoteSet notes = new();
    private WorldMapView view = new();

    [BeforeTest]
    public void CentreTheView()
    {
        notes = new NoteSet();
        view = new WorldMapView { CentreTileX = 512, CentreTileY = 512, PixelsPerTile = 4 };
    }

    [TestCase]
    public void HitTest_PrefersAPinToAPathUnderIt()
    {
        var category = notes.AddCategory("Mixed", 0);
        AddRoad(category);
        var pin = notes.AddPoint(category, NoteKind.Pin, 500, 503, "pin");

        AssertThat(HitAt(500, 503)).IsEqual(new NoteHit(pin, -1));
    }

    [TestCase]
    public void HitTest_PrefersTextToAPathUnderIt()
    {
        var category = notes.AddCategory("Mixed", 0);
        AddRoad(category);
        var text = notes.AddPoint(category, NoteKind.Text, 500, 503, "note");

        AssertThat(HitAt(500, 503)?.NoteId).IsEqual(text);
    }

    [TestCase]
    public void HitTest_SaysWhichSegmentOfAPathIsUnderTheCursor()
    {
        var road = AddRoad(notes.AddCategory("Roads", 0));

        AssertThat(HitAt(500, 503, offsetY: 2)).IsEqual(new NoteHit(road, 1));
    }

    [TestCase]
    public void HitSegment_FindsTheSegmentOfOnePathOrNone()
    {
        var road = notes.FindNote(AddRoad(notes.AddCategory("Roads", 0)))!;
        var (panelX, panelY) = view.TileToPanel(Width, Height, 500.5, 503.5);

        AssertThat(view.HitSegment(road, Width, Height, panelX, panelY, Radius)).IsEqual(1);
        AssertThat(view.HitSegment(road, Width, Height, panelX, panelY + 100, Radius)).IsEqual(-1);
    }

    [TestCase]
    public void HitTest_SkipsAHiddenCategory()
    {
        var category = notes.AddCategory("Hidden", 0);
        notes.AddPoint(category, NoteKind.Pin, 500, 503, "pin");
        notes.FindCategory(category)!.Visible = false;

        AssertThat(HitAt(500, 503)).IsNull();
    }

    [TestCase]
    public void HitTest_SkipsTheChildOfAHiddenCategory()
    {
        var parent = notes.AddCategory("Parent", 0);
        var child = notes.AddCategory("Child", 0, parent);
        notes.AddPoint(child, NoteKind.Pin, 500, 503, "pin");
        notes.FindCategory(parent)!.Visible = false;

        AssertThat(HitAt(500, 503)).IsNull();
    }

    [TestCase]
    public void HitTest_MissesBeyondTheRadius()
    {
        notes.AddPoint(notes.AddCategory("Pins", 0), NoteKind.Pin, 500, 503, "pin");

        AssertThat(HitAt(500, 503, 20)).IsNull();
    }

    private int AddRoad(int categoryId) =>
        notes.AddPath(
            categoryId,
            [
                new PathSegment(500, 488, 500, 498), new PathSegment(500, 498, 500, 508),
                new PathSegment(500, 508, 500, 518),
            ],
            "road");

    private NoteHit? HitAt(int tileX, int tileY, double offsetX = 0, double offsetY = 0)
    {
        var (panelX, panelY) = view.TileToPanel(Width, Height, tileX + 0.5, tileY + 0.5);
        return view.HitTest(notes, Width, Height, panelX + offsetX, panelY + offsetY, Radius);
    }
}
