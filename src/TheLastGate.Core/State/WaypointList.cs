using System.Collections.Generic;

namespace TheLastGate.State;

public sealed class WaypointList
{
    private readonly List<WaypointRow> incoming = [];

    public IReadOnlyList<WaypointRow> Rows { get; private set; } = [];

    public bool Open { get; private set; }

    internal void Close() => Open = false;

    internal bool Receive(int rowIndex, int rowCount, WaypointRow row)
    {
        if (rowIndex == 0)
        {
            incoming.Clear();
        }

        if (rowIndex != incoming.Count)
        {
            return false;
        }

        incoming.Add(row);
        if (rowIndex != rowCount - 1)
        {
            return false;
        }

        Rows = [.. incoming];
        Open = true;
        return true;
    }
}
