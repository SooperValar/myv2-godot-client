using System;

namespace TheLastGate.Windows;

public enum OptionsTab
{
    General,
    Audio,
    Keybinds,
}

public sealed class OptionsWindow
{
    public bool IsOpen { get; private set; }

    public OptionsTab Tab { get; private set; }

    public void Toggle() => throw new NotImplementedException();

    public bool Close() => throw new NotImplementedException();

    public void Show(OptionsTab tab) => throw new NotImplementedException();
}
