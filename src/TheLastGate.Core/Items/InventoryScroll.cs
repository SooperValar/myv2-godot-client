using System;

namespace TheLastGate.Items;

public sealed class InventoryScroll
{
    public const int Columns = 10;
    public const int VisibleRows = 3;
    public const int VisibleSlots = Columns * VisibleRows;

    public int Row => throw new NotImplementedException();

    public static int LastRow => throw new NotImplementedException();

    public void ScrollBy(int rows) => throw new NotImplementedException();

    public void ScrollTo(int row) => throw new NotImplementedException();

    public int SlotShownIn(int cell) => throw new NotImplementedException();
}
