using System.Linq;
using TheLastGate.Assets;
using TheLastGate.Hotkeys;
using TheLastGate.State;
using TheLastGate.World;

namespace TheLastGate.Windows;

public enum OptionsTab
{
    General,
    Audio,
    Keybinds,
    Display,
}

// Nothing but its own button, its hotkey and Escape opens or closes it; the tab is remembered while it's shut.
// Leaving the tab or the window stops a key capture, so the keyboard is never left waiting on a hidden row.
public sealed class OptionsWindow
{
    private const string ChatCommandKey = "/";

    public bool IsOpen { get; private set; }

    public OptionsTab Tab { get; private set; }

    public string? Capturing { get; private set; }

    public bool ConfirmingReset { get; private set; }

    public void Toggle()
    {
        IsOpen = !IsOpen;
        StopKeybindChoices();
    }

    public bool Close()
    {
        var wasOpen = IsOpen;
        IsOpen = false;
        StopKeybindChoices();
        return wasOpen;
    }

    public void Show(OptionsTab tab)
    {
        Tab = tab;
        StopKeybindChoices();
    }

    public void StartCapture(string id) => Capturing = id;

    // The slash stays free for chat commands, and a modifier alone only makes sense for a binding that is held.
    public LogLine? Capture(KeyChord chord, KeyBindings bindings)
    {
        if (Capturing is not { } id || chord.Key == ChatCommandKey)
        {
            return null;
        }

        if (IsModifier(chord.Key))
        {
            if (!Bindings.All.First(binding => binding.Id == id).Hold)
            {
                return null;
            }

            chord = chord with { Modifiers = MapKeys.None };
        }

        Capturing = null;
        if (bindings.Assign(id, chord) is not { } displaced)
        {
            return null;
        }

        var name = Bindings.All.First(binding => binding.Id == displaced).Name;
        return new LogLine($"Unbound {name} ({chord.LongName}) - key reassigned.", (int)TextColour.Red);
    }

    public void CancelCapture() => Capturing = null;

    public void UnbindCaptured(KeyBindings bindings)
    {
        if (Capturing is { } id)
        {
            bindings.Clear(id);
        }

        Capturing = null;
    }

    public LogLine? ChooseResetAll(KeyBindings bindings)
    {
        if (!ConfirmingReset)
        {
            ConfirmingReset = true;
            return null;
        }

        bindings.ResetAll();
        ConfirmingReset = false;
        return new LogLine("Keybinds have been set to their defaults.", (int)TextColour.Green);
    }

    private static bool IsModifier(string key) => key is KeyNames.Shift or KeyNames.Ctrl or KeyNames.Alt;

    private void StopKeybindChoices()
    {
        Capturing = null;
        ConfirmingReset = false;
    }
}
