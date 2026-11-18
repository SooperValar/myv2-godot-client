using TheLastGate.Map;
using TheLastGate.Notes;
using TheLastGate.State;
using TheLastGate.Windows;

namespace TheLastGate.Tests;

public class WorldMapEditorTests
{
    private const double Width = 800;
    private const double Height = 600;

    private readonly WorldMapEditor editor;
    private readonly NoteSet notes = new();
    private readonly WorldMapPanel panel;
    private readonly List<string> problems = [];
    private readonly WorldMapView view = new() { CentreTileX = 512, CentreTileY = 1024, PixelsPerTile = 4 };
    private readonly WorldMapWindow window = new();

    public WorldMapEditorTests()
    {
        editor = window.Editor;
        editor.Problem += problems.Add;
        panel = new WorldMapPanel(view, Width, Height);
    }

    [Fact]
    public void NearestTileUnder_HoldsACursorOffTheWorldToItsEdge()
    {
        var (offX, offY) = view.TileToPanel(Width, Height, -5.5, 1000.5);
        var (onX, onY) = view.TileToPanel(Width, Height, 500.5, 1000.5);

        Assert.Equal((0, 1000, false), view.NearestTileUnder(Width, Height, offX, offY));
        Assert.Equal((500, 1000, true), view.NearestTileUnder(Width, Height, onX, onY));
    }

    [Fact]
    public void SnapToEnd_FindsTheNearestEndWithinReach()
    {
        PathSegment[] segments = [new(90, 100, 100, 140), new(100, 140, 120, 160)];

        Assert.Equal((100, 140), WorldMapView.SnapToEnd(segments, 101, 141, 2));
        Assert.Null(WorldMapView.SnapToEnd(segments, 101, 141, 1));
    }

    [Fact]
    public void EnsureCurrentCategory_PicksTheFirstCategory()
    {
        var first = notes.AddCategory("First", 0);
        notes.AddCategory("Second", 1);

        editor.EnsureCurrentCategory(notes);

        Assert.Equal(first, editor.CurrentCategoryId);
    }

    [Fact]
    public void EnsureCurrentCategory_PutsDownADrawingToolWithNoCategoryToDrawInto()
    {
        editor.ChooseTool(WorldMapTool.Pin);

        editor.EnsureCurrentCategory(notes);

        Assert.Equal(0, editor.CurrentCategoryId);
        Assert.Equal(WorldMapTool.Select, editor.Tool);
    }

    [Fact]
    public void PinTool_AClickAsksForANameThenAddsThePin()
    {
        var category = UseCategory();
        editor.ChooseTool(WorldMapTool.Pin);

        Click(500, 1000);
        Assert.Equal(NoteLabelPurpose.Pin, editor.LabelPurpose);
        editor.ConfirmLabel(notes, "Camp");

        var pin = Assert.Single(notes.FindCategory(category)!.Notes);
        Assert.Equal((NoteKind.Pin, "Camp", 500, 1000), (pin.Kind, pin.Label, pin.TileX, pin.TileY));
        Assert.Equal(NoteLabelPurpose.None, editor.LabelPurpose);
    }

    [Theory]
    [InlineData(2, NoteLabelPurpose.Pin)]
    [InlineData(3, NoteLabelPurpose.None)]
    public void PinTool_StillPlacesAPinWhenTheMouseMovesACoupleOfPixels(double nudge, NoteLabelPurpose asked)
    {
        UseCategory();
        editor.ChooseTool(WorldMapTool.Pin);

        Click(500, 1000, nudge);

        Assert.Equal(asked, editor.LabelPurpose);
    }

    [Fact]
    public void PinTool_PlacesNothingOffTheWorld()
    {
        UseCategory();
        editor.ChooseTool(WorldMapTool.Pin);

        Click(-5, 1000);

        Assert.Equal(NoteLabelPurpose.None, editor.LabelPurpose);
    }

    [Fact]
    public void TextTool_NeedsSomeText()
    {
        UseCategory();
        editor.ChooseTool(WorldMapTool.Text);

        Click(500, 1000);

        Assert.Equal(NoteLabelPurpose.Text, editor.LabelPurpose);
        Assert.False(editor.CanConfirmLabel(""));
        Assert.True(editor.CanConfirmLabel("Here"));
    }

    [Fact]
    public void APinMayHaveNoName()
    {
        UseCategory();
        editor.ChooseTool(WorldMapTool.Pin);

        Click(500, 1000);

        Assert.True(editor.CanConfirmLabel(""));
    }

