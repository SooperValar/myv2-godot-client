using TheLastGate.Net;
using TheLastGate.State;
using TheLastGate.Windows;

namespace TheLastGate.Tests;

public class OptionsWindowTests
{
    private readonly GameState state = new();
    private readonly OptionsWindow window = new();

    [Fact]
    public void Toggle_OpensOnTheGeneralTabAndClosesAgain()
    {
        window.Toggle();
        Assert.True(window.IsOpen);
        Assert.Equal(OptionsTab.General, window.Tab);

        window.Toggle();
        Assert.False(window.IsOpen);
    }

    [Theory]
    [InlineData(OptionsTab.Audio)]
    [InlineData(OptionsTab.Display)]
    [InlineData(OptionsTab.Keybinds)]
    public void Show_SwitchesTab(OptionsTab tab)
    {
        window.Toggle();

        window.Show(tab);

        Assert.Equal(tab, window.Tab);
    }

    [Fact]
    public void Tabs_KeepTheCurrentClientsThreeThenTheGodotDisplayTab()
    {
        Assert.Equal(
            [OptionsTab.General, OptionsTab.Audio, OptionsTab.Keybinds, OptionsTab.Display],
            Enum.GetValues<OptionsTab>());
    }

    [Fact]
    public void ReopeningTheWindow_KeepsTheLastTab()
    {
        window.Toggle();
        window.Show(OptionsTab.Audio);
        window.Close();

        window.Toggle();

        Assert.Equal(OptionsTab.Audio, window.Tab);
    }

    [Fact]
    public void Close_SaysWhetherTheWindowWasOpen()
    {
        window.Toggle();

        Assert.True(window.Close());
        Assert.False(window.IsOpen);
        Assert.False(window.Close());
    }

    [Fact]
    public void Escape_ClosesTheOptionsWithoutAPause()
    {
        state.OptionsWindow.Toggle();

        Assert.True(state.CloseWindowsByEscape());

        Assert.False(state.OptionsWindow.IsOpen);
        Assert.False(state.Pause.Active);
    }

    [Fact]
    public void ThePause_LeavesTheOptionsOpen()
    {
        state.OptionsWindow.Toggle();
        state.Pause.Start();

        new TickProcessor(state).Process(TestCommand.Payload());

        Assert.True(state.OptionsWindow.IsOpen);
    }
}
