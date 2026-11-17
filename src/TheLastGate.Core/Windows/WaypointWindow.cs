using System;
using TheLastGate.Net;
using TheLastGate.World;

namespace TheLastGate.Windows;

public sealed class WaypointWindow
{
    public const int VisibleRows = 8;
    public const int MaxScroll = Waypoints.Count - VisibleRows;

    private const int ArrowRows = 2;
    private const int ArrowRowsWithModifier = 8;
    private const int BarTopAtStart = 36;
    private const int BarTravel = 176;
    private const int DescribeOffset = 32;

    public WaypointWindow(WindowPause pause) => Pause = pause;

    public WindowPause Pause { get; }

    public bool IsOpen { get; private set; }

    // Kept while the window is shut, so it reopens where it was left.
    public int Scroll { get; private set; }

    public static int BarTop(int scroll) => BarTopAtStart + scroll * BarTravel / MaxScroll;

    public static int ArrowStep(MapKeys keys) => keys == MapKeys.None ? ArrowRows : ArrowRowsWithModifier;

    public void Open() => IsOpen = true;

    public bool Close()
    {
        var wasOpen = IsOpen;
        IsOpen = false;
        return wasOpen;
    }

    public void ScrollBy(int rows) => Scroll = Math.Clamp(Scroll + rows, 0, MaxScroll);

    public Waypoint At(int row) => Waypoints.All[row + Scroll];

    // Travelling to an unknown waypoint is still sent; the server answers that it isn't known yet.
    public byte[]? Choose(int row, MapButton button, int foundMask)
    {
        var number = At(row).Number;
        switch (button)
        {
            case MapButton.Left:
                if (Waypoints.Found(foundMask, number))
                {
                    IsOpen = false;
                    Pause.Start();
                }

                return ClientPackets.OneValue(ClientCommand.Waypoints, number);
            case MapButton.Right:
                return ClientPackets.OneValue(ClientCommand.Waypoints, number + DescribeOffset);
            default:
                return null;
        }
    }
}
