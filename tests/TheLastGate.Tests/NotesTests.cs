using TheLastGate.Notes;

namespace TheLastGate.Tests;

public class NotesTests
{
    private static readonly PathSegment[] TwoSegments = [new(1, 2, 3, 4), new(3, 4, 5, 6)];

    private readonly NoteSet set = new();

    [Fact]
    public void CategoriesAndNotes_ShareOneIdCounterThatNeverReuses()
    {
        var category = set.AddCategory("Keys", 0);
        var firstPin = set.AddPoint(category, NoteKind.Pin, 10, 20, "a");
        set.RemoveNote(firstPin);

        var secondPin = set.AddPoint(category, NoteKind.Pin, 10, 20, "b");

        Assert.Equal(1, category);
        Assert.Equal(2, firstPin);
        Assert.Equal(3, secondPin);
    }

    [Theory]
    [InlineData(999, 1, 1)]
    [InlineData(1, 2048, 1)]
    [InlineData(1, 1, -1)]
    public void AddPoint_RefusesAnUnknownCategoryOrATileOffTheMap(int category, int tileX, int tileY)
    {
        set.AddCategory("Keys", 0);

        Assert.Equal(0, set.AddPoint(category, NoteKind.Pin, tileX, tileY, "x"));
    }

    [Fact]
    public void Text_NeedsALabelButAPinDoesNot()
    {
        var category = set.AddCategory("Notes", 0);

        Assert.Equal(0, set.AddPoint(category, NoteKind.Text, 1, 1, ""));
        Assert.NotEqual(0, set.AddPoint(category, NoteKind.Pin, 1, 1, ""));

        var text = set.AddPoint(category, NoteKind.Text, 1, 1, "Gate");
        Assert.False(set.ReplaceNote(text, set.FindNote(text)! with { Label = "" }));
        Assert.Equal("Gate", set.FindNote(text)!.Label);
    }

    [Fact]
    public void AddPoint_OnlyMakesPinsAndText()
    {
        var category = set.AddCategory("Notes", 0);

        Assert.Equal(0, set.AddPoint(category, NoteKind.Path, 1, 1, "x"));
    }

    [Fact]
    public void AddPath_KeepsItsOwnCopyOfTheSegments()
    {
        var category = set.AddCategory("Routes", 1);
        var segments = TwoSegments.ToArray();
        var path = set.AddPath(category, segments, "road");

        segments[0] = new PathSegment(900, 2, 3, 4);

        var found = set.FindNote(path)!;
        Assert.Equal(NoteKind.Path, found.Kind);
        Assert.Equal(TwoSegments, found.Segments);
        Assert.Equal((1, 2), (found.TileX, found.TileY));
    }

    [Fact]
    public void AddPath_RefusesNoSegmentsTooManyOrOffTheMap()
    {
        var category = set.AddCategory("Routes", 1);

        Assert.Equal(0, set.AddPath(category, [], "empty"));
        Assert.Equal(0, set.AddPath(category, LongPath(NoteLimits.SegmentsPerPath + 1), "long"));
        Assert.Equal(0, set.AddPath(category, [new PathSegment(1, 1, 2048, 1)], "off"));
    }

    [Fact]
    public void ACategory_HoldsAtMost256Notes()
    {
        var category = set.AddCategory("Many", 0);
        for (var count = 0; count < NoteLimits.NotesPerCategory; count++)
        {
            Assert.NotEqual(0, set.AddPoint(category, NoteKind.Pin, 1, 1, "p"));
        }

        Assert.Equal(0, set.AddPoint(category, NoteKind.Pin, 1, 1, "p"));
    }

    [Fact]
    public void ALongLabel_IsCutBetweenCharacters()
    {
        var category = set.AddCategory("Text", 0);

        var accented = set.AddPoint(category, NoteKind.Text, 3, 3, new string('a', 46) + "é");
        var exact = set.AddPoint(category, NoteKind.Text, 3, 3, new string('b', 47));

        Assert.Equal(new string('a', 46), set.FindNote(accented)!.Label);
        Assert.Equal(new string('b', 47), set.FindNote(exact)!.Label);
    }

    [Fact]
    public void ReplaceNote_KeepsTheIdAndKind()
    {
        var category = set.AddCategory("Routes", 0);
        var path = set.AddPath(category, TwoSegments, "old");
        var edited = set.FindNote(path)! with { Segments = TwoSegments[..1], Label = "new" };

        Assert.True(set.ReplaceNote(path, edited));

        var stored = set.FindNote(path)!;
        Assert.Equal(path, stored.Id);
        Assert.Equal(NoteKind.Path, stored.Kind);
        Assert.Single(stored.Segments);
        Assert.Equal("new", stored.Label);
    }