    [Fact]
    public void CancelLabel_AddsNothing()
    {
        var category = UseCategory();
        editor.ChooseTool(WorldMapTool.Pin);
        Click(500, 1000);

        editor.CancelLabel();

        Assert.Empty(notes.FindCategory(category)!.Notes);
        Assert.Equal(NoteLabelPurpose.None, editor.LabelPurpose);
    }

    [Fact]
    public void ConfirmLabel_SaysWhenTheCategoryIsFull()
    {
        var category = UseCategory();
        for (var count = 0; count < NoteLimits.NotesPerCategory; count++)
        {
            notes.AddPoint(category, NoteKind.Pin, 10, count, "");
        }

        editor.ChooseTool(WorldMapTool.Pin);
        Click(500, 1000);
        editor.ConfirmLabel(notes, "Camp");

        Assert.Equal(["Category is full."], problems);
    }

    [Fact]
    public void LineTool_ADragAddsASegmentToThePathBeingDrawn()
    {
        var category = UseCategory();
        editor.ChooseTool(WorldMapTool.Line);

        Drag(500, 1000, 500, 1010);

        Assert.Equal([new PathSegment(500, 1000, 500, 1010)], editor.PendingSegments);
        Assert.Equal(category, editor.PendingCategoryId);
        Assert.Empty(notes.FindCategory(category)!.Notes);
    }

    [Fact]
    public void LineTool_SnapsBothEndsOfASegmentToTheEndsAlreadyDrawn()
    {
        UseCategory();
        editor.ChooseTool(WorldMapTool.Line);
        Drag(500, 1000, 500, 1010);

        Drag(501, 1011, 501, 999);

        Assert.Equal(new PathSegment(500, 1010, 500, 1000), editor.PendingSegments[1]);
    }

    [Fact]
    public void LineTool_AClickAddsNoSegment()
    {
        UseCategory();
        editor.ChooseTool(WorldMapTool.Line);

        Click(500, 1000, 2);

        Assert.Empty(editor.PendingSegments);
    }

    [Fact]
    public void LineTool_SaysWhenTheCategoryHasNoRoomForAnotherSegment()
    {
        var category = UseCategory();
        var longPath = Enumerable.Range(0, NoteLimits.SegmentsPerPath)
            .Select(step => new PathSegment(5, step, 6, step))
            .ToArray();
        for (var count = 0; count < NoteLimits.SegmentsPerCategory / NoteLimits.SegmentsPerPath; count++)
        {
            notes.AddPath(category, longPath, "");
        }

        editor.ChooseTool(WorldMapTool.Line);
        Drag(500, 1000, 500, 1010);

        Assert.Empty(editor.PendingSegments);
        Assert.Equal(["Category is full."], problems);
    }

    [Fact]
    public void LineTool_SaysWhenThePathIsFull()
    {
        UseCategory();
        editor.ChooseTool(WorldMapTool.Line);
        for (var step = 0; step <= NoteLimits.SegmentsPerPath; step++)
        {
            var row = 10 + step / 100 * 10;
            var column = 10 + step % 100 * 5;
            Drag(row, column, row + 3, column);
        }

        Assert.Equal(NoteLimits.SegmentsPerPath, editor.PendingSegments.Count);
        Assert.Equal(["Path is full."], problems);
    }

    [Fact]
    public void Preview_ShowsTheSegmentBeingDraggedOutToWhereItWouldSnap()
    {
        UseCategory();
        editor.ChooseTool(WorldMapTool.Line);
        Drag(500, 1000, 500, 1010);
        var (pressX, pressY) = At(505, 1005);
        var (mouseX, mouseY) = At(501, 1011);

        editor.PressLeft(panel, pressX, pressY);
        Assert.Null(editor.Preview(panel, pressX, pressY));
        editor.MoveMouse(panel, mouseX, mouseY);

        Assert.Equal(new PathSegment(505, 1005, 500, 1010), editor.Preview(panel, mouseX, mouseY));
    }

    [Fact]
    public void Preview_IsOnlyForDrawingSegments()
    {
        UseCategory();
        var (pressX, pressY) = At(505, 1005);
        var (mouseX, mouseY) = At(510, 1005);

        editor.PressLeft(panel, pressX, pressY);
        editor.MoveMouse(panel, mouseX, mouseY);

        Assert.Null(editor.Preview(panel, mouseX, mouseY));
    }

