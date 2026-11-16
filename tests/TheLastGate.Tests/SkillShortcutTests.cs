using TheLastGate.Assets;
using TheLastGate.State;
using TheLastGate.Stats;

namespace TheLastGate.Tests;

public class SkillShortcutTests
{
    private const int Dagger = 3;
    private const int Sword = 4;

    private const string ShortcutKeysHint = "Dagger selected for shortcut. Right-click on one of the shortcut keys "
        + "in the bottom right to set a shortcut.";

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

        Assert.Equal(new LogLine(ShortcutKeysHint, (int)TextColour.Lime), first);
        Assert.Equal(new LogLine("Dagger selected for shortcut.", (int)TextColour.Lime), second);
    }

    [Fact]
    public void Toggle_DoesNotExplainTheShortcutKeysToOtherClasses()
    {
        var line = new SkillShortcut().Toggle(Dagger, "Dagger", Kindred.SeyanDu);

        Assert.Equal(new LogLine("Dagger selected for shortcut.", (int)TextColour.Lime), line);
    }
}
