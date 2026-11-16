using System;

namespace TheLastGate.Hud;

public sealed class LogScroll
{
    public const int PageLines = 11;
    public const int WheelLines = 3;
    public const int MaxLinesBack = 176;

    public static readonly TimeSpan ReturnAfter = TimeSpan.FromSeconds(30);

    public int LinesBack => throw new NotImplementedException();

    public void Scroll(int linesBack) => throw new NotImplementedException();

    public void LinesChanged(int linesAdded, int oldestLinesBack) => throw new NotImplementedException();

    public void Advance(TimeSpan elapsed) => throw new NotImplementedException();
}
