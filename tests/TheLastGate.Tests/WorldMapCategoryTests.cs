using TheLastGate.Map;
using TheLastGate.Notes;
using TheLastGate.Windows;

namespace TheLastGate.Tests;

public class WorldMapCategoryTests
{
    private readonly WorldMapEditor editor;
    private readonly NoteSet notes = new();
    private readonly List<string> problems = [];
    private readonly WorldMapWindow window = new();

    public WorldMapCategoryTests()
    {
        editor = window.Editor;
        editor.Problem += problems.Add;
    }

    [Fact]
    public void Expand_OpensAndClosesACategory()
    {
        var category = notes.AddCategory("Camps", 0);

        editor.Expand(category, true);
        Assert.Contains(category, editor.ExpandedCategories);

        editor.Expand(category, false);
        Assert.Empty(editor.ExpandedCategories);
    }

    [Fact]
    public void ChooseCategory_IsWhereNewNotesGo()
    {
        notes.AddCategory("First", 0);
        var second = notes.AddCategory("Second", 1);

        editor.ChooseCategory(second);
        editor.EnsureCurrentCategory(notes);

        Assert.Equal(second, editor.CurrentCategoryId);
    }

    [Fact]
    public void CreateCategory_ChoosesItAndOpensItsParent()
    {
        var parent = notes.AddCategory("Parent", 0);

        var created = editor.CreateCategory(notes, "Child", 3, parent);

        Assert.Equal(parent, notes.FindCategory(created)!.ParentId);
        Assert.Equal(3, notes.FindCategory(created)!.ColourIndex);
        Assert.Equal(created, editor.CurrentCategoryId);
        Assert.Contains(parent, editor.ExpandedCategories);
    }

    [Fact]
    public void CreateCategory_SaysWhenItWouldNestTooDeep()
    {
        var parent = 0;
        for (var depth = 0; depth < NoteLimits.Depth; depth++)
        {
            parent = notes.AddCategory($"Level {depth}", 0, parent);
        }

        Assert.Equal(0, editor.CreateCategory(notes, "Too deep", 0, parent));
        Assert.Equal(["Categories can only nest 8 deep."], problems);
    }

    [Fact]
    public void CreateCategory_SaysWhenThereAreTooMany()
    {
        for (var count = 0; count < NoteLimits.Categories; count++)
        {
            notes.AddCategory($"Category {count}", 0);
        }

        Assert.Equal(0, editor.CreateCategory(notes, "One more", 0, 0));
        Assert.Equal(["Too many categories."], problems);
    }

    [Fact]
    public void MoveCategory_OpensItsNewParentAndThatParentsParents()
    {
        var top = notes.AddCategory("Top", 0);
        var middle = notes.AddCategory("Middle", 0, top);
        var moving = notes.AddCategory("Moving", 0);

        editor.MoveCategory(notes, moving, middle);

        Assert.Equal(middle, notes.FindCategory(moving)!.ParentId);
        Assert.Equal([top, middle], editor.ExpandedCategories.Order());
    }

    [Fact]
    public void SendTo_OpensTheCategoryTheNoteWentTo()
    {
        var from = notes.AddCategory("From", 0);
        var top = notes.AddCategory("Top", 0);
        var to = notes.AddCategory("To", 0, top);
        var pin = notes.AddPoint(from, NoteKind.Pin, 10, 10, "Camp");

        editor.SendTo(notes, pin, to);

        Assert.Equal([top, to], editor.ExpandedCategories.Order());
    }

    [Fact]
    public void DeleteCategory_LetsGoOfEverythingInIt()
    {
        var kept = notes.AddCategory("Kept", 0);
        var doomed = notes.AddCategory("Doomed", 0);
        var pin = notes.AddPoint(doomed, NoteKind.Pin, 10, 10, "Camp");
        editor.ChooseCategory(doomed);
        editor.Expand(doomed, true);
        editor.BeginEdit(notes, pin);

        editor.DeleteCategory(notes, doomed);

        Assert.Null(notes.FindCategory(doomed));
        Assert.Null(editor.Edited);
        Assert.Equal(0, editor.SelectedNoteId);
        Assert.Empty(editor.ExpandedCategories);
        editor.EnsureCurrentCategory(notes);
        Assert.Equal(kept, editor.CurrentCategoryId);
    }

