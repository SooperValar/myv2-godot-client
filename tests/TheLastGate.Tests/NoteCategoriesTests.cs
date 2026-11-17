using TheLastGate.Notes;

namespace TheLastGate.Tests;

public class NoteCategoriesTests
{
    private readonly NoteSet set = new();

    [Fact]
    public void Ids_StartAtOneAndNeverRepeat()
    {
        var first = set.AddCategory("A", 0);
        var second = set.AddCategory("B", 0);

        Assert.Equal(1, first);
        Assert.Equal(2, second);
    }

    [Fact]
    public void ANewCategory_IsShownWithItsLabelsAndMarksTheSetChanged()
    {
        var keys = set.FindCategory(set.AddCategory("Keys", 3))!;

        Assert.True(keys.Visible);
        Assert.True(keys.ShowLabels);
        Assert.Equal(3, keys.ColourIndex);
        Assert.Equal(0, keys.ParentId);
        Assert.True(set.Changed);
    }

    [Fact]
    public void AnInvalidColour_FallsBackToTheFirst()
    {
        Assert.Equal(0, set.FindCategory(set.AddCategory("Keys", 99))!.ColourIndex);
    }

    [Fact]
    public void SiblingNames_AreUniqueIgnoringCaseButCousinsMayShare()
    {
        var aston = set.AddCategory("Aston", 0);
        var neiseer = set.AddCategory("Neiseer", 0);

        var first = set.AddCategory("Quest Givers", 0, aston);
        var second = set.AddCategory("Quest Givers", 0, neiseer);
        var third = set.AddCategory("quest givers", 0, aston);

        Assert.Equal("Quest Givers", set.FindCategory(first)!.Name);
        Assert.Equal("Quest Givers", set.FindCategory(second)!.Name);
        Assert.Equal("quest givers (2)", set.FindCategory(third)!.Name);
    }

    [Fact]
    public void ALongName_IsCutToFitAndKeepsRoomForItsNumber()
    {
        var wanted = new string('x', 40);

        var first = set.AddCategory(wanted, 0);
        var second = set.AddCategory(wanted, 0);

        Assert.Equal(new string('x', 31), set.FindCategory(first)!.Name);
        Assert.Equal(new string('x', 27) + " (2)", set.FindCategory(second)!.Name);
    }

    [Fact]
    public void Depth_StopsAtEightLevels()
    {
        var parent = 0;
        for (var level = 1; level <= NoteLimits.Depth; level++)
        {
            parent = set.AddCategory("Level", 0, parent);
            Assert.NotEqual(0, parent);
        }

        Assert.Equal(0, set.AddCategory("Too deep", 0, parent));
        Assert.Equal(NoteLimits.Depth, set.Depth(parent));
        Assert.Equal(0, set.Depth(999));
    }

    [Fact]
    public void AddCategory_RefusesAnUnknownParent()
    {
        Assert.Equal(0, set.AddCategory("Orphan", 0, 999));
    }

    [Fact]
    public void AddCategory_StopsAtTheCategoryLimit()
    {
        for (var category = 0; category < NoteLimits.Categories; category++)
        {
            set.AddCategory($"C{category}", 0);
        }

        Assert.Equal(0, set.AddCategory("One too many", 0));
    }

    [Fact]
    public void RemoveCategory_RefusesACategoryWithChildren()
    {
        var parent = set.AddCategory("Parent", 0);
        var child = set.AddCategory("Child", 0, parent);

        Assert.True(set.HasChildren(parent));
        Assert.False(set.RemoveCategory(parent));
        Assert.True(set.RemoveCategory(child));
        Assert.False(set.HasChildren(parent));
        Assert.True(set.RemoveCategory(parent));
        Assert.Empty(set.Categories);
    }

    [Fact]
    public void AHiddenAncestor_HidesItsDescendants()
    {
        var parent = set.FindCategory(set.AddCategory("Parent", 0))!;
        var child = set.FindCategory(set.AddCategory("Child", 0, parent.Id))!;

        parent.Visible = false;
        Assert.False(set.IsShown(child));
        parent.Visible = true;
        Assert.True(set.IsShown(child));

        parent.ShowLabels = false;
        Assert.False(set.LabelsShown(child));
        parent.ShowLabels = true;
        Assert.True(set.LabelsShown(child));
    }

    [Fact]
    public void Tree_ListsCategoriesParentFirst()
    {
        var first = set.AddCategory("A", 0);
        var second = set.AddCategory("B", 0);
        set.AddCategory("A1", 0, first);

        Assert.Equal(["A", "A1", "B"], set.Tree().Select(category => category.Name));
        Assert.Equal(["A", "A1"], set.Tree(first).Select(category => category.Name));
        Assert.Equal(["B"], set.Tree(second).Select(category => category.Name));
        Assert.Empty(set.Tree(999));
    }

