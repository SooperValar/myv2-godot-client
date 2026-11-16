using System;

namespace TheLastGate.Stats;

public sealed class SkillScroll
{
    public const int VisibleRows = 10;

    public int Row => throw new NotImplementedException();

    public static int LastRow => throw new NotImplementedException();

    public void ScrollBy(int rows) => throw new NotImplementedException();

    public void ScrollTo(int row) => throw new NotImplementedException();

    public int PositionShownIn(int visibleRow) => throw new NotImplementedException();
}
