using TheLastGate.Assets;
using TheLastGate.Hotkeys;
using TheLastGate.State;
using TheLastGate.Windows;
using TheLastGate.World;

namespace TheLastGate.Tests;

public class KeybindCaptureTests
{
    private readonly KeyBindings bindings = new();
    private readonly OptionsWindow window = new();

    public KeybindCaptureTests()
    {
        window.Toggle();
        window.Show(OptionsTab.Keybinds);
    }

    [Fact]
    public void StartCapture_WaitsForAKeyOnThatRow()
    {
        window.StartCapture("speed_fast_hotkey");

        Assert.Equal("speed_fast_hotkey", window.Capturing);
    }

    [Fact]
    public void StartCapture_OnAnotherRow_MovesTheWait()
    {
        window.StartCapture("speed_fast_hotkey");

        window.StartCapture("speed_slow_hotkey");

        Assert.Equal("speed_slow_hotkey", window.Capturing);
    }

    [Fact]
    public void Capture_BindsTheKeyWithItsModifiersAndStopsWaiting()
    {
        window.StartCapture("toggle_worldmap_hotkey");

        var line = window.Capture(new KeyChord("M", MapKeys.Ctrl | MapKeys.Alt), bindings);

        Assert.Null(line);
        Assert.Equal("Ctrl+Alt+M", bindings.LongName("toggle_worldmap_hotkey"));
        Assert.Null(window.Capturing);
    }

    [Fact]
    public void Capture_TakesTheKeyFromTheBindingThatHadItAndSaysSoInRed()
    {
        window.StartCapture("toggle_worldmap_hotkey");

        var line = window.Capture(new KeyChord("F1", MapKeys.None), bindings);

        Assert.Equal(new LogLine("Unbound Fast Mode (F1) - key reassigned.", (int)TextColour.Red), line);
        Assert.Equal("F1", bindings.LongName("toggle_worldmap_hotkey"));
        Assert.Equal("Not Set", bindings.LongName("speed_fast_hotkey"));
    }

    [Theory]
    [InlineData(MapKeys.None)]
    [InlineData(MapKeys.Ctrl)]
    public void Capture_LeavesTheSlashForChatAndKeepsWaiting(MapKeys modifiers)
    {
        window.StartCapture("toggle_worldmap_hotkey");

        window.Capture(new KeyChord("/", modifiers), bindings);

        Assert.Equal("Not Set", bindings.LongName("toggle_worldmap_hotkey"));
        Assert.Equal("toggle_worldmap_hotkey", window.Capturing);
    }

    [Fact]
    public void Capture_AcceptsTheKeypadSlash()
    {
        window.StartCapture("toggle_worldmap_hotkey");

        window.Capture(new KeyChord("Keypad /", MapKeys.None), bindings);

        Assert.Equal("Keypad /", bindings.LongName("toggle_worldmap_hotkey"));
    }

    [Theory]
    [InlineData(KeyNames.Shift)]
    [InlineData(KeyNames.Ctrl)]
    [InlineData(KeyNames.Alt)]
    public void Capture_KeepsWaitingWhenAModifierIsPressedForAnOrdinaryBinding(string modifier)
    {
        window.StartCapture("toggle_worldmap_hotkey");

        window.Capture(new KeyChord(modifier, MapKeys.None), bindings);

        Assert.Equal("Not Set", bindings.LongName("toggle_worldmap_hotkey"));
        Assert.Equal("toggle_worldmap_hotkey", window.Capturing);
    }

    [Fact]
    public void Capture_BindsAModifierOnItsOwnToAHoldBinding()
    {
        window.StartCapture("item_tooltip_hotkey");

        window.Capture(new KeyChord(KeyNames.Shift, MapKeys.Ctrl), bindings);

        Assert.Equal("Shift", bindings.LongName("item_tooltip_hotkey"));
        Assert.Null(window.Capturing);
    }

    [Fact]
    public void Capture_DoesNothingWhenNoRowIsWaiting()
    {
        var line = window.Capture(new KeyChord("F1", MapKeys.Ctrl), bindings);

        Assert.Null(line);
        Assert.Equal("F1", bindings.LongName("speed_fast_hotkey"));
        Assert.Null(bindings.Find(new KeyChord("F1", MapKeys.Ctrl)));
    }

    [Fact]
    public void CancelCapture_KeepsTheBinding()
    {
        window.StartCapture("speed_fast_hotkey");

        window.CancelCapture();

        Assert.Null(window.Capturing);
        Assert.Equal("F1", bindings.LongName("speed_fast_hotkey"));
    }

    [Fact]
    public void UnbindCaptured_ClearsTheBindingAndStopsWaiting()
    {
        window.StartCapture("speed_fast_hotkey");

        window.UnbindCaptured(bindings);

        Assert.Null(window.Capturing);
        Assert.Equal("Not Set", bindings.LongName("speed_fast_hotkey"));
    }

    [Fact]
    public void ClosingTheWindow_StopsWaiting()
    {
        window.StartCapture("speed_fast_hotkey");

        window.Close();

        Assert.Null(window.Capturing);
    }

    [Fact]
    public void TogglingTheWindow_StopsWaiting()
    {
        window.StartCapture("speed_fast_hotkey");

        window.Toggle();

        Assert.Null(window.Capturing);
    }

    [Fact]
    public void ChangingTab_StopsWaiting()
    {
        window.StartCapture("speed_fast_hotkey");

        window.Show(OptionsTab.General);

        Assert.Null(window.Capturing);
    }

    [Fact]
    public void ChooseResetAll_FirstAsksForConfirmation()
    {
        bindings.Clear("speed_fast_hotkey");

        var line = window.ChooseResetAll(bindings);

        Assert.Null(line);
        Assert.True(window.ConfirmingReset);
        Assert.Equal("Not Set", bindings.LongName("speed_fast_hotkey"));
    }

    [Fact]
    public void ChooseResetAll_Twice_RestoresEveryDefaultAndSaysSoInGreen()
    {
        bindings.Clear("speed_fast_hotkey");
        window.ChooseResetAll(bindings);

        var line = window.ChooseResetAll(bindings);

        Assert.Equal(new LogLine("Keybinds have been set to their defaults.", (int)TextColour.Green), line);
        Assert.False(window.ConfirmingReset);
        Assert.Equal("F1", bindings.LongName("speed_fast_hotkey"));
    }

    [Theory]
    [InlineData(OptionsTab.Keybinds)]
    [InlineData(OptionsTab.General)]
    public void ChoosingATab_DropsTheConfirmation(OptionsTab tab)
    {
        window.ChooseResetAll(bindings);

        window.Show(tab);

        Assert.False(window.ConfirmingReset);
    }

    [Fact]
    public void TogglingTheWindow_DropsTheConfirmation()
    {
        window.ChooseResetAll(bindings);

        window.Toggle();
        window.Toggle();

        Assert.False(window.ConfirmingReset);
    }

    [Fact]
    public void ClosingTheWindow_DropsTheConfirmation()
    {
        window.ChooseResetAll(bindings);

        window.Close();

        Assert.False(window.ConfirmingReset);
    }
}
