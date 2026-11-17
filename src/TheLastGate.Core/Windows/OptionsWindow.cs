using System;
using TheLastGate.Hotkeys;
using TheLastGate.State;

namespace TheLastGate.Windows;

public enum OptionsTab
{
    General,
    Audio,
    Keybinds,
}

// Nothing but its own button, its hotkey and Escape opens or closes it; the tab is remembered while it's shut.
public sealed class OptionsWindow
{
    public bool IsOpen { get; private set; }

    public OptionsTab Tab { get; private set; }

    public string? Capturing { get; private set; }

    public bool ConfirmingReset { get; private set; }

    public void Toggle() => IsOpen = !IsOpen;

    public bool Close()
    {
        var wasOpen = IsOpen;
        IsOpen = false;
        return wasOpen;
    }

    public void Show(OptionsTab tab) => Tab = tab;

    public void StartCapture(string id) => throw new NotImplementedException();

    public LogLine? Capture(KeyChord chord, KeyBindings bindings) => throw new NotImplementedException();

    public void CancelCapture() => throw new NotImplementedException();

    public void UnbindCaptured(KeyBindings bindings) => throw new NotImplementedException();

    public LogLine? ChooseResetAll(KeyBindings bindings) => throw new NotImplementedException();
}
