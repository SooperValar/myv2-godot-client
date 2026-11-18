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
    public void Place_SkipsHiddenCategoriesAndPinsFarOffThePanel()
    {
        var shown = notes.AddCategory("Shown", 0);
        notes.AddPoint(shown, NoteKind.Pin, 512, 1024 + 200, "far");
        var nearby = notes.AddPoint(shown, NoteKind.Pin, 512, 1024 + 150, "nearby");
        var hidden = notes.AddCategory("Hidden", 0);
        notes.AddPoint(hidden, NoteKind.Pin, 512, 1024, "hidden");
        notes.FindCategory(hidden)!.Visible = false;

        Assert.Equal(new[] { nearby }, Place().Keys);
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
