using GdUnit4;
using TheLastGate.Assets;
using TheLastGate.Characters;
using TheLastGate.Hotbar;
using TheLastGate.State;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Hotbar;

[TestSuite]
public class HotbarAssignmentsTests
{
    private const int Protect = 17;
    private const int Bless = 21;
    private const int Light = 14;
    private const int Identify = 22;
    private const int Sword = 3;
    private const int Meditate = 30;

    [TestCase]
    public void Assign_PutsTheSkillOnTheButton()
    {
        var settings = new CharacterSettings();

        var line = HotbarAssignments.Assign(settings, 3, Protect, "Protect", "C-4");

        AssertThat(settings.HotbarActions[3]).IsEqual(Protect);
        AssertThat(settings.HotbarNames[3]).IsEqual("Protect");
        AssertThat(line).IsEqual(new LogLine("C-4 is now Protect.", (int)TextColour.Yellow));
    }

    [TestCase]
    public void Assign_ShortensTheButtonNameToSevenLetters()
    {
        var settings = new CharacterSettings();

        var line = HotbarAssignments.Assign(settings, 3, 11, "Magic Shield", "C-4");

        AssertThat(settings.HotbarNames[3]).IsEqual("Magic S");
        AssertThat(line.Text).IsEqual("C-4 is now Magic Shield.");
    }

    [TestCase]
    public void Assign_ReplacesAnotherSkillOnTheButton()
    {
        var settings = new CharacterSettings();
        HotbarAssignments.Assign(settings, 3, Protect, "Protect", "C-4");

        HotbarAssignments.Assign(settings, 3, Bless, "Bless", "C-4");

        AssertThat(settings.HotbarActions[3]).IsEqual(Bless);
        AssertThat(settings.HotbarNames[3]).IsEqual("Bless");
    }

    [TestCase]
    public void Assign_OfTheSkillAlreadyOnTheButtonUnassignsIt()
    {
        var settings = new CharacterSettings();
        HotbarAssignments.Assign(settings, 3, Protect, "Protect", "C-4");

        var line = HotbarAssignments.Assign(settings, 3, Protect, "Protect", "C-4");

        AssertThat(settings.HotbarActions[3]).IsEqual(CharacterSettings.Unassigned);
        AssertThat(settings.HotbarNames[3]).IsEqual("-");
        AssertThat(line).IsEqual(new LogLine("C-4 is now unassigned.", (int)TextColour.Yellow));
    }

    [TestCase]
    public void Assign_PutsAnItemOnTheButton()
    {
        var settings = new CharacterSettings();

        var line = HotbarAssignments.Assign(settings, 0, 105, HotbarButtons.ItemName(105), "A-Keypad 1");

        AssertThat(settings.HotbarActions[0]).IsEqual(105);
        AssertThat(settings.HotbarNames[0]).IsEqual("Item 5");
        AssertThat(line).IsEqual(new LogLine("A-Keypad 1 is now Item 5.", (int)TextColour.Yellow));
    }

    [TestCase]
    public void Clear_UnassignsTheButton()
    {
        var settings = new CharacterSettings();
        HotbarAssignments.Assign(settings, 3, Protect, "Protect", "C-4");

        HotbarAssignments.Clear(settings, 3);

        AssertThat(settings.HotbarActions[3]).IsEqual(CharacterSettings.Unassigned);
        AssertThat(settings.HotbarNames[3]).IsEqual("-");
    }

    [TestCase]
    public void MenuSkills_ListsTheLearntSkillsMyV2LetsYouUseInMenuOrder()
    {
        var skills = new PlayerState().Skills;
        skills[Protect][0] = 10;
        skills[Light][0] = 1;
        skills[Bless][0] = 5;
        skills[Identify][0] = 1;
        skills[Sword][0] = 20;
        skills[Meditate][0] = 20;

        AssertThat(HotbarAssignments.MenuSkills(skills)).ContainsExactly(Bless, Protect, Identify, Light);
    }
}
