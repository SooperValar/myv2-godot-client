using TheLastGate.Notes;

namespace TheLastGate.Tests;

public class NoteImportTests
{
    private readonly NoteSet imported = new();
    private readonly NoteSet set = new();

    [Fact]
    public void Conflict_PrefersAnExactNameThenTheFirstIgnoringCase()
    {
        var lower = set.AddCategory("dungeons", 0);
        var upper = set.Attach(NoteCategory.Detached("Dungeons", 0, true, true, []), 0);

        Assert.Equal(upper, NoteImport.Conflict(set, Incoming("Dungeons"), 0));
        Assert.Equal(lower, NoteImport.Conflict(set, Incoming("DUNGEONS"), 0));
        Assert.Equal(0, NoteImport.Conflict(set, Incoming("Keys"), 0));
    }

    [Fact]
    public void Add_WithANewName_TakesTheWholeTreeAndItsSettings()
    {
        set.AddPoint(set.AddCategory("Dungeons", 1), NoteKind.Pin, 1, 1, "Home");
        var incomingRoot = imported.AddCategory("Dungeons", 4);
        imported.FindCategory(incomingRoot)!.ShowLabels = false;
        var incomingCavern = imported.AddCategory("Cold Cavern", 2, incomingRoot);
        imported.AddPoint(incomingCavern, NoteKind.Pin, 3, 4, "Entrance");
        imported.AddCategory("Ice Lair", 3, incomingRoot);

        var result = NoteImport.Add(set, imported, 0, "Friend Dungeons");

        Assert.Equal(NoteImportStatus.Ok, result.Status);
        Assert.Equal(4, set.Categories.Count);
        var root = set.FindCategory(result.RootId)!;
        Assert.Equal(("Friend Dungeons", 0, 4, false), (root.Name, root.ParentId, root.ColourIndex, root.ShowLabels));
        Assert.Equal(["Cold Cavern", "Ice Lair"], Children(result.RootId));
        var cavern = Child(result.RootId, "Cold Cavern")!;
        Assert.Equal(2, cavern.ColourIndex);
        Assert.Equal(3, Assert.Single(cavern.Notes).TileX);
        Assert.NotNull(Child(0, "Dungeons"));
    }

    [Fact]
    public void Add_WithATakenName_ChangesNothing()
    {
        set.AddCategory("Dungeons", 1);
        var incomingRoot = imported.AddCategory("DUNGEONS", 4);
        imported.AddCategory("Cold Cavern", 2, incomingRoot);

        Assert.Equal(NoteImportStatus.NameTaken, NoteImport.Add(set, imported, 0, null).Status);
        Assert.Equal(NoteImportStatus.NameTaken, NoteImport.Add(set, imported, 0, "dungeons").Status);
        Assert.Single(set.Categories);
    }

    [Fact]
    public void Add_GivesDuplicateSiblingsTheirOwnNames()
    {
        var incomingRoot = imported.AddCategory("Dungeons", 0);
        imported.AddCategory("Cold Cavern", 0, incomingRoot);
        imported.Attach(NoteCategory.Detached("Cold Cavern", 0, true, true, []), incomingRoot);

        var result = NoteImport.Add(set, imported, 0, null);

        Assert.Equal(NoteImportStatus.Ok, result.Status);
        Assert.Equal(["Cold Cavern", "Cold Cavern (2)"], Children(result.RootId));
    }

    [Fact]
    public void Merge_GivesDuplicateSiblingsWithNoMatchTheirOwnNames()
    {
        var localRoot = set.AddCategory("Dungeons", 0);
        var incomingRoot = imported.AddCategory("Dungeons", 0);
        imported.AddCategory("Cold Cavern", 0, incomingRoot);
        imported.Attach(NoteCategory.Detached("Cold Cavern", 0, true, true, []), incomingRoot);

        Assert.Equal(NoteImportStatus.Ok, NoteImport.Merge(set, imported, localRoot).Status);

        Assert.Equal(["Cold Cavern", "Cold Cavern (2)"], Children(localRoot));
    }

    [Fact]
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

        Assert.Equal(localRoot, NoteImport.Conflict(set, imported, 0));
        var result = NoteImport.Merge(set, imported, localRoot);

