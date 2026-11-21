using TheLastGate.Assets;
using TheLastGate.State;
using TheLastGate.Stats;
using TheLastGate.World;

namespace TheLastGate.Tests;

public class SkillShortcutTests
{
    private const int Dagger = 3;
    private const int Sword = 4;

    private const string ShortcutKeysHint =
        " Right-click on one of the shortcut keys in the bottom right to set a shortcut.";

    [Fact]
    public void Toggle_SelectsTheSkillForAShortcut()
    {
        var shortcut = new SkillShortcut();

        var line = shortcut.Toggle(Dagger, "Dagger", 0);

        Assert.Equal(Dagger, shortcut.Selected);
        Assert.Equal(new LogLine("Dagger selected for shortcut.", (int)TextColour.Lime), line);
    }

    [Fact]
    public void Toggle_OnTheSelectedSkillUnselectsIt()
    {
        var shortcut = new SkillShortcut();
        shortcut.Toggle(Dagger, "Dagger", 0);

        var line = shortcut.Toggle(Dagger, "Dagger", 0);

        Assert.Null(shortcut.Selected);
        Assert.Equal(new LogLine("Dagger no longer selected for shortcut.", (int)TextColour.Lime), line);
    }

    [Fact]
    public void Toggle_OnAnotherSkillSelectsThatOne()
    {
        var shortcut = new SkillShortcut();
        shortcut.Toggle(Dagger, "Dagger", 0);

        var line = shortcut.Toggle(Sword, "Sword", 0);

        Assert.Equal(Sword, shortcut.Selected);
        Assert.Equal(new LogLine("Sword selected for shortcut.", (int)TextColour.Lime), line);
    }

    [Theory]
    [InlineData(Kindred.Mercenary)]
    [InlineData(Kindred.Templar)]
    [InlineData(Kindred.Harakim)]
    public void Toggle_ExplainsTheShortcutKeysToMercenariesTemplarsAndHarakimTheFirstTime(uint kindred)
    {
        var shortcut = new SkillShortcut();

        var first = shortcut.Toggle(Dagger, "Dagger", kindred);
        shortcut.Toggle(Dagger, "Dagger", kindred);
        var second = shortcut.Toggle(Dagger, "Dagger", kindred);

        Assert.Equal(new LogLine("Dagger selected for shortcut." + ShortcutKeysHint, (int)TextColour.Lime), first);
        Assert.Equal(new LogLine("Dagger selected for shortcut.", (int)TextColour.Lime), second);
    }

    [Fact]
    public void Toggle_DoesNotExplainTheShortcutKeysToOtherClasses()
    {
        var line = new SkillShortcut().Toggle(Dagger, "Dagger", Kindred.SeyanDu);

        Assert.Equal(new LogLine("Dagger selected for shortcut.", (int)TextColour.Lime), line);
    }

    [Theory]
    [InlineData(MapButton.Right, MapKeys.Ctrl, true)]
    [InlineData(MapButton.Right, MapKeys.Alt, true)]
    [InlineData(MapButton.Right, MapKeys.Ctrl | MapKeys.Shift, true)]
    [InlineData(MapButton.Right, MapKeys.None, false)]
    [InlineData(MapButton.Right, MapKeys.Shift, false)]
    [InlineData(MapButton.Left, MapKeys.Ctrl, false)]
    public void SelectsItem_IsARightClickWithControlOrAlt(MapButton button, MapKeys keys, bool selects)
    {
        Assert.Equal(selects, SkillShortcut.SelectsItem(button, keys));
    }

    [Fact]
    public void ToggleInventory_SelectsTheSlotAndExplainsTheShortcutKeysTheFirstTime()
    {
        var shortcut = new SkillShortcut();

        var first = shortcut.ToggleInventory(5);

        Assert.Equal(105, shortcut.Selected);
        var text = "Inventory slot 5 selected for shortcut." + ShortcutKeysHint;
        Assert.Equal(new LogLine(text, (int)TextColour.Lime), first);
    }

    [Fact]
    public void ToggleInventory_OnTheSelectedSlotUnselectsItAndThenSelectsItWithoutTheHint()
    {
        var shortcut = new SkillShortcut();
        shortcut.ToggleInventory(5);

        var unselected = shortcut.ToggleInventory(5);
        var selected = shortcut.ToggleInventory(5);

        var unselectedText = "Inventory slot 5 no longer selected for shortcut.";
        Assert.Equal(new LogLine(unselectedText, (int)TextColour.Lime), unselected);
        Assert.Equal(new LogLine("Inventory slot 5 selected for shortcut.", (int)TextColour.Lime), selected);
    }

    [Fact]
    public void ToggleEquipment_SelectsTheGearSlotAndExplainsTheShortcutKeysTheFirstTime()
    {
        var shortcut = new SkillShortcut();

        var first = shortcut.ToggleEquipment(WornSlot.Head);

        Assert.Equal(200, shortcut.Selected);
        var text = "*Helmet equipment slot selected for shortcut." + ShortcutKeysHint;
        Assert.Equal(new LogLine(text, (int)TextColour.Lime), first);
    }

    [Fact]
    public void ToggleEquipment_OnTheSelectedSlotUnselectsIt()
    {
        var shortcut = new SkillShortcut();
        shortcut.ToggleEquipment(WornSlot.MainHand);

        var line = shortcut.ToggleEquipment(WornSlot.MainHand);

        Assert.Null(shortcut.Selected);
        var text = "*Weapon equipment slot no longer selected for shortcut.";
        Assert.Equal(new LogLine(text, (int)TextColour.Lime), line);
    }

    [Fact]
    public void Toggles_ExplainTheShortcutKeysOnceForSkillsInventoryAndEquipmentEach()
    {
        var shortcut = new SkillShortcut();
        shortcut.Toggle(Dagger, "Dagger", Kindred.Mercenary);
        shortcut.ToggleInventory(5);

        var line = shortcut.ToggleEquipment(WornSlot.Head);

        Assert.EndsWith(ShortcutKeysHint, line.Text);
    }

    [Fact]
    public void Toggle_OnASkillReplacesASelectedItem()
    {
        var shortcut = new SkillShortcut();
        shortcut.ToggleInventory(5);

        shortcut.Toggle(Dagger, "Dagger", 0);

        Assert.Equal(Dagger, shortcut.Selected);
    }
}
