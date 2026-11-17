using System;

namespace TheLastGate.Windows;

public sealed class WindowPause
{
    public const int Ticks = 24;

    public bool Active => throw new NotImplementedException();

    public void Start() => throw new NotImplementedException();

    public void Cancel() => throw new NotImplementedException();

    public void EndOfTick() => throw new NotImplementedException();
}
