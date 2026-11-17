using System;
using TheLastGate.World;

namespace TheLastGate.Windows;

public sealed class WaypointWindow
{
    public const int VisibleRows = 8;
    public const int MaxScroll = Waypoints.Count - VisibleRows;

    public WaypointWindow(WindowPause pause) => Pause = pause;

    public WindowPause Pause { get; }

    public bool IsOpen { get; private set; }

    public int Scroll { get; private set; }

    public static int BarTop(int scroll) => throw new NotImplementedException();

    public static int ArrowStep(MapKeys keys) => throw new NotImplementedException();

    public void Open() => throw new NotImplementedException();

    public bool Close() => throw new NotImplementedException();

    public void ScrollBy(int rows) => throw new NotImplementedException();

    public Waypoint At(int row) => throw new NotImplementedException();

    public byte[]? Choose(int row, MapButton button, int foundMask) => throw new NotImplementedException();
}
