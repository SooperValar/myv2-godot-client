using System.Linq;
using GdUnit4;
using TheLastGate.Hotbar;
using TheLastGate.State;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Hotbar;

[TestSuite]
public class GcCommandsTests
{
    private const int GhostCompanion = 27;

    [TestCase]
    public void Menu_IsEmptyWithoutGhostCompanion()
    {
        AssertThat(GcCommands.MenuFor(new PlayerState().Skills)).IsEmpty();
    }

    [TestCase]
    public void Menu_ListsEveryCommandOnceGhostCompanionIsLearnt()
    {
        var skills = new PlayerState().Skills;
        skills[GhostCompanion][0] = 1;

        AssertThat(GcCommands.MenuFor(skills).Select(entry => entry.Name).ToArray()).ContainsExactly(
            "GC Att", "GC Off", "GC Def", "GC Pas", "GC Wait", "GC Fol", "GC Move", "GC Buff", "GC Tran");
    }

    [TestCase]
    public void Menu_ActionsAreCommandActions()
    {
        var skills = new PlayerState().Skills;
        skills[GhostCompanion][0] = 1;

        foreach (var (action, name) in GcCommands.MenuFor(skills))
        {
            AssertThat(GcCommands.IsCommand(action)).IsTrue();
            AssertThat(GcCommands.Name(action)).IsEqual(name);
        }
    }

    [TestCase("GC Off", "Offense")]
    [TestCase("GC Def", "Defense")]
    [TestCase("GC Pas", "Passive")]
    [TestCase("GC Fol", "Follow")]
    [TestCase("GC Tran", "Transfer")]
    public void Line_SaysTheWordTheCompanionObeys(string name, string line)
    {
        AssertThat(GcCommands.Line(ActionNamed(name), 0)).IsEqual(line);
    }

    [TestCase(0, "#gca")]
    [TestCase(42, "#gca 42")]
    public void Line_OfAttackAddsTheSelectedCharacter(int selectedCharacter, string line)
    {
        AssertThat(GcCommands.Line(ActionNamed("GC Att"), selectedCharacter)).IsEqual(line);
    }

    [TestCase(21)]
    [TestCase(105)]
    [TestCase(208)]
    public void Line_IsNoneForSkillsAndItems(int action)
    {
        AssertThat(GcCommands.IsCommand(action)).IsFalse();
        AssertThat(GcCommands.Line(action, 0)).IsNull();
    }

    [TestCase]
    public void UsePacket_IsNoneForACommand()
    {
        AssertThat(HotbarButtons.UsePacket(ActionNamed("GC Off"), 0)).IsNull();
    }

    private static int ActionNamed(string name)
    {
        var skills = new PlayerState().Skills;
        skills[GhostCompanion][0] = 1;
        return GcCommands.MenuFor(skills).First(entry => entry.Name == name).Action;
    }
}
