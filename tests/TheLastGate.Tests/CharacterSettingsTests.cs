using TheLastGate.Characters;

namespace TheLastGate.Tests;

public class CharacterSettingsTests
{
    [Fact]
    public void New_ShowsWallsNamesAndHealthPercentWithAnEmptyHotbar()
    {
        var settings = new CharacterSettings();

        Assert.True(settings.HideWalls);
        Assert.True(settings.ShowNames);
        Assert.True(settings.ShowHealthPercent);
        Assert.False(settings.ShowHealthBars);
        Assert.False(settings.ShowStatBase);
        Assert.All(settings.HotbarActions, action => Assert.Equal(CharacterSettings.Unassigned, action));
        Assert.All(settings.HotbarNames, name => Assert.Equal("-", name));
    }

    [Fact]
    public void Write_ThenParse_KeepsTogglesAndHotbar()
    {
        var settings = new CharacterSettings { HideWalls = false, ShowHealthBars = true };
        settings.HotbarActions[3] = 17;
        settings.HotbarNames[3] = "Protect";
        var stored = new byte[CharacterRecord.SettingsSize];

        settings.Write(stored);
        var parsed = CharacterSettings.Parse(stored);

        Assert.False(parsed.HideWalls);
        Assert.True(parsed.ShowNames);
        Assert.True(parsed.ShowHealthBars);
        Assert.Equal(17, parsed.HotbarActions[3]);
        Assert.Equal("Protect", parsed.HotbarNames[3]);
        Assert.Equal(CharacterSettings.Unassigned, parsed.HotbarActions[4]);
    }

    [Fact]
    public void Write_TruncatesHotbarNamesToFitTheirField()
    {
        var settings = new CharacterSettings();
        settings.HotbarNames[0] = "Magic Shield";
        var stored = new byte[CharacterRecord.SettingsSize];

        settings.Write(stored);

        Assert.Equal("Magic S", CharacterSettings.Parse(stored).HotbarNames[0]);
    }

    [Fact]
    public void SetStartingHotbar_GivesATemplarCleave()
    {
        AssertStartingHotbar(CharacterClass.Templar, (7, 40, "Cleave"));
    }

    [Fact]
    public void SetStartingHotbar_GivesAHarakimBlastGhostCompanionMagicShieldAndProtect()
    {
        AssertStartingHotbar(
            CharacterClass.Harakim,
            (3, 17, "Protect"),
            (4, 11, "Magic S"),
            (7, 24, "Blast"),
            (11, 27, "Ghost C"));
    }

    [Theory]
    [InlineData(CharacterClass.Mercenary)]
    [InlineData(CharacterClass.Seyan)]
    [InlineData(CharacterClass.Lycanthrope)]
    public void SetStartingHotbar_GivesEveryOtherClassBlindAndProtect(CharacterClass characterClass)
    {
        AssertStartingHotbar(characterClass, (3, 17, "Protect"), (11, 37, "Blind"));
    }

    [Fact]
    public void SetStartingHotbar_ClearsWhatTheHotbarHeldBefore()
    {
        var settings = new CharacterSettings();
        settings.HotbarActions[10] = 105;
        settings.HotbarNames[10] = "Potion";

        settings.SetStartingHotbar(CharacterClass.Templar);

        Assert.Equal(CharacterSettings.Unassigned, settings.HotbarActions[10]);
        Assert.Equal("-", settings.HotbarNames[10]);
    }

    private static void AssertStartingHotbar(
        CharacterClass characterClass,
        params (int Slot, int Skill, string Name)[] classSkills)
    {
        var settings = new CharacterSettings();

        settings.SetStartingHotbar(characterClass);

        var expectedActions = Enumerable.Repeat(CharacterSettings.Unassigned, CharacterSettings.HotbarSlots).ToArray();
        var expectedNames = Enumerable.Repeat("-", CharacterSettings.HotbarSlots).ToArray();
        foreach (var (slot, skill, name) in classSkills.Append((0, 50, "Light")).Append((19, 51, "Recall")))
        {
            expectedActions[slot] = skill;
            expectedNames[slot] = name;
        }

        Assert.Equal(expectedActions, settings.HotbarActions);
        Assert.Equal(expectedNames, settings.HotbarNames);
    }
}