    [Fact]
    public void ReplaceNote_MovesAPathsAnchorToItsFirstSegment()
    {
        var category = set.AddCategory("Routes", 0);
        var path = set.AddPath(category, TwoSegments, "route");

        set.ReplaceNote(path, set.FindNote(path)! with { Segments = TwoSegments[1..] });

        Assert.Equal((3, 4), (set.FindNote(path)!.TileX, set.FindNote(path)!.TileY));
    }

    [Fact]
    public void ReplaceNote_MovesAPinToItsNewTile()
    {
        var category = set.AddCategory("Keys", 0);
        var pin = set.AddPoint(category, NoteKind.Pin, 1, 1, "a");

        Assert.True(set.ReplaceNote(pin, set.FindNote(pin)! with { TileX = 7, TileY = 8 }));
        Assert.False(set.ReplaceNote(pin, set.FindNote(pin)! with { TileX = 2048 }));

        Assert.Equal((7, 8), (set.FindNote(pin)!.TileX, set.FindNote(pin)!.TileY));
    }

    [Fact]
    public void RemovingACategory_RemovesItsNotes()
    {
        var keep = set.AddCategory("Keep", 0);
        var doomed = set.AddCategory("Doomed", 1);
        var pin = set.AddPoint(doomed, NoteKind.Pin, 4, 4, "x");

        Assert.True(set.RemoveCategory(doomed));

        Assert.Null(set.FindNote(pin));
        Assert.NotNull(set.FindCategory(keep));
    }

    [Fact]
    public void AddingANote_MarksTheSetChanged()
    {
        var category = set.AddCategory("Keys", 0);
        set.Changed = false;

        set.AddPoint(category, NoteKind.Pin, 1, 1, "x");

        Assert.True(set.Changed);
    }

    [Fact]
    public void ThePalette_FallsBackToItsFirstColour()
    {
        Assert.Equal(0xFFD34Du, NotePalette.Rgb(1));
        Assert.Equal(0x4FC3FFu, NotePalette.Rgb(99));
        Assert.Equal(0x4FC3FFu, NotePalette.Rgb(-1));
    }

    [Fact]
    public void Count_CountsEachKind()
    {
        var category = set.AddCategory("Mixed", 0);
        set.AddPoint(category, NoteKind.Pin, 1, 1, "a");
        set.AddPoint(category, NoteKind.Pin, 2, 2, "b");
        set.AddPoint(category, NoteKind.Text, 3, 3, "c");
        set.AddPath(category, TwoSegments[..1], "d");

        var mixed = set.FindCategory(category)!;
        Assert.Equal(2, mixed.Count(NoteKind.Pin));
        Assert.Equal(1, mixed.Count(NoteKind.Text));
        Assert.Equal(1, mixed.Count(NoteKind.Path));
    }

    [Fact]
    public void ACategory_HoldsAtMost4096PathSegments()
    {
        var flood = set.AddCategory("Flood", 0);
        var other = set.AddCategory("Other", 0);
        for (var path = 0; path < NoteLimits.SegmentsPerCategory / NoteLimits.SegmentsPerPath; path++)
        {
            Assert.NotEqual(0, set.AddPath(flood, LongPath(NoteLimits.SegmentsPerPath), "flood"));
        }

        Assert.Equal(NoteLimits.SegmentsPerCategory, set.FindCategory(flood)!.SegmentTotal);
        Assert.Equal(0, set.AddPath(flood, TwoSegments[..1], "one more"));
        Assert.NotEqual(0, set.AddPath(other, TwoSegments[..1], "elsewhere"));
    }

    [Fact]
    public void ReplaceNote_RefusesGrowingAPathPastTheCategorysSegments()
    {
        var flood = set.AddCategory("Flood", 0);
        for (var path = 0; path < NoteLimits.SegmentsPerCategory / NoteLimits.SegmentsPerPath - 1; path++)
        {
            set.AddPath(flood, LongPath(NoteLimits.SegmentsPerPath), "flood");
        }

        set.AddPath(flood, LongPath(NoteLimits.SegmentsPerPath - 1), "almost");
        var small = set.AddPath(flood, TwoSegments[..1], "small");

        Assert.False(set.ReplaceNote(small, set.FindNote(small)! with { Segments = TwoSegments }));
        Assert.True(set.ReplaceNote(small, set.FindNote(small)! with { Label = "renamed" }));
    }

