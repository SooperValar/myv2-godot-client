using TheLastGate.Map;
using TheLastGate.Notes;

namespace TheLastGate.Tests;

public class WorldMapLabelsTests
{
    private const double Width = 800;
    private const double Height = 600;

    private readonly NoteSet notes = new();
    private readonly WorldMapView view = new() { CentreTileX = 512, CentreTileY = 1024, PixelsPerTile = 4 };

    [Fact]
    public void PinCentre_IsTheMiddleOfItsTileOnWholePixels()
    {
        var pin = AddPin(512, 1024, "pin");

        Assert.Equal((402f, 302f), WorldMapLabels.PinCentre(view, Width, Height, pin));
    }

    [Fact]
    public void Place_PutsAPinsNameToTheRightOfItsMarker()
    {
        var pin = AddPin(512, 1024, "pin");

        var placed = Place();

        Assert.Equal(new LabelRect(410, 295, 451, 308), placed[pin.Id]);
    }

    [Fact]
    public void Place_PutsAnEditedPinsNameBesideWhereItIsBeingMoved()
    {
        var pin = AddPin(512, 1024, "pin");

        var placed = WorldMapLabels.Place(notes, view, Width, Height, Measure, edited: pin with { TileY = 1034 });

        Assert.Equal(new LabelRect(450, 295, 491, 308), placed[pin.Id]);
    }

    [Fact]
    public void Place_ShowsNoNamesWhenZoomedFarOut()
    {
        AddPin(512, 1024, "pin");
        view.PixelsPerTile = 2.6;

        Assert.Empty(Place());
        Assert.True(WorldMapLabels.NamesHidden(view));
    }

    [Fact]
    public void Place_KeepsNamesOffOtherPinsMarkers()
    {
        var first = AddPin(512, 1024, "first");
        AddPin(512, 1027, "second");

        Assert.Equal(353, Place()[first.Id].Left);
    }

    [Fact]
    public void Place_KeepsNamesOffTextLabels()
    {
        var pin = AddPin(512, 1024, "pin");
        var text = notes.FindNote(notes.AddPoint(notes.Categories[0].Id, NoteKind.Text, 512, 1026, "note"))!;
        var (plate, _, _) = WorldMapLabels.TextPlate(view, Width, Height, text, Measure(text.Label));

        var placed = Place();

        Assert.False(placed[pin.Id].Overlaps(plate));
    }

    [Fact]
    public void Place_GivesTheSelectedPinTheFirstChoiceOfSpot()
    {
        var first = AddPin(512, 1024, "first");
        var second = AddPin(512, 1024, "second");

        Assert.Equal(410, Place()[first.Id].Left);
        Assert.Equal(410, Place(second.Id)[second.Id].Left);
        Assert.Equal(353, Place(second.Id)[first.Id].Left);
    }

    [Fact]
    public void Place_SkipsPinsWithoutNamesOrWithNamesTurnedOff()
    {
        AddPin(512, 1024, "");
        var parent = notes.Categories[0].Id;
        var child = notes.AddCategory("Child", 0, parent);
        notes.AddPoint(child, NoteKind.Pin, 512, 1030, "hidden name");
        notes.FindCategory(parent)!.ShowLabels = false;

        Assert.Empty(Place());
    }

    [Fact]
    public void Place_SkipsHiddenCategoriesButNotPinsFarOffThePanel()
    {
        var shown = notes.AddCategory("Shown", 0);
        var far = notes.AddPoint(shown, NoteKind.Pin, 512, 1024 + 200, "far");
        var nearby = notes.AddPoint(shown, NoteKind.Pin, 512, 1024 + 150, "nearby");
        var hidden = notes.AddCategory("Hidden", 0);
        notes.AddPoint(hidden, NoteKind.Pin, 512, 1024, "hidden");
        notes.FindCategory(hidden)!.Visible = false;

        Assert.Equal([far, nearby], Place().Keys.Order());
    }

    // A pin past the edge of the panel takes room beside its neighbour whether or not it is in sight.
    [Theory]
    [InlineData(0.0)]
    [InlineData(10.0)]
    [InlineData(-37.3)]
    [InlineData(0.6)]
    public void Panning_LeavesEveryNameWhereItWasBesideItsPin(double panTiles)
    {
        var edge = AddPin(512, 1024 + 160, "edge");
        AddPin(512, 1024 + 166, "beyond");
        view.CentreTileY += panTiles;

        var name = Place()[edge.Id];

        var (centreX, centreY) = WorldMapLabels.PinCentre(view, Width, Height, edge);
        Assert.Equal((-49f, -7f), (name.Left - centreX, name.Top - centreY));
    }

    [Fact]
    public void Layout_PlacesNamesAroundTheirPinsWithoutThePanel()
    {
        var pin = AddPin(512, 1024, "pin");

        var layout = WorldMapLabels.Layout(notes, view.PixelsPerTile, Measure);

        Assert.Equal(new LabelRect(8, -7, 49, 6), layout[pin.Id]);
    }

    [Fact]
    public void TextPlate_CentresTheTextOnItsTileWithPadding()
    {
        var text = notes.FindNote(notes.AddPoint(notes.AddCategory("Text", 0), NoteKind.Text, 512, 1026, "note"))!;

        var (plate, textX, textY) = WorldMapLabels.TextPlate(view, Width, Height, text, (40, 12));

        Assert.Equal((390f, 296f), (textX, textY));
        Assert.Equal(new LabelRect(386, 294, 434, 310), plate);
    }

    private static (float Width, float Height) Measure(string text) => (40, 12);

    private Note AddPin(int tileX, int tileY, string label)
    {
        var category = notes.Categories.Count > 0 ? notes.Categories[0].Id : notes.AddCategory("Pins", 0);
        return notes.FindNote(notes.AddPoint(category, NoteKind.Pin, tileX, tileY, label))!;
    }

    private IReadOnlyDictionary<int, LabelRect> Place(int selectedNoteId = 0) =>
        WorldMapLabels.Place(notes, view, Width, Height, Measure, selectedNoteId);
}