    [Fact]
    public void DeleteCategory_ThrowsAwayAPathBeingDrawnIntoIt()
    {
        var doomed = notes.AddCategory("Doomed", 0);
        editor.EnsureCurrentCategory(notes);
        editor.ChooseTool(WorldMapTool.Line);
        DrawOneSegment();

        editor.DeleteCategory(notes, doomed);

        Assert.Empty(editor.PendingSegments);
    }

    [Fact]
    public void RemoveDuplicatePins_SaysHowManyWent()
    {
        var category = notes.AddCategory("Camps", 0);
        notes.AddPoint(category, NoteKind.Pin, 10, 10, "Camp");
        var copy = notes.AddPoint(category, NoteKind.Pin, 10, 10, "Camp");
        editor.Select(copy);

        Assert.Equal(1, editor.RemoveDuplicatePins(notes, category));

        Assert.Equal(["Removed 1 duplicate pin."], problems);
        Assert.Equal(0, editor.SelectedNoteId);
    }

    [Fact]
    public void RemoveDuplicatePins_CountsInThePlural()
    {
        var category = notes.AddCategory("Camps", 0);
        for (var copies = 0; copies < 3; copies++)
        {
            notes.AddPoint(category, NoteKind.Pin, 10, 10, "Camp");
        }

        editor.RemoveDuplicatePins(notes, category);

        Assert.Equal(["Removed 2 duplicate pins."], problems);
    }

    [Fact]
    public void FinishImport_ChoosesAndOpensTheImportedCategory()
    {
        var parent = notes.AddCategory("Shared", 0);

        Assert.True(editor.FinishImport(new NoteImportResult(NoteImportStatus.Ok, 42, ""), parent));

        Assert.Equal(42, editor.CurrentCategoryId);
        Assert.Equal([parent, 42], editor.ExpandedCategories.Order());
    }

    [Fact]
    public void FinishImport_ThatFailed_ChangesNothing()
    {
        Assert.False(editor.FinishImport(new NoteImportResult(NoteImportStatus.TooDeep, 0, ""), 0));

        Assert.Empty(editor.ExpandedCategories);
    }

    [Fact]
    public void ShowNote_AsksTheMapToMoveToItAndStopsFollowing()
    {
        var category = notes.AddCategory("Camps", 0);
        var pin = notes.FindNote(notes.AddPoint(category, NoteKind.Pin, 300, 700, "Camp"))!;
        window.Follow = true;

        window.ShowNote(pin);

        Assert.Equal((300, 700), window.MoveRequest);
        Assert.False(window.Follow);
    }

    [Fact]
    public void ShowNote_ForAPath_MovesToWhereItStarts()
    {
        var category = notes.AddCategory("Roads", 0);
        var road = notes.FindNote(notes.AddPath(category, [new PathSegment(40, 50, 60, 70)], "Road"))!;

        window.ShowNote(road);

        Assert.Equal((40, 50), window.MoveRequest);
    }

    private void DrawOneSegment()
    {
        var view = new WorldMapView { CentreTileX = 512, CentreTileY = 1024, PixelsPerTile = 4 };
        var panel = new WorldMapPanel(view, 800, 600);
        var (startX, startY) = view.TileToPanel(800, 600, 500.5, 1000.5);
        var (endX, endY) = view.TileToPanel(800, 600, 500.5, 1010.5);
        editor.PressLeft(panel, startX, startY);
        editor.MoveMouse(panel, endX, endY);
        editor.ReleaseLeft(notes, panel, endX, endY);
    }
}