    [Fact]
    public void Attach_GivesTheCategoryAndItsNotesNewIdsUnderItsParent()
    {
        var parent = set.AddCategory("Parent", 0);
        var detached = NoteCategory.Detached("Keys", 2, true, true, [Pin(77, 5, 6)]);

        var attached = set.FindCategory(set.Attach(detached, parent))!;

        Assert.Equal(parent, attached.ParentId);
        Assert.NotEqual(0, attached.Id);
        Assert.NotEqual(77, attached.Notes[0].Id);
        Assert.NotEqual(0, attached.Notes[0].Id);
        Assert.Equal((5, 6), (attached.Notes[0].TileX, attached.Notes[0].TileY));
        Assert.True(attached.Visible);
    }

    [Fact]
    public void Attach_RefusesAnUnknownParent()
    {
        Assert.Equal(0, set.Attach(NoteCategory.Detached("Keys", 0, true, true, []), 999));
    }

    [Fact]
    public void Attach_KeepsAClashingNameAsItIs()
    {
        var upper = set.Attach(NoteCategory.Detached("Aston", 0, true, true, []), 0);
        var lower = set.Attach(NoteCategory.Detached("aston", 0, true, true, []), 0);

        Assert.Equal("Aston", set.FindCategory(upper)!.Name);
        Assert.Equal("aston", set.FindCategory(lower)!.Name);
    }

    [Fact]
    public void NewNotes_TakeTheirCategorysColour()
    {
        var category = set.AddCategory("Keys", 0);

        var pin = set.AddPoint(category, NoteKind.Pin, 1, 1, "a");
        var path = set.AddPath(category, TwoSegments[..1], "b");

        Assert.Equal(Note.CategoryColour, set.FindNote(pin)!.ColourIndex);
        Assert.Equal(Note.CategoryColour, set.FindNote(path)!.ColourIndex);
    }

    [Fact]
    public void SetNoteColour_OverridesTheCategorysColourUntilCleared()
    {
        var category = set.FindCategory(set.AddCategory("A", 3))!;
        var pin = set.AddPoint(category.Id, NoteKind.Pin, 1, 1, "");

        Assert.Equal(NotePalette.Rgb(3), NoteSet.Rgb(category, set.FindNote(pin)!));
        Assert.True(set.SetNoteColour(pin, 2));
        Assert.Equal(NotePalette.Rgb(2), NoteSet.Rgb(category, set.FindNote(pin)!));
        Assert.True(set.SetNoteColour(pin, Note.CategoryColour));
        Assert.Equal(NotePalette.Rgb(3), NoteSet.Rgb(category, set.FindNote(pin)!));
        Assert.False(set.SetNoteColour(pin, 9));
        Assert.False(set.SetNoteColour(999, 1));
    }

    [Fact]
    public void MoveNote_KeepsItsIdAndColourAndRespectsTheLimit()
    {
        var first = set.AddCategory("A", 0);
        var second = set.AddCategory("B", 0);
        var pin = set.AddPoint(first, NoteKind.Pin, 4, 5, "Pin");
        set.SetNoteColour(pin, 4);

        Assert.False(set.MoveNote(pin, first));
        Assert.False(set.MoveNote(pin, 999));
        Assert.True(set.MoveNote(pin, second));

        Assert.Equal(second, set.OwnerOf(pin)!.Id);
        Assert.Equal(4, set.FindNote(pin)!.ColourIndex);
        Assert.Empty(set.FindCategory(first)!.Notes);

        for (var count = 0; count < NoteLimits.NotesPerCategory; count++)
        {
            set.AddPoint(first, NoteKind.Pin, 1, 1, "");
        }

        Assert.False(set.MoveNote(pin, first));
    }

    [Fact]
    public void IsIdenticalTo_ComparesEverythingButTheId()
    {
        var category = set.AddCategory("A", 0);
        var original = set.FindNote(
            set.AddPath(category, [new PathSegment(1, 1, 2, 2), new PathSegment(2, 2, 3, 3)], "Road"))!;

        Assert.True(original.IsIdenticalTo(original with { Id = 999 }));
        Assert.False(original.IsIdenticalTo(original with { Label = "Xoad" }));
        Assert.False(original.IsIdenticalTo(original with { TileX = 2 }));
        Assert.False(original.IsIdenticalTo(original with { ColourIndex = 3 }));
        Assert.False(
            original.IsIdenticalTo(
                original with { Segments = [new PathSegment(1, 1, 2, 2), new PathSegment(2, 2, 3, 4)] }));
        Assert.True(
            original.IsIdenticalTo(
                original with { Segments = [new PathSegment(1, 1, 2, 2), new PathSegment(2, 2, 3, 3)] }));
    }

    private static Note Pin(int id, int tileX, int tileY) =>
        new(NoteKind.Pin, "", tileX, tileY, [], Note.CategoryColour) { Id = id };

    private static PathSegment[] LongPath(int segments) =>
        Enumerable.Range(0, segments).Select(index => new PathSegment(index % 2047, 1, index % 2047 + 1, 1)).ToArray();
}
