using System;
using System.Collections.Generic;
using TheLastGate.State;

namespace TheLastGate.Windows;

public readonly record struct TextWindowAction(IReadOnlyList<byte[]> Packets, bool PlaysClick);

public sealed class TextWindows
{
    public const int ClosedPauseTicks = 24;

    public bool MessageOfTheDayOpen => throw new NotImplementedException();

    public bool WelcomeOpen => throw new NotImplementedException();

    public int? Tutorial => throw new NotImplementedException();

    public int TutorialPage => throw new NotImplementedException();

    public bool AnyOpen => throw new NotImplementedException();

    public void Show(WindowRequest request) => throw new NotImplementedException();

    public TextWindowAction Close() => throw new NotImplementedException();

    public TextWindowAction Ok() => throw new NotImplementedException();

    public TextWindowAction Left() => throw new NotImplementedException();

    public TextWindowAction Right() => throw new NotImplementedException();

    public bool CloseByEscape() => throw new NotImplementedException();

    public void CancelPause() => throw new NotImplementedException();

    public void EndOfTick() => throw new NotImplementedException();
}
