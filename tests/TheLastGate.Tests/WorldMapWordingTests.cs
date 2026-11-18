using TheLastGate.Notes;
using TheLastGate.Windows;

namespace TheLastGate.Tests;

public class WorldMapWordingTests
{
    private readonly NoteSet notes = new();

    [Theory]
    [InlineData(NoteKind.Pin, "Pin: ")]
    [InlineData(NoteKind.Text, "Text: ")]
    [InlineData(NoteKind.Path, "Path: ")]
    public void KindPrefix_StartsEachNoteRow(NoteKind kind, string prefix)
    {
        Assert.Equal(prefix, WorldMapWording.KindPrefix(kind));
    }

    [Fact]
    public void DeleteNoteQuestion_NamesTheNote()
    {
        var category = notes.AddCategory("Camps", 0);
        var text = notes.FindNote(notes.AddPoint(category, NoteKind.Text, 1, 1, "Danger"))!;

        Assert.Equal("Delete text label \"Danger\"?", WorldMapWording.DeleteNoteQuestion(text));
    }

    [Fact]
    public void DeleteNoteQuestion_ForANoteWithNoName()
    {
        var category = notes.AddCategory("Roads", 0);
        var road = notes.FindNote(notes.AddPath(category, [new PathSegment(1, 1, 2, 2)], ""))!;

        Assert.Equal("Delete this unnamed path?", WorldMapWording.DeleteNoteQuestion(road));
    }

    [Fact]
    public void DeleteCategoryQuestion_SaysHowManyNotesGoWithIt()
    {
        var category = notes.AddCategory("Camps", 0);
        notes.AddPoint(category, NoteKind.Pin, 1, 1, "A");
        notes.AddPoint(category, NoteKind.Pin, 2, 2, "B");

        Assert.Equal(
            "Delete category \"Camps\" and its 2 annotations?",
            WorldMapWording.DeleteCategoryQuestion(notes.FindCategory(category)!));
    }

    [Theory]
    [InlineData(0, "Remove duplicate pins...")]
    [InlineData(1, "Remove 1 duplicate pin...")]
    [InlineData(3, "Remove 3 duplicate pins...")]
    public void DuplicatePinsItem_CountsTheDuplicates(int count, string item)
    {
        Assert.Equal(item, WorldMapWording.DuplicatePinsItem(count));
    }

    [Fact]
    public void DuplicatePinsQuestion_MentionsSubcategories()
    {
        Assert.Equal(
            "Remove 2 duplicate pins from \"Camps\" and its subcategories?",
            WorldMapWording.DuplicatePinsQuestion(2, "Camps", true));
        Assert.Equal(
            "Remove 1 duplicate pin from \"Camps\"?",
            WorldMapWording.DuplicatePinsQuestion(1, "Camps", false));
    }
}
