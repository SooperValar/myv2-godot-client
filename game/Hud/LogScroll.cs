using System;

namespace TheLastGate.Hud;

public sealed class LogScroll
{
    public const int PageLines = 11;
    public const int WheelLines = 3;
    public const int MaxLinesBack = 176;

    public static readonly TimeSpan ReturnAfter = TimeSpan.FromSeconds(30);

    private int oldestLinesBack;
    private TimeSpan untilReturn;

    public int LinesBack { get; private set; }

    public void Scroll(int linesBack)
    {
        LinesBack = Math.Clamp(LinesBack + linesBack, 0, Math.Min(MaxLinesBack, oldestLinesBack));
        untilReturn = ReturnAfter;
    }

    public void LinesChanged(int linesAdded, int oldestLinesBack)
    {
        this.oldestLinesBack = oldestLinesBack;
        if (LinesBack > 0)
        {
            LinesBack = Math.Min(LinesBack + linesAdded, oldestLinesBack);
        }
    }

    public void Advance(TimeSpan elapsed)
    {
        if (LinesBack == 0)
        {
            return;
        }

        untilReturn -= elapsed;
        if (untilReturn <= TimeSpan.Zero)
        {
            LinesBack = 0;
        }
    }
}