        Assert.Equal(NoteImportStatus.Ok, result.Status);
        Assert.Equal(localRoot, result.RootId);
        Assert.Equal(3, set.Categories.Count);
        Assert.Equal(1, set.FindCategory(localRoot)!.ColourIndex);
        var cavern = set.FindCategory(localCavern)!;
        Assert.Equal(("Cold Cavern", false, 1), (cavern.Name, cavern.Visible, cavern.ColourIndex));
        Assert.Equal([10, 20], cavern.Notes.Select(note => note.TileX));
        Assert.Equal(old, cavern.Notes[0].Id);
        Assert.NotEqual(cavern.Notes[0].Id, cavern.Notes[1].Id);
        var lair = Child(localRoot, "Ice Lair")!;
        Assert.Equal(5, lair.ColourIndex);
        Assert.Single(lair.Notes);
        Assert.True(set.Changed);
    }

    [Fact]
    public void Merge_SkipsPinsWithTheSameLabelAndTile()
    {
        var localRoot = set.AddCategory("Dungeons", 1);
        set.AddPoint(localRoot, NoteKind.Pin, 5, 5, "Gate");
        var incomingRoot = imported.AddCategory("Dungeons", 4);
        imported.AddPoint(incomingRoot, NoteKind.Pin, 5, 5, "Gate");
        imported.SetNoteColour(imported.AddPoint(incomingRoot, NoteKind.Pin, 5, 5, "Gate"), 2);
        imported.AddPoint(incomingRoot, NoteKind.Pin, 5, 5, "gate");
        imported.AddPoint(incomingRoot, NoteKind.Pin, 6, 5, "Gate");

        Assert.Equal(NoteImportStatus.Ok, NoteImport.Merge(set, imported, localRoot).Status);

        var notes = set.FindCategory(localRoot)!.Notes;
        Assert.Equal(["Gate", "gate", "Gate"], notes.Select(note => note.Label));
        Assert.Equal(6, notes[2].TileX);
    }

    [Fact]
    public void Merge_KeepsRecolouredText()
    {
        var localRoot = set.AddCategory("Dungeons", 1);
        set.AddPoint(localRoot, NoteKind.Text, 5, 5, "Gate");
        var incomingRoot = imported.AddCategory("Dungeons", 4);
        imported.AddPoint(incomingRoot, NoteKind.Text, 5, 5, "Gate");
        imported.SetNoteColour(imported.AddPoint(incomingRoot, NoteKind.Text, 5, 5, "Gate"), 2);

        NoteImport.Merge(set, imported, localRoot);

        Assert.Equal(2, set.FindCategory(localRoot)!.Notes.Count);
    }

    [Fact]
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

        Assert.Equal(NoteImportStatus.Ok, NoteImport.Merge(set, imported, localRoot).Status);

        Assert.Equal(4, set.Categories.Count);
        Assert.Single(Children(localRoot));
        Assert.Equal(["Ice", "Lava"], Children(localCaves));
        Assert.Equal(["Gate", "Door"], set.FindCategory(localIce)!.Notes.Select(note => note.Label));
    }

    [Fact]
    public void Add_UnderACategory_NestsTheImport()
    {
        var shared = set.AddCategory("Shared", 0);
        set.AddCategory("Friends", 0, shared);
        set.AddCategory("Bosses", 0);
        var incomingRoot = imported.AddCategory("Bosses", 2);
        imported.AddCategory("Dragon", 2, incomingRoot);

        Assert.Equal(0, NoteImport.Conflict(set, imported, shared));
        var result = NoteImport.Add(set, imported, shared, null);

        Assert.Equal(NoteImportStatus.Ok, result.Status);
        var bosses = set.FindCategory(result.RootId)!;
        Assert.Equal((shared, "Bosses"), (bosses.ParentId, bosses.Name));
        Assert.Equal(["Dragon"], Children(result.RootId));
        Assert.Equal(2, Children(shared).Count);
        Assert.Equal(2, Children(0).Count);
    }

    [Fact]
    public void Add_UnderACategory_ChecksTheNamesThere()
    {
        var shared = set.AddCategory("Shared", 0);
        var existingBosses = set.AddCategory("Bosses", 0, shared);
        var incomingRoot = imported.AddCategory("BOSSES", 2);
        imported.AddCategory("Dragon", 2, incomingRoot);

        Assert.Equal(existingBosses, NoteImport.Conflict(set, imported, shared));
        Assert.Equal(0, NoteImport.Conflict(set, imported, 0));
        Assert.Equal(NoteImportStatus.NameTaken, NoteImport.Add(set, imported, shared, null).Status);
        Assert.Equal(2, set.Categories.Count);
        Assert.Equal(NoteImportStatus.Ok, NoteImport.Add(set, imported, shared, "Their Bosses").Status);
        Assert.Equal(2, Children(shared).Count);
    }

    [Fact]
    public void Add_UnderTheDeepestCategory_ChangesNothing()
    {
        var parent = 0;
        for (var level = 1; level <= NoteLimits.Depth; level++)
        {
            parent = set.AddCategory("Level", 0, parent);
        }

        imported.AddCategory("Bosses", 2);

        Assert.Equal(NoteImportStatus.TooDeep, NoteImport.Add(set, imported, parent, null).Status);
        Assert.Equal(NoteLimits.Depth, set.Categories.Count);
    }

    [Fact]
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

        Assert.Equal(NoteImportStatus.CategoryFull, result.Status);
        Assert.Equal("Cold Cavern", result.CategoryName);
        Assert.Equal(2, set.Categories.Count);
        Assert.Equal(NoteLimits.NotesPerCategory - 1, set.FindCategory(localCavern)!.Notes.Count);
    }

    [Fact]
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

        Assert.Equal(NoteImportStatus.CategoryFull, result.Status);
        Assert.Equal("Cold Cavern", result.CategoryName);
        Assert.Equal(NoteLimits.NotesPerCategory - 1, set.FindCategory(localCavern)!.Notes.Count);
    }

    [Fact]
    public void Merge_DeeperThanEightLevels_ChangesNothing()
    {
        var parent = 0;
        for (var level = 1; level <= 7; level++)
        {
            parent = set.AddCategory("Level", 0, parent);
        }

        var deepRoot = imported.AddCategory("Level", 0);
        imported.AddCategory("Inner", 0, imported.AddCategory("Outer", 0, deepRoot));

        Assert.Equal(NoteImportStatus.TooDeep, NoteImport.Merge(set, imported, parent).Status);
        Assert.Equal(7, set.Categories.Count);

        var shallow = new NoteSet();
        shallow.AddCategory("Outer", 0, shallow.AddCategory("Level", 0));
        Assert.Equal(NoteImportStatus.Ok, NoteImport.Merge(set, shallow, parent).Status);
        Assert.Equal(8, set.Categories.Count);
    }

    [Fact]
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

        Assert.Equal(NoteImportStatus.TooManyCategories, NoteImport.Merge(set, imported, localRoot).Status);
        Assert.Equal(NoteLimits.Categories - 1, set.Categories.Count);
    }

    [Theory]
    [InlineData(NoteImportStatus.NameTaken, "", "That name is already taken.")]
    [InlineData(NoteImportStatus.TooManyCategories, "", "Import would go over the 256 category limit.")]
    [InlineData(NoteImportStatus.TooDeep, "", "Import would nest categories more than 8 deep.")]
    [InlineData(NoteImportStatus.CategoryFull, "Cold Cavern", "Import would overfill \"Cold Cavern\".")]
    [InlineData(NoteImportStatus.Failed, "", "Import failed.")]
    public void Problem_UsesTheCurrentClientsWords(NoteImportStatus status, string categoryName, string message)
    {
        Assert.Equal(message, NoteImport.Problem(new NoteImportResult(status, 0, categoryName)));
    }

    [Fact]
    public void ConflictMessages_NameTheCategories()
    {
        Assert.Equal(
            "You already have a category called \"Bosses\".",
            NoteImport.ConflictMessage(null, "Bosses"));
        Assert.Equal(
            "\"Shared\" already has a category called \"Bosses\".",
            NoteImport.ConflictMessage("Shared", "Bosses"));
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