    [Fact]
    public void FinishPendingPath_AsksForTheNameThenAddsThePath()
    {
        var category = UseCategory();
        editor.ChooseTool(WorldMapTool.Line);
        Drag(500, 1000, 500, 1010);
        Drag(500, 1010, 510, 1010);
        editor.Focused = true;

        Assert.True(editor.FinishPendingPath());
        Assert.Equal(NoteLabelPurpose.Path, editor.LabelPurpose);
        editor.ConfirmLabel(notes, "Road");

        var road = Assert.Single(notes.FindCategory(category)!.Notes);
        Assert.Equal("Road", road.Label);
        Assert.Equal([new PathSegment(500, 1000, 500, 1010), new PathSegment(500, 1010, 510, 1010)], road.Segments);
        Assert.Empty(editor.PendingSegments);
    }

    [Fact]
    public void FinishPendingPath_WaitsForTheMapToHaveFocus()
    {
        UseCategory();
        editor.ChooseTool(WorldMapTool.Line);
        Drag(500, 1000, 500, 1010);
        editor.Focused = false;

        Assert.False(editor.FinishPendingPath());
        Assert.Equal(NoteLabelPurpose.None, editor.LabelPurpose);
    }

    [Fact]
    public void PuttingDownTheLineTool_AsksForThePathsName()
    {
        UseCategory();
        editor.ChooseTool(WorldMapTool.Line);
        Drag(500, 1000, 500, 1010);

        editor.ChooseTool(WorldMapTool.Select);

        Assert.Equal(NoteLabelPurpose.Path, editor.LabelPurpose);
    }

    [Fact]
    public void CancellingThatName_ThrowsThePathAway()
    {
        UseCategory();
        editor.ChooseTool(WorldMapTool.Line);
        Drag(500, 1000, 500, 1010);
        editor.ChooseTool(WorldMapTool.Select);

        editor.CancelLabel();

        Assert.Empty(editor.PendingSegments);
    }

    [Fact]
    public void DiscardPendingPath_ThrowsAwayThePathBeingDrawn()
    {
        UseCategory();
        editor.ChooseTool(WorldMapTool.Line);
        Drag(500, 1000, 500, 1010);

        Assert.True(editor.DiscardPendingPath());
        Assert.Empty(editor.PendingSegments);
        Assert.False(editor.DiscardPendingPath());
    }

    [Fact]
    public void Escape_ThrowsAwayAPathBeingDrawnBeforeClosingTheMap()
    {
        var state = new GameState();
        var map = state.WorldMapWindow;
        map.OpenAtPlayer();
        UseCategory(map.Editor);
        map.Editor.ChooseTool(WorldMapTool.Line);
        Drag(500, 1000, 500, 1010, map.Editor);

        Assert.True(state.CloseWindowsByEscape());
        Assert.True(map.IsOpen);
        Assert.Empty(map.Editor.PendingSegments);

        Assert.True(state.CloseWindowsByEscape());
        Assert.False(map.IsOpen);
    }

    [Fact]
    public void Closing_KeepsAPathWaitingForItsNameAndGoesBackToTheLineTool()
    {
        UseCategory();
        window.OpenAtPlayer();
        editor.ChooseTool(WorldMapTool.Line);
        Drag(500, 1000, 500, 1010);
        editor.ChooseTool(WorldMapTool.Select);

        window.Close();

        Assert.Equal(WorldMapTool.Line, editor.Tool);
        Assert.Single(editor.PendingSegments);
        Assert.Equal(NoteLabelPurpose.None, editor.LabelPurpose);
    }

    [Fact]
    public void Opening_GivesTheMapFocus()
    {
        window.OpenAtPlayer();

        Assert.True(editor.Focused);
    }

    [Fact]
    public void SelectTool_AClickSelectsTheNoteUnderTheCursorOrNothing()
    {
        var category = UseCategory();
        var pin = notes.AddPoint(category, NoteKind.Pin, 500, 1000, "Camp");

        Click(500, 1000, 2);
        Assert.Equal(pin, editor.SelectedNoteId);

        Click(520, 1000);
        Assert.Equal(0, editor.SelectedNoteId);
    }

    [Fact]
    public void EditingAPin_DraggingMovesOnlyTheCopyUntilSaved()
    {
        var category = UseCategory();
        var pin = notes.AddPoint(category, NoteKind.Pin, 500, 1000, "Camp");
        editor.BeginEdit(notes, pin);

        Drag(500, 1000, 505, 1003);

        Assert.Equal((505, 1003), (editor.Edited!.TileX, editor.Edited.TileY));
        Assert.Equal((500, 1000), (notes.FindNote(pin)!.TileX, notes.FindNote(pin)!.TileY));

        editor.SaveEdit(notes);

        Assert.Null(editor.Edited);
        Assert.Equal((505, 1003), (notes.FindNote(pin)!.TileX, notes.FindNote(pin)!.TileY));
    }