    [Fact]
    public void IsWithinAndSubtreeHeight_FollowTheTree()
    {
        var first = set.AddCategory("A", 0);
        var second = set.AddCategory("B", 0);
        var child = set.AddCategory("A1", 0, first);

        Assert.True(set.IsWithin(child, first));
        Assert.False(set.IsWithin(first, child));
        Assert.True(set.IsWithin(first, first));
        Assert.Equal(2, set.SubtreeHeight(first));
        Assert.Equal(1, set.SubtreeHeight(second));
    }

    [Fact]
    public void RenameCategory_AddsANumberOnlyWhenASiblingHasTheName()
    {
        var first = set.AddCategory("Aston", 0);
        var second = set.AddCategory("Bank", 0);

        Assert.True(set.RenameCategory(second, "aston"));
        Assert.Equal("aston (2)", set.FindCategory(second)!.Name);
        Assert.True(set.RenameCategory(first, "ASTON"));
        Assert.Equal("ASTON", set.FindCategory(first)!.Name);
        Assert.False(set.RenameCategory(first, ""));
        Assert.False(set.RenameCategory(999, "Name"));
    }

    [Fact]
    public void SetCategoryColour_RefusesAColourOffThePalette()
    {
        var category = set.AddCategory("A", 0);

        Assert.True(set.SetCategoryColour(category, 5));
        Assert.False(set.SetCategoryColour(category, NotePalette.Size));
        Assert.Equal(5, set.FindCategory(category)!.ColourIndex);
    }

    [Fact]
    public void MoveCategory_RefusesLoopsAndStayingPut()
    {
        var first = set.AddCategory("A", 0);
        var child = set.AddCategory("A1", 0, first);

        Assert.False(set.MoveCategory(first, child));
        Assert.False(set.MoveCategory(first, first));
        Assert.False(set.MoveCategory(child, first));
        Assert.True(set.CanMoveCategory(child, 0));
        Assert.True(set.MoveCategory(child, 0));
        Assert.Equal(0, set.FindCategory(child)!.ParentId);
    }

    [Fact]
    public void MoveCategory_TakesItsSubtreeAndANumberIfTheNameClashes()
    {
        var first = set.AddCategory("A", 0);
        var other = set.AddCategory("B", 0);
        var givers = set.AddCategory("Quest Givers", 0, other);
        var nested = set.AddCategory("quest givers", 0, first);
        set.AddCategory("Leaf", 0, nested);

        Assert.True(set.MoveCategory(nested, other));

        Assert.Equal(other, set.FindCategory(nested)!.ParentId);
        Assert.Equal("quest givers (2)", set.FindCategory(nested)!.Name);
        Assert.Equal(2, set.SubtreeHeight(nested));
        Assert.Equal("Quest Givers", set.FindCategory(givers)!.Name);
    }

    [Fact]
    public void MoveCategory_RefusesGoingDeeperThanEightLevels()
    {
        var chain = 0;
        for (var level = 1; level < NoteLimits.Depth; level++)
        {
            chain = set.AddCategory("Chain", 0, chain);
        }

        var pair = set.AddCategory("Pair", 0);
        set.AddCategory("Pair child", 0, pair);

        Assert.False(set.CanMoveCategory(pair, chain));
        Assert.False(set.MoveCategory(pair, chain));
    }

    [Fact]
    public void ShiftCategory_SwapsWithTheNeighbouringSibling()
    {
        var aston = set.AddCategory("Aston", 0);
        var questGivers = set.AddCategory("Quest Givers", 1, aston);
        set.AddCategory("Neiseer", 2);
        var treasure = set.AddCategory("Treasure", 3, aston);
        var dungeons = set.AddCategory("Dungeons", 4);
        set.Changed = false;

        Assert.False(set.CanShiftCategory(aston, -1));
        Assert.False(set.CanShiftCategory(dungeons, 1));
        Assert.False(set.CanShiftCategory(questGivers, -1));
        Assert.False(set.CanShiftCategory(treasure, 1));
        Assert.False(set.CanShiftCategory(999, 1));
        Assert.False(set.ShiftCategory(aston, -1));
        Assert.False(set.Changed);

        Assert.True(set.ShiftCategory(dungeons, -1));
        Assert.True(set.Changed);
        Assert.Equal(
            ["Aston", "Quest Givers", "Treasure", "Dungeons", "Neiseer"],
            set.Tree().Select(category => category.Name));

        Assert.True(set.ShiftCategory(treasure, -1));
        Assert.Equal(["Aston", "Treasure", "Quest Givers"], set.Tree(aston).Select(category => category.Name));
        Assert.Equal(aston, set.FindCategory(treasure)!.ParentId);
    }
}
