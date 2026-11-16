using System;
using TheLastGate.State;

namespace TheLastGate.Items;

public sealed class InventoryScroll
{
    public const int Columns = 10;
    public const int VisibleRows = 3;
    public const int VisibleSlots = Columns * VisibleRows;

    public int Row { get; private set; }

    public static int LastRow => PlayerState.InventorySlots / Columns - VisibleRows;

    public void ScrollBy(int rows) => ScrollTo(Row + rows);

    public void ScrollTo(int row) => Row = Math.Clamp(row, 0, LastRow);

    public int SlotShownIn(int cell) => cell + Row * Columns;
}