    [Fact]
    public void BeginEdit_PicksUpTheSelectToolAndSelectsTheNote()
    {
        var category = UseCategory();
        var pin = notes.AddPoint(category, NoteKind.Pin, 500, 1000, "Camp");
        editor.ChooseTool(WorldMapTool.Pin);

        editor.BeginEdit(notes, pin);

        Assert.Equal(WorldMapTool.Select, editor.Tool);
        Assert.Equal(pin, editor.SelectedNoteId);
    }

    [Fact]
    public void CancelEdit_LeavesTheNoteAsItWas()
    {
        var category = UseCategory();
        var pin = notes.AddPoint(category, NoteKind.Pin, 500, 1000, "Camp");
        editor.BeginEdit(notes, pin);
        Drag(500, 1000, 505, 1003);
        editor.RenameEdited("Moved");

        editor.CancelEdit();

        Assert.Null(editor.Edited);
        Assert.Equal(
            ("Camp", 500, 1000),
            (notes.FindNote(pin)!.Label, notes.FindNote(pin)!.TileX, notes.FindNote(pin)!.TileY));
    }

    [Fact]
    public void RenameEdited_RenamesTheNoteWhenSaved()
    {
        var category = UseCategory();
        var pin = notes.AddPoint(category, NoteKind.Pin, 500, 1000, "Camp");
        editor.BeginEdit(notes, pin);

        editor.RenameEdited("Old camp");
        editor.SaveEdit(notes);

        Assert.Equal("Old camp", notes.FindNote(pin)!.Label);
    }

    [Fact]
    public void CanSaveEdit_NotForTextWithNoText()
    {
        var category = UseCategory();
        var text = notes.AddPoint(category, NoteKind.Text, 500, 1000, "Here");
        editor.BeginEdit(notes, text);

        editor.RenameEdited("");

        Assert.False(editor.CanSaveEdit);
    }

    [Fact]
    public void EditingAPath_ADragAddsASegmentAndAClickRemovesOne()
    {
        var category = UseCategory();
        var road = notes.AddPath(category, [new PathSegment(500, 1000, 500, 1010)], "Road");
        editor.BeginEdit(notes, road);

        Drag(500, 1010, 505, 1010);
        Click(500, 1005);

        Assert.Equal([new PathSegment(500, 1010, 505, 1010)], editor.Edited!.Segments);
        Assert.Single(notes.FindNote(road)!.Segments);

        editor.SaveEdit(notes);

        Assert.Equal([new PathSegment(500, 1010, 505, 1010)], notes.FindNote(road)!.Segments);
    }

    [Fact]
    public void SavingAPathWithNoSegmentsLeft_DeletesIt()
    {
        var category = UseCategory();
        var road = notes.AddPath(category, [new PathSegment(500, 1000, 500, 1010)], "Road");
        editor.BeginEdit(notes, road);
        Click(500, 1005);

        editor.SaveEdit(notes);

        Assert.Null(notes.FindNote(road));
        Assert.Equal(0, editor.SelectedNoteId);
    }

    [Fact]
    public void WhileEditing_ClickingAnotherNoteSelectsItAndStopsEditing()
    {
        var category = UseCategory();
        var first = notes.AddPoint(category, NoteKind.Pin, 500, 1000, "First");
        var second = notes.AddPoint(category, NoteKind.Pin, 520, 1000, "Second");
        editor.BeginEdit(notes, first);

        Click(520, 1000);

        Assert.Equal(second, editor.SelectedNoteId);
        Assert.Null(editor.Edited);
    }

    [Fact]
    public void DrawingCategoryId_IsTheEditedPathsCategory()
    {
        UseCategory();
        var other = notes.AddCategory("Other", 1);
        var road = notes.AddPath(other, [new PathSegment(500, 1000, 500, 1010)], "Road");

        editor.BeginEdit(notes, road);

        Assert.Equal(other, editor.DrawingCategoryId(notes));
    }

    [Fact]
    public void Delete_RemovesTheNoteAndForgetsIt()
    {
        var category = UseCategory();
        var pin = notes.AddPoint(category, NoteKind.Pin, 500, 1000, "Camp");
        editor.BeginEdit(notes, pin);

        editor.Delete(notes, pin);

        Assert.Null(notes.FindNote(pin));
        Assert.Null(editor.Edited);
        Assert.Equal(0, editor.SelectedNoteId);
    }

