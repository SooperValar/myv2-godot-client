using GdUnit4;
using TheLastGate.Notes;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Notes;

[TestSuite]
public class NoteImportTests
{
    private NoteSet imported = new();
    private NoteSet set = new();

    [BeforeTest]
    public void StartWithNoNotes()
    {
        imported = new NoteSet();
        set = new NoteSet();
    }

    [TestCase]
    public void Conflict_PrefersAnExactNameThenTheFirstIgnoringCase()
    {
        var lower = set.AddCategory("dungeons", 0);
        var upper = set.Attach(NoteCategory.Detached("Dungeons", 0, true, true, []), 0);

        AssertThat(NoteImport.Conflict(set, Incoming("Dungeons"), 0)).IsEqual(upper);
        AssertThat(NoteImport.Conflict(set, Incoming("DUNGEONS"), 0)).IsEqual(lower);
        AssertThat(NoteImport.Conflict(set, Incoming("Keys"), 0)).IsEqual(0);
    }

    [TestCase]
    public void Add_WithANewName_TakesTheWholeTreeAndItsSettings()
    {
        set.AddPoint(set.AddCategory("Dungeons", 1), NoteKind.Pin, 1, 1, "Home");
        var incomingRoot = imported.AddCategory("Dungeons", 4);
        imported.FindCategory(incomingRoot)!.ShowLabels = false;
        var incomingCavern = imported.AddCategory("Cold Cavern", 2, incomingRoot);
        imported.AddPoint(incomingCavern, NoteKind.Pin, 3, 4, "Entrance");
        imported.AddCategory("Ice Lair", 3, incomingRoot);

        var result = NoteImport.Add(set, imported, 0, "Friend Dungeons");

        AssertThat(result.Status).IsEqual(NoteImportStatus.Ok);
        AssertThat(set.Categories.Count).IsEqual(4);
        var root = set.FindCategory(result.RootId)!;
        AssertThat((root.Name, root.ParentId, root.ColourIndex, root.ShowLabels))
            .IsEqual(("Friend Dungeons", 0, 4, false));
        AssertThat(Children(result.RootId)).ContainsExactly("Cold Cavern", "Ice Lair");
        var cavern = Child(result.RootId, "Cold Cavern")!;
        AssertThat(cavern.ColourIndex).IsEqual(2);
        AssertThat(cavern.Notes).HasSize(1);
        AssertThat(cavern.Notes[0].TileX).IsEqual(3);
        AssertThat(Child(0, "Dungeons")).IsNotNull();
    }

    [TestCase]
    public void Add_WithATakenName_ChangesNothing()
    {
        set.AddCategory("Dungeons", 1);
        var incomingRoot = imported.AddCategory("DUNGEONS", 4);
        imported.AddCategory("Cold Cavern", 2, incomingRoot);

        AssertThat(NoteImport.Add(set, imported, 0, null).Status).IsEqual(NoteImportStatus.NameTaken);
        AssertThat(NoteImport.Add(set, imported, 0, "dungeons").Status).IsEqual(NoteImportStatus.NameTaken);
        AssertThat(set.Categories).HasSize(1);
    }

    [TestCase]
    public void Add_GivesDuplicateSiblingsTheirOwnNames()
    {
        var incomingRoot = imported.AddCategory("Dungeons", 0);
        imported.AddCategory("Cold Cavern", 0, incomingRoot);
        imported.Attach(NoteCategory.Detached("Cold Cavern", 0, true, true, []), incomingRoot);

        var result = NoteImport.Add(set, imported, 0, null);

        AssertThat(result.Status).IsEqual(NoteImportStatus.Ok);
        AssertThat(Children(result.RootId)).ContainsExactly("Cold Cavern", "Cold Cavern (2)");
    }

    [TestCase]
    public void Merge_GivesDuplicateSiblingsWithNoMatchTheirOwnNames()
    {
        var localRoot = set.AddCategory("Dungeons", 0);
        var incomingRoot = imported.AddCategory("Dungeons", 0);
        imported.AddCategory("Cold Cavern", 0, incomingRoot);
        imported.Attach(NoteCategory.Detached("Cold Cavern", 0, true, true, []), incomingRoot);

        AssertThat(NoteImport.Merge(set, imported, localRoot).Status).IsEqual(NoteImportStatus.Ok);

        AssertThat(Children(localRoot)).ContainsExactly("Cold Cavern", "Cold Cavern (2)");
    }

