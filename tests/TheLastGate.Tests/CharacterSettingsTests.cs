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
}