    [Fact]
    public void DeleteSegment_LeavesTheRestOfThePath()
    {
        var category = UseCategory();
        var road = notes.AddPath(
            category,
            [new PathSegment(500, 1000, 500, 1010), new PathSegment(500, 1010, 510, 1010)],
            "Road");

        editor.DeleteSegment(notes, road, 0);

        Assert.Equal([new PathSegment(500, 1010, 510, 1010)], notes.FindNote(road)!.Segments);
    }

    [Fact]
    public void DeleteSegment_OfAOneSegmentPath_DeletesThePath()
    {
        var category = UseCategory();
        var road = notes.AddPath(category, [new PathSegment(500, 1000, 500, 1010)], "Road");

        editor.DeleteSegment(notes, road, 0);

        Assert.Null(notes.FindNote(road));
    }

    [Fact]
    public void Recolour_AlsoRecoloursTheEditedCopy()
    {
        var category = UseCategory();
        var pin = notes.AddPoint(category, NoteKind.Pin, 500, 1000, "Camp");
        editor.BeginEdit(notes, pin);

        editor.Recolour(notes, pin, 3);

        Assert.Equal(3, notes.FindNote(pin)!.ColourIndex);
        Assert.Equal(3, editor.Edited!.ColourIndex);
    }

    [Fact]
    public void SendTo_MovesTheNoteAndStopsEditingIt()
    {
        var category = UseCategory();
        var other = notes.AddCategory("Other", 1);
        var pin = notes.AddPoint(category, NoteKind.Pin, 500, 1000, "Camp");
        editor.BeginEdit(notes, pin);

        editor.SendTo(notes, pin, other);

        Assert.Equal(other, notes.OwnerOf(pin)!.Id);
        Assert.Null(editor.Edited);
    }

    [Fact]
    public void SendTo_SaysWhenThatCategoryIsFull()
    {
        var category = UseCategory();
        var other = notes.AddCategory("Other", 1);
        for (var count = 0; count < NoteLimits.NotesPerCategory; count++)
        {
            notes.AddPoint(other, NoteKind.Pin, 10, count, "");
        }

        var pin = notes.AddPoint(category, NoteKind.Pin, 500, 1000, "Camp");

        editor.SendTo(notes, pin, other);

        Assert.Equal(category, notes.OwnerOf(pin)!.Id);
        Assert.Equal(["That category is full."], problems);
    }

    [Fact]
    public void ARightClick_FindsTheNoteForItsMenu()
    {
        var category = UseCategory();
        var road = notes.AddPath(
            category,
            [new PathSegment(500, 1000, 500, 1010), new PathSegment(500, 1010, 510, 1010)],
            "Road");
        var (pointX, pointY) = At(505, 1010);

        editor.PressRight(pointX, pointY);
        editor.MoveMouse(panel, pointX + 2, pointY);

        Assert.Equal(new NoteHit(road, 1), editor.ReleaseRight(notes, panel, pointX + 2, pointY));
    }

    [Fact]
    public void ARightDrag_PansInsteadOfOpeningTheMenu()
    {
        var category = UseCategory();
        notes.AddPoint(category, NoteKind.Pin, 500, 1000, "Camp");
        var (pointX, pointY) = At(500, 1000);

        editor.PressRight(pointX, pointY);
        editor.MoveMouse(panel, pointX + 10, pointY);
        editor.MoveMouse(panel, pointX, pointY);

        Assert.Null(editor.ReleaseRight(notes, panel, pointX, pointY));
    }

    private int UseCategory(WorldMapEditor? used = null)
    {
        var category = notes.Categories.Count > 0 ? notes.Categories[0].Id : notes.AddCategory("Notes", 0);
        (used ?? editor).EnsureCurrentCategory(notes);
        return category;
    }

    private (double X, double Y) At(int tileX, int tileY) => view.TileToPanel(Width, Height, tileX + 0.5, tileY + 0.5);

    private void Click(int tileX, int tileY, double nudge = 0)
    {
        var (pointX, pointY) = At(tileX, tileY);
        editor.PressLeft(panel, pointX, pointY);
        editor.MoveMouse(panel, pointX + nudge, pointY);
        editor.ReleaseLeft(notes, panel, pointX + nudge, pointY);
    }

    private void Drag(int fromX, int fromY, int toX, int toY, WorldMapEditor? used = null)
    {
        var dragging = used ?? editor;
        var (startX, startY) = At(fromX, fromY);
        var (endX, endY) = At(toX, toY);
        dragging.PressLeft(panel, startX, startY);
        dragging.MoveMouse(panel, endX, endY);
        dragging.ReleaseLeft(notes, panel, endX, endY);
    }
}