    [TestCase]
    public void Merge_MatchesByNameAndKeepsTheLocalSettings()
    {
        var localRoot = set.AddCategory("Dungeons", 1);
        var localCavern = set.AddCategory("Cold Cavern", 1, localRoot);
        set.FindCategory(localCavern)!.Visible = false;
        var old = set.AddPoint(localCavern, NoteKind.Pin, 10, 10, "Old");
        var incomingRoot = imported.AddCategory("Dungeons", 4);
        var incomingCavern = imported.AddCategory("cold cavern", 4, incomingRoot);
        imported.AddPoint(incomingCavern, NoteKind.Pin, 20, 20, "New");
        var incomingLair = imported.AddCategory("Ice Lair", 5, incomingRoot);
        imported.AddPoint(incomingLair, NoteKind.Pin, 30, 30, "Lair");

        AssertThat(NoteImport.Conflict(set, imported, 0)).IsEqual(localRoot);
        var result = NoteImport.Merge(set, imported, localRoot);

        AssertThat(result.Status).IsEqual(NoteImportStatus.Ok);
        AssertThat(result.RootId).IsEqual(localRoot);
        AssertThat(set.Categories.Count).IsEqual(3);
        AssertThat(set.FindCategory(localRoot)!.ColourIndex).IsEqual(1);
        var cavern = set.FindCategory(localCavern)!;
        AssertThat((cavern.Name, cavern.Visible, cavern.ColourIndex)).IsEqual(("Cold Cavern", false, 1));
        AssertThat(cavern.Notes.Select(note => note.TileX)).ContainsExactly(10, 20);
        AssertThat(cavern.Notes[0].Id).IsEqual(old);
        AssertThat(cavern.Notes[1].Id).IsNotEqual(cavern.Notes[0].Id);
        var lair = Child(localRoot, "Ice Lair")!;
        AssertThat(lair.ColourIndex).IsEqual(5);
        AssertThat(lair.Notes).HasSize(1);
        AssertThat(set.Changed).IsTrue();
    }

    [TestCase]
    public void Merge_SkipsPinsWithTheSameLabelAndTile()
    {
        var localRoot = set.AddCategory("Dungeons", 1);
        set.AddPoint(localRoot, NoteKind.Pin, 5, 5, "Gate");
        var incomingRoot = imported.AddCategory("Dungeons", 4);
        imported.AddPoint(incomingRoot, NoteKind.Pin, 5, 5, "Gate");
        imported.SetNoteColour(imported.AddPoint(incomingRoot, NoteKind.Pin, 5, 5, "Gate"), 2);
        imported.AddPoint(incomingRoot, NoteKind.Pin, 5, 5, "gate");
        imported.AddPoint(incomingRoot, NoteKind.Pin, 6, 5, "Gate");

        AssertThat(NoteImport.Merge(set, imported, localRoot).Status).IsEqual(NoteImportStatus.Ok);

        var notes = set.FindCategory(localRoot)!.Notes;
        AssertThat(notes.Select(note => note.Label)).ContainsExactly("Gate", "gate", "Gate");
        AssertThat(notes[2].TileX).IsEqual(6);
    }

    [TestCase]
    public void Merge_KeepsRecolouredText()
    {
        var localRoot = set.AddCategory("Dungeons", 1);
        set.AddPoint(localRoot, NoteKind.Text, 5, 5, "Gate");
        var incomingRoot = imported.AddCategory("Dungeons", 4);
        imported.AddPoint(incomingRoot, NoteKind.Text, 5, 5, "Gate");
        imported.SetNoteColour(imported.AddPoint(incomingRoot, NoteKind.Text, 5, 5, "Gate"), 2);

        NoteImport.Merge(set, imported, localRoot);

        AssertThat(set.FindCategory(localRoot)!.Notes.Count).IsEqual(2);
    }

