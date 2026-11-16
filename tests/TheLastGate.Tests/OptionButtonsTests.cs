using TheLastGate.Characters;
using TheLastGate.Hud;
using TheLastGate.Net;

namespace TheLastGate.Tests;

public class OptionButtonsTests
{
    [Theory]
    [InlineData(OptionButton.Fast, 2)]
    [InlineData(OptionButton.Normal, 1)]
    [InlineData(OptionButton.Slow, 0)]
    public void Packet_ChangesTheMovementMode(OptionButton button, int mode)
    {
        Assert.Equal(ClientPackets.ShortAndInt(ClientCommand.Mode, mode, 0), OptionButtons.Packet(button));
    }

    [Theory]
    [InlineData(OptionButton.HealthPercent)]
    [InlineData(OptionButton.Names)]
    [InlineData(OptionButton.Options)]
    [InlineData(OptionButton.Exit)]
    public void Packet_SendsNothingForOtherButtons(OptionButton button)
    {
        Assert.Null(OptionButtons.Packet(button));
    }

    [Fact]
    public void Toggle_FlipsEachButtonsSetting()
    {
        var settings = new CharacterSettings
        {
            ShowHealthPercent = false,
            ShowStatBase = false,
            HideWalls = false,
            ShowNames = false,
            ShowHealthBars = false,
        };

        Assert.True(OptionButtons.Toggle(OptionButton.HealthPercent, settings));
        Assert.True(OptionButtons.Toggle(OptionButton.StatBase, settings));
        Assert.True(OptionButtons.Toggle(OptionButton.HideWalls, settings));
        Assert.True(OptionButtons.Toggle(OptionButton.Names, settings));
        Assert.True(OptionButtons.Toggle(OptionButton.HealthBars, settings));

        Assert.True(settings.ShowHealthPercent);
        Assert.True(settings.ShowStatBase);
        Assert.True(settings.HideWalls);
        Assert.True(settings.ShowNames);
        Assert.True(settings.ShowHealthBars);

        OptionButtons.Toggle(OptionButton.Names, settings);
        Assert.False(settings.ShowNames);
    }

    [Theory]
    [InlineData(OptionButton.Fast)]
    [InlineData(OptionButton.Options)]
    [InlineData(OptionButton.Exit)]
    public void Toggle_LeavesSettingsAloneForOtherButtons(OptionButton button)
    {
        var settings = new CharacterSettings();

        Assert.False(OptionButtons.Toggle(button, settings));
        Assert.True(settings.HideWalls);
        Assert.True(settings.ShowNames);
    }

    [Theory]
    [InlineData(2, OptionButton.Fast)]
    [InlineData(1, OptionButton.Normal)]
    [InlineData(0, OptionButton.Slow)]
    public void IsMarked_MarksTheCurrentMode(int mode, OptionButton marked)
    {
        var settings = new CharacterSettings();

        foreach (var button in new[] { OptionButton.Fast, OptionButton.Normal, OptionButton.Slow })
        {
            Assert.Equal(button == marked, OptionButtons.IsMarked(button, settings, mode));
        }
    }

    [Fact]
    public void IsMarked_MarksEachToggleThatIsOn()
    {
        var settings = new CharacterSettings
        {
            ShowHealthPercent = true,
            ShowStatBase = false,
            HideWalls = true,
            ShowNames = false,
            ShowHealthBars = true,
        };

        Assert.True(OptionButtons.IsMarked(OptionButton.HealthPercent, settings, 1));
        Assert.False(OptionButtons.IsMarked(OptionButton.StatBase, settings, 1));
        Assert.True(OptionButtons.IsMarked(OptionButton.HideWalls, settings, 1));
        Assert.False(OptionButtons.IsMarked(OptionButton.Names, settings, 1));
        Assert.True(OptionButtons.IsMarked(OptionButton.HealthBars, settings, 1));
        Assert.False(OptionButtons.IsMarked(OptionButton.Options, settings, 1));
        Assert.False(OptionButtons.IsMarked(OptionButton.Exit, settings, 1));
    }

    [Theory]
    [InlineData(
        OptionButton.Fast,
        "F1/FAST: Makes you move faster, but uses more endurance. You will also be seen more easily.")]
    [InlineData(OptionButton.Normal, "F2/NORMAL: Move at normal speed. Takes no endurance when not fighting.")]
    [InlineData(
        OptionButton.Slow,
        "F3/SLOW: Makes you move slower. You regain endurance while not fighting. Decreases chances of being seen.")]
    [InlineData(
        OptionButton.HealthPercent,
        "F4/HEALTH: Toggle whether the current health of all characters is displayed as a percent.")]
    [InlineData(
        OptionButton.StatBase,
        "F5/STATS: Toggle seeing stat bases on the skill list, displayed as blue numbers.")]
    [InlineData(OptionButton.HideWalls, "F6/HIDE: Toggle whether walls and other high objects are displayed.")]
    [InlineData(OptionButton.Names, "F7/NAMES: Toggle whether the name of all characters is displayed.")]
    [InlineData(
        OptionButton.HealthBars,
        "F8/BARS: Toggle whether the current health of all characters is displayed as bars.")]
    [InlineData(
        OptionButton.Exit,
        "F12/EXIT: Leave the game immediately. Will cost you 50% health and can result in death.")]
    public void Help_GivesTheCurrentClientsText(OptionButton button, string help)
    {
        Assert.Equal(help, OptionButtons.Help(button));
    }

    [Theory]
    [InlineData(OptionButton.Options)]
    [InlineData(OptionButton.FirstBlank)]
    [InlineData(OptionButton.SecondBlank)]
    public void Help_HasNothingForTheOtherButtons(OptionButton button)
    {
        Assert.Null(OptionButtons.Help(button));
    }
}
