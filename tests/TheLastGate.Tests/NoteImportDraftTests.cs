using TheLastGate.Notes;
using TheLastGate.Windows;

namespace TheLastGate.Tests;

public class NoteImportDraftTests
{
    private readonly NoteSet notes = new();

    [Fact]
    public void AClipboardCode_IsPastedInReadyToAdd()
    {
        var code = CampsCode();

        var draft = new NoteImportDraft(notes, 0, "  \r\n" + code);

        Assert.Equal(code, draft.Text);
        Assert.Equal("Camps: 1 category, 1 pins, 0 labels, 0 paths", draft.Preview);
        Assert.True(draft.IsReady);
        Assert.False(draft.HasConflict);
    }

    [Fact]
    public void AClipboardWithNoCode_LeavesTheBoxEmpty()
    {
        var draft = new NoteImportDraft(notes, 0, "hello");

        Assert.Equal("", draft.Text);
        Assert.Equal("", draft.Preview);
        Assert.False(draft.IsReady);
    }

    [Fact]
    public void ChangeText_SaysWhatIsWrongWithTheCode()
    {
        var draft = new NoteImportDraft(notes, 0, "");

        draft.ChangeText("hello");

        Assert.Equal("That is not a world map code.", draft.Preview);
        Assert.False(draft.IsReady);
    }

    [Fact]
    public void Add_PutsTheCategoryInTheNotes()
    {
        var draft = new NoteImportDraft(notes, 0, CampsCode());

        var result = draft.Add();

        Assert.Equal(NoteImportStatus.Ok, result?.Status);
        var camps = notes.FindCategory(result!.RootId)!;
        Assert.Equal("Camps", camps.Name);
        Assert.Single(camps.Notes);
    }

    [Fact]
    public void ACategoryOfTheSameName_IsAConflict()
    {
        notes.AddCategory("camps", 0);

        var draft = new NoteImportDraft(notes, 0, CampsCode());

        Assert.True(draft.HasConflict);
        Assert.Equal("You already have a category called \"Camps\".", draft.ConflictMessage);
        Assert.Equal("Camps", draft.RootName);
        Assert.False(draft.CanAddAsNew);
    }

    [Fact]
    public void AddAsNew_UsesTheNewName()
    {
        notes.AddCategory("Camps", 0);
        var draft = new NoteImportDraft(notes, 0, CampsCode()) { RootName = "Camps 2" };

        Assert.True(draft.CanAddAsNew);
        var result = draft.AddAsNew();

        Assert.Equal("Camps 2", notes.FindCategory(result!.RootId)!.Name);
    }

    [Fact]
    public void Merge_AddsTheNotesToTheCategoryAlreadyThere()
    {
        var camps = notes.AddCategory("Camps", 0);
        var draft = new NoteImportDraft(notes, 0, CampsCode());

        var result = draft.Merge();

        Assert.Equal(camps, result!.RootId);
        Assert.Single(notes.FindCategory(camps)!.Notes);
    }

    [Fact]
    public void ImportingIntoACategory_NamesIt()
    {
        var shared = notes.AddCategory("Shared", 0);
        notes.AddCategory("Camps", 0, shared);

        var draft = new NoteImportDraft(notes, shared, CampsCode());

        Assert.Equal("Shared", draft.DestinationName);
        Assert.Equal("\"Shared\" already has a category called \"Camps\".", draft.ConflictMessage);
    }

    [Fact]
    public void DestinationGone_WhenTheCategoryToImportIntoIsDeleted()
    {
        var shared = notes.AddCategory("Shared", 0);
        var draft = new NoteImportDraft(notes, shared, CampsCode());

        notes.RemoveCategory(shared);

        Assert.True(draft.DestinationGone);
    }

    [Fact]
    public void AnImportThatFails_SaysWhy()
    {
        var parent = 0;
        for (var depth = 0; depth < NoteLimits.Depth; depth++)
        {
            parent = notes.AddCategory($"Level {depth}", 0, parent);
        }

        var draft = new NoteImportDraft(notes, parent, CampsCode());

        var result = draft.Add();

        Assert.Equal(NoteImportStatus.TooDeep, result?.Status);
        Assert.Equal(NoteImport.Problem(result!), draft.Error);
    }

    [Fact]
    public void Add_IsNotPossibleWithoutACode()
    {
        var draft = new NoteImportDraft(notes, 0, "");

        Assert.Null(draft.Add());
    }

    private static string CampsCode()
    {
        var source = new NoteSet();
        var camps = source.AddCategory("Camps", 2);
        source.AddPoint(camps, NoteKind.Pin, 10, 10, "Camp");
        return NoteShareCode.Export(source, camps)!;
    }
}