    [TestCase]
    public void Merge_ReachesEveryLevel()
    {
        var localRoot = set.AddCategory("Dungeons", 1);
        var localCaves = set.AddCategory("Caves", 1, localRoot);
        var localIce = set.AddCategory("Ice", 1, localCaves);
        set.AddPoint(localIce, NoteKind.Pin, 5, 5, "Gate");
        var incomingRoot = imported.AddCategory("Dungeons", 4);
        var incomingCaves = imported.AddCategory("caves", 4, incomingRoot);
        var incomingIce = imported.AddCategory("Ice", 4, incomingCaves);
        imported.SetNoteColour(imported.AddPoint(incomingIce, NoteKind.Pin, 5, 5, "Gate"), 3);
        imported.AddPoint(incomingIce, NoteKind.Pin, 6, 6, "Door");
        imported.AddCategory("Lava", 4, incomingCaves);

        AssertThat(NoteImport.Merge(set, imported, localRoot).Status).IsEqual(NoteImportStatus.Ok);

        AssertThat(set.Categories.Count).IsEqual(4);
        AssertThat(Children(localRoot)).HasSize(1);
        AssertThat(Children(localCaves)).ContainsExactly("Ice", "Lava");
        AssertThat(set.FindCategory(localIce)!.Notes.Select(note => note.Label)).ContainsExactly("Gate", "Door");
    }

    [TestCase]
    public void Add_UnderACategory_NestsTheImport()
    {
        var shared = set.AddCategory("Shared", 0);
        set.AddCategory("Friends", 0, shared);
        set.AddCategory("Bosses", 0);
        var incomingRoot = imported.AddCategory("Bosses", 2);
        imported.AddCategory("Dragon", 2, incomingRoot);

        AssertThat(NoteImport.Conflict(set, imported, shared)).IsEqual(0);
        var result = NoteImport.Add(set, imported, shared, null);

        AssertThat(result.Status).IsEqual(NoteImportStatus.Ok);
        var bosses = set.FindCategory(result.RootId)!;
        AssertThat((bosses.ParentId, bosses.Name)).IsEqual((shared, "Bosses"));
        AssertThat(Children(result.RootId)).ContainsExactly("Dragon");
        AssertThat(Children(shared).Count).IsEqual(2);
        AssertThat(Children(0).Count).IsEqual(2);
    }

    [TestCase]
    public void Add_UnderACategory_ChecksTheNamesThere()
    {
        var shared = set.AddCategory("Shared", 0);
        var existingBosses = set.AddCategory("Bosses", 0, shared);
        var incomingRoot = imported.AddCategory("BOSSES", 2);
        imported.AddCategory("Dragon", 2, incomingRoot);

        AssertThat(NoteImport.Conflict(set, imported, shared)).IsEqual(existingBosses);
        AssertThat(NoteImport.Conflict(set, imported, 0)).IsEqual(0);
        AssertThat(NoteImport.Add(set, imported, shared, null).Status).IsEqual(NoteImportStatus.NameTaken);
        AssertThat(set.Categories.Count).IsEqual(2);
        AssertThat(NoteImport.Add(set, imported, shared, "Their Bosses").Status).IsEqual(NoteImportStatus.Ok);
        AssertThat(Children(shared).Count).IsEqual(2);
    }

    [TestCase]
    public void Add_UnderTheDeepestCategory_ChangesNothing()
    {
        var parent = 0;
        for (var level = 1; level <= NoteLimits.Depth; level++)
        {
            parent = set.AddCategory("Level", 0, parent);
        }

        imported.AddCategory("Bosses", 2);

        AssertThat(NoteImport.Add(set, imported, parent, null).Status).IsEqual(NoteImportStatus.TooDeep);
        AssertThat(set.Categories.Count).IsEqual(NoteLimits.Depth);
    }

    [TestCase]
    public void Merge_OverTheNoteLimit_ChangesNothing()
    {
        var localRoot = set.AddCategory("Dungeons", 1);
        var localCavern = set.AddCategory("Cold Cavern", 1, localRoot);
        for (var pin = 0; pin < NoteLimits.NotesPerCategory - 1; pin++)
        {
            set.AddPoint(localCavern, NoteKind.Pin, pin, 0, "Pin");
        }

        var incomingRoot = imported.AddCategory("Dungeons", 4);
        var incomingCavern = imported.AddCategory("cold cavern", 4, incomingRoot);
        imported.AddPoint(incomingCavern, NoteKind.Pin, 1, 100, "New");
        imported.AddPoint(incomingCavern, NoteKind.Pin, 2, 100, "New");
        imported.AddCategory("Ice Lair", 5, incomingRoot);

        var result = NoteImport.Merge(set, imported, localRoot);

        AssertThat(result.Status).IsEqual(NoteImportStatus.CategoryFull);
        AssertThat(result.CategoryName).IsEqual("Cold Cavern");
        AssertThat(set.Categories.Count).IsEqual(2);
        AssertThat(set.FindCategory(localCavern)!.Notes.Count).IsEqual(NoteLimits.NotesPerCategory - 1);
    }

