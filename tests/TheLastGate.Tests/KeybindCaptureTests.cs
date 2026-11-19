using TheLastGate.Assets;
using TheLastGate.Hotkeys;
using TheLastGate.State;
using TheLastGate.Windows;
using TheLastGate.World;

namespace TheLastGate.Tests;

public class KeybindCaptureTests
{
    private static readonly KeyChord FunctionOne = new("F1", MapKeys.None);

    private static readonly IReadOnlyDictionary<string, KeyChord?> Current = new Dictionary<string, KeyChord?>
    {
        ["speed_fast_hotkey"] = FunctionOne,
        ["speed_slow_hotkey"] = new KeyChord("F3", MapKeys.None),
        ["toggle_worldmap_hotkey"] = null,
        ["item_tooltip_hotkey"] = new KeyChord(KeyNames.Alt, MapKeys.None),
    };

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
        var ctrlAltM = new KeyChord("M", MapKeys.Ctrl | MapKeys.Alt);

        var change = window.Capture(ctrlAltM, Current);

        Assert.Equal([new BindingChange("toggle_worldmap_hotkey", ctrlAltM)], change?.Changes);
        Assert.Null(change?.Line);
        Assert.Null(window.Capturing);
    }

    [Fact]
    public void Capture_TakesTheKeyFromTheBindingThatHadItAndSaysSoInRed()
    {
        window.StartCapture("toggle_worldmap_hotkey");

        var change = window.Capture(FunctionOne, Current);

        Assert.Equal(
            [new BindingChange("speed_fast_hotkey", null), new BindingChange("toggle_worldmap_hotkey", FunctionOne)],
            change?.Changes);
        Assert.Equal(new LogLine("Unbound Fast Mode (F1) - key reassigned.", (int)TextColour.Red), change?.Line);
    }

    [Theory]
    [InlineData(MapKeys.None)]
    [InlineData(MapKeys.Ctrl)]
    public void Capture_LeavesTheSlashForChatAndKeepsWaiting(MapKeys modifiers)
    {
        window.StartCapture("toggle_worldmap_hotkey");

        Assert.Null(window.Capture(new KeyChord("/", modifiers), Current));
        Assert.Equal("toggle_worldmap_hotkey", window.Capturing);
    }

    [Fact]
    public void Capture_AcceptsTheKeypadSlash()
    {
        window.StartCapture("toggle_worldmap_hotkey");
        var keypadSlash = new KeyChord("Keypad /", MapKeys.None);

        var change = window.Capture(keypadSlash, Current);

        Assert.Equal([new BindingChange("toggle_worldmap_hotkey", keypadSlash)], change?.Changes);
    }

    [Theory]
    [InlineData(KeyNames.Shift)]
    [InlineData(KeyNames.Ctrl)]
    [InlineData(KeyNames.Alt)]
    public void Capture_KeepsWaitingWhenAModifierIsPressedForAnOrdinaryBinding(string modifier)
    {
        window.StartCapture("toggle_worldmap_hotkey");

        Assert.Null(window.Capture(new KeyChord(modifier, MapKeys.None), Current));
        Assert.Equal("toggle_worldmap_hotkey", window.Capturing);
    }

    [Fact]
    public void Capture_BindsAModifierOnItsOwnToAHoldBinding()
    {
        window.StartCapture("item_tooltip_hotkey");

        var change = window.Capture(new KeyChord(KeyNames.Shift, MapKeys.Ctrl), Current);

        Assert.Equal(
            [new BindingChange("item_tooltip_hotkey", new KeyChord(KeyNames.Shift, MapKeys.None))],
            change?.Changes);
        Assert.Null(window.Capturing);
    }

    [Fact]
    public void Capture_DoesNothingWhenNoRowIsWaiting()
    {
        Assert.Null(window.Capture(new KeyChord("F1", MapKeys.Ctrl), Current));
    }

    [Fact]
    public void CancelCapture_StopsWaiting()
    {
        window.StartCapture("speed_fast_hotkey");

        window.CancelCapture();

        Assert.Null(window.Capturing);
    }

    [Fact]
    public void UnbindCaptured_ClearsTheBindingAndStopsWaiting()
    {
        window.StartCapture("speed_fast_hotkey");

        Assert.Equal(new BindingChange("speed_fast_hotkey", null), window.UnbindCaptured());
        Assert.Null(window.Capturing);
    }

    [Fact]
    public void UnbindCaptured_DoesNothingWhenNoRowIsWaiting()
    {
        Assert.Null(window.UnbindCaptured());
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
        Assert.Null(window.ChooseResetAll());
        Assert.True(window.ConfirmingReset);
    }

    [Fact]
    public void ChooseResetAll_Twice_ResetsAndSaysSoInGreen()
    {
        window.ChooseResetAll();

        var line = window.ChooseResetAll();

        Assert.Equal(new LogLine("Keybinds have been set to their defaults.", (int)TextColour.Green), line);
        Assert.False(window.ConfirmingReset);
    }

    [Theory]
    [InlineData(OptionsTab.Keybinds)]
    [InlineData(OptionsTab.General)]
    public void ChoosingATab_DropsTheConfirmation(OptionsTab tab)
    {
        window.ChooseResetAll();

        window.Show(tab);

        Assert.False(window.ConfirmingReset);
    }

    [Fact]
    public void TogglingTheWindow_DropsTheConfirmation()
    {
        window.ChooseResetAll();

        window.Toggle();
        window.Toggle();

        Assert.False(window.ConfirmingReset);
    }

    [Fact]
    public void ClosingTheWindow_DropsTheConfirmation()
    {
        window.ChooseResetAll();

        window.Close();

        Assert.False(window.ConfirmingReset);
    }
}
