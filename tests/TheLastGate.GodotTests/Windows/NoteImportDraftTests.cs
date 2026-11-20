using GdUnit4;
using TheLastGate.Notes;
using TheLastGate.Windows;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Windows;

[TestSuite]
public class NoteImportDraftTests
{
    private NoteSet notes = new();

    [BeforeTest]
    public void StartWithNoNotes() => notes = new NoteSet();

    [TestCase]
    public void AClipboardCode_IsPastedInReadyToAdd()
    {
        var code = CampsCode();

        var draft = new NoteImportDraft(notes, 0, "  \r\n" + code);

        AssertThat(draft.Text).IsEqual(code);
        AssertThat(draft.Preview).IsEqual("Camps: 1 category, 1 pins, 0 labels, 0 paths");
        AssertThat(draft.IsReady).IsTrue();
        AssertThat(draft.HasConflict).IsFalse();
    }

    [TestCase]
    public void AClipboardWithNoCode_LeavesTheBoxEmpty()
    {
        var draft = new NoteImportDraft(notes, 0, "hello");

        AssertThat(draft.Text).IsEqual("");
        AssertThat(draft.Preview).IsEqual("");
        AssertThat(draft.IsReady).IsFalse();
    }

    [TestCase]
    public void ChangeText_SaysWhatIsWrongWithTheCode()
    {
        var draft = new NoteImportDraft(notes, 0, "");

        draft.ChangeText("hello");

        AssertThat(draft.Preview).IsEqual("That is not a world map code.");
        AssertThat(draft.IsReady).IsFalse();
    }

    [TestCase]
    public void Add_PutsTheCategoryInTheNotes()
    {
        var draft = new NoteImportDraft(notes, 0, CampsCode());

        var result = draft.Add();

        AssertThat(result?.Status).IsEqual(NoteImportStatus.Ok);
        var camps = notes.FindCategory(result!.RootId)!;
        AssertThat(camps.Name).IsEqual("Camps");
        AssertThat(camps.Notes).HasSize(1);
    }

    [TestCase]
    public void ACategoryOfTheSameName_IsAConflict()
    {
        notes.AddCategory("camps", 0);

        var draft = new NoteImportDraft(notes, 0, CampsCode());

        AssertThat(draft.HasConflict).IsTrue();
        AssertThat(draft.ConflictMessage).IsEqual("You already have a category called \"Camps\".");
        AssertThat(draft.RootName).IsEqual("Camps");
        AssertThat(draft.CanAddAsNew).IsFalse();
    }

    [TestCase]
    public void AddAsNew_UsesTheNewName()
    {
        notes.AddCategory("Camps", 0);
        var draft = new NoteImportDraft(notes, 0, CampsCode()) { RootName = "Camps 2" };

        AssertThat(draft.CanAddAsNew).IsTrue();
        var result = draft.AddAsNew();

        AssertThat(notes.FindCategory(result!.RootId)!.Name).IsEqual("Camps 2");
    }

    [TestCase]
    public void Merge_AddsTheNotesToTheCategoryAlreadyThere()
    {
        var camps = notes.AddCategory("Camps", 0);
        var draft = new NoteImportDraft(notes, 0, CampsCode());

        var result = draft.Merge();

        AssertThat(result!.RootId).IsEqual(camps);
        AssertThat(notes.FindCategory(camps)!.Notes).HasSize(1);
    }

    [TestCase]
    public void ImportingIntoACategory_NamesIt()
    {
        var shared = notes.AddCategory("Shared", 0);
        notes.AddCategory("Camps", 0, shared);

        var draft = new NoteImportDraft(notes, shared, CampsCode());

        AssertThat(draft.DestinationName).IsEqual("Shared");
        AssertThat(draft.ConflictMessage).IsEqual("\"Shared\" already has a category called \"Camps\".");
    }

    [TestCase]
    public void DestinationGone_WhenTheCategoryToImportIntoIsDeleted()
    {
        var shared = notes.AddCategory("Shared", 0);
        var draft = new NoteImportDraft(notes, shared, CampsCode());

        notes.RemoveCategory(shared);

        AssertThat(draft.DestinationGone).IsTrue();
    }

    [TestCase]
    public void AnImportThatFails_SaysWhy()
    {
        var parent = 0;
        for (var depth = 0; depth < NoteLimits.Depth; depth++)
        {
            parent = notes.AddCategory($"Level {depth}", 0, parent);
        }

        var draft = new NoteImportDraft(notes, parent, CampsCode());

        var result = draft.Add();

        AssertThat(result?.Status).IsEqual(NoteImportStatus.TooDeep);
        AssertThat(draft.Error).IsEqual(NoteImport.Problem(result!));
    }

    [TestCase]
    public void Add_IsNotPossibleWithoutACode()
    {
        var draft = new NoteImportDraft(notes, 0, "");

        AssertThat(draft.Add()).IsNull();
    }

    private static string CampsCode()
    {
        var source = new NoteSet();
        var camps = source.AddCategory("Camps", 2);
        source.AddPoint(camps, NoteKind.Pin, 10, 10, "Camp");
        return NoteShareCode.Export(source, camps)!;
    }
}