    [TestCase]
    public void Merge_CountsEveryMatchAgainstTheLimit()
    {
        var localRoot = set.AddCategory("Dungeons", 1);
        var localCavern = set.AddCategory("Cold Cavern", 1, localRoot);
        for (var pin = 0; pin < NoteLimits.NotesPerCategory - 1; pin++)
        {
            set.AddPoint(localCavern, NoteKind.Pin, pin, 0, "Pin");
        }

        var incomingRoot = imported.AddCategory("Dungeons", 4);
        var firstCavern = imported.AddCategory("Cold Cavern", 4, incomingRoot);
        var secondCavern = imported.Attach(NoteCategory.Detached("cold cavern", 4, true, true, []), incomingRoot);
        imported.AddPoint(firstCavern, NoteKind.Pin, 1, 100, "New");
        imported.AddPoint(secondCavern, NoteKind.Pin, 2, 100, "New");

        var result = NoteImport.Merge(set, imported, localRoot);

        AssertThat(result.Status).IsEqual(NoteImportStatus.CategoryFull);
        AssertThat(result.CategoryName).IsEqual("Cold Cavern");
        AssertThat(set.FindCategory(localCavern)!.Notes.Count).IsEqual(NoteLimits.NotesPerCategory - 1);
    }

    [TestCase]
    public void Merge_DeeperThanEightLevels_ChangesNothing()
    {
        var parent = 0;
        for (var level = 1; level <= 7; level++)
        {
            parent = set.AddCategory("Level", 0, parent);
        }

        var deepRoot = imported.AddCategory("Level", 0);
        imported.AddCategory("Inner", 0, imported.AddCategory("Outer", 0, deepRoot));

        AssertThat(NoteImport.Merge(set, imported, parent).Status).IsEqual(NoteImportStatus.TooDeep);
        AssertThat(set.Categories.Count).IsEqual(7);

        var shallow = new NoteSet();
        shallow.AddCategory("Outer", 0, shallow.AddCategory("Level", 0));
        AssertThat(NoteImport.Merge(set, shallow, parent).Status).IsEqual(NoteImportStatus.Ok);
        AssertThat(set.Categories.Count).IsEqual(8);
    }

    [TestCase]
    public void Merge_OverTheCategoryLimit_ChangesNothing()
    {
        var localRoot = set.AddCategory("Dungeons", 0);
        for (var category = 1; category < NoteLimits.Categories - 1; category++)
        {
            set.AddCategory($"Category {category}", 0);
        }

        var incomingRoot = imported.AddCategory("Dungeons", 0);
        imported.AddCategory("New A", 0, incomingRoot);
        imported.AddCategory("New B", 0, incomingRoot);

        AssertThat(NoteImport.Merge(set, imported, localRoot).Status).IsEqual(NoteImportStatus.TooManyCategories);
        AssertThat(set.Categories.Count).IsEqual(NoteLimits.Categories - 1);
    }

    [TestCase(NoteImportStatus.NameTaken, "", "That name is already taken.")]
    [TestCase(NoteImportStatus.TooManyCategories, "", "Import would go over the 256 category limit.")]
    [TestCase(NoteImportStatus.TooDeep, "", "Import would nest categories more than 8 deep.")]
    [TestCase(NoteImportStatus.CategoryFull, "Cold Cavern", "Import would overfill \"Cold Cavern\".")]
    [TestCase(NoteImportStatus.Failed, "", "Import failed.")]
    public void Problem_UsesTheCurrentClientsWords(NoteImportStatus status, string categoryName, string message)
    {
        AssertThat(NoteImport.Problem(new NoteImportResult(status, 0, categoryName))).IsEqual(message);
    }

    [TestCase]
    public void ConflictMessages_NameTheCategories()
    {
        AssertThat(NoteImport.ConflictMessage(null, "Bosses"))
            .IsEqual("You already have a category called \"Bosses\".");
        AssertThat(NoteImport.ConflictMessage("Shared", "Bosses"))
            .IsEqual("\"Shared\" already has a category called \"Bosses\".");
    }

    private static NoteSet Incoming(string name)
    {
        var incoming = new NoteSet();
        incoming.AddCategory(name, 0);
        return incoming;
    }

    private List<string> Children(int parentId) =>
        set.Categories.Where(category => category.ParentId == parentId).Select(category => category.Name).ToList();

    private NoteCategory? Child(int parentId, string name) =>
        set.Categories.FirstOrDefault(category => category.ParentId == parentId && category.Name == name);
}
