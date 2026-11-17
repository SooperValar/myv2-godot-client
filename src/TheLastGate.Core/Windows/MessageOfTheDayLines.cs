using System;
using System.Collections.Generic;
using TheLastGate.State;

namespace TheLastGate.Windows;

public sealed class MessageOfTheDayLines
{
    public const int VisibleLines = 27;

    public IReadOnlyList<LogLine> Lines => throw new NotImplementedException();

    public void Add(LogLine line) => throw new NotImplementedException();
}
