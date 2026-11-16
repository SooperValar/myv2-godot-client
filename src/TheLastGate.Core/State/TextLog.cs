using System;
using System.Collections.Generic;

namespace TheLastGate.State;

public sealed class TextLog
{
    public const int MaxLines = 220;

    private const int PendingCapacity = 512;

    private readonly List<LogLine> lines = [];

    public IReadOnlyList<LogLine> Lines => lines;

    internal byte[] Pending { get; } = new byte[PendingCapacity];
    internal int PendingLength { get; set; }

    public event Action<LogLine>? LineAdded;

    public void Add(LogLine line)
    {
        if (lines.Count == MaxLines)
        {
            lines.RemoveAt(0);
        }

        lines.Add(line);
        LineAdded?.Invoke(line);
    }
}
