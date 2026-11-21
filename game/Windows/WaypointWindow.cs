using System;
using System.Collections.Generic;
using System.Linq;
using TheLastGate.Net;
using TheLastGate.State;
using TheLastGate.World;

namespace TheLastGate.Windows;

public sealed class WaypointWindow
{
    public const int VisibleRows = 8;

    private IReadOnlyList<Waypoint> shown = Waypoints.All;

    public WaypointWindow(WindowPause pause) => Pause = pause;

    public WindowPause Pause { get; }

    public bool IsOpen { get; private set; }

    // Kept while the window is shut, so it reopens where it was left.
    public int Scroll { get; private set; }

    public string Search { get; private set; } = "";

    public int ShownCount => shown.Count;

    public int? Selected { get; private set; }

    public int ScrollLimit => Math.Max(0, shown.Count - VisibleRows);

    // Unfound waypoints are left out of a search so their names stay secret.
    public static bool Matches(Waypoint waypoint, string search, int foundMask) =>
        Waypoints.Found(foundMask, waypoint.Number)
        && waypoint.Name.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase);

    public void Open() => IsOpen = true;

    public bool Close()
    {
        var wasOpen = IsOpen;
        IsOpen = false;
        return wasOpen;
    }

    public void SetSearch(string search, int foundMask)
    {
        if (search == Search)
        {
            return;
        }

        Search = search;
        shown = search.Trim().Length == 0
            ? Waypoints.All
            : Waypoints.All.Where(waypoint => Matches(waypoint, search, foundMask)).ToList();
        Scroll = 0;
        Selected = search.Trim().Length > 0 && shown.Count > 0 ? 0 : null;
    }

    public void ClearSearch(int foundMask)
    {
        SetSearch("", foundMask);
        ClearSelection();
    }

    // With nothing selected the first move picks the top row in view, so the list's place is kept.
    public void MoveSelection(int rows)
    {
        if (shown.Count == 0)
        {
            return;
        }

        Selected = Selected is { } current ? Math.Clamp(current + rows, 0, shown.Count - 1) : Scroll;
        if (Selected < Scroll)
        {
            Scroll = Selected.Value;
        }
        else if (Selected >= Scroll + VisibleRows)
        {
            Scroll = Selected.Value - VisibleRows + 1;
        }
    }

    public void ClearSelection() => Selected = null;

    public void ScrollBy(int rows) => ScrollTo(Scroll + rows);

    public void ScrollTo(int row) => Scroll = Math.Clamp(row, 0, ScrollLimit);

    public bool HasRow(int row) => row + Scroll < shown.Count;

    public bool IsSelected(int row) => Selected == row + Scroll;

    public Waypoint At(int row) => shown[row + Scroll];

    public byte[]? ChooseSelected(int foundMask) =>
        Selected is { } index ? ChooseIndex(index, MapButton.Left, foundMask) : null;

    public byte[]? Choose(int row, MapButton button, int foundMask) =>
        HasRow(row) ? ChooseIndex(row + Scroll, button, foundMask) : null;

    // Travelling to an unknown waypoint is still sent; the server answers that it isn't known yet.
    private byte[]? ChooseIndex(int index, MapButton button, int foundMask)
    {
        var number = shown[index].Number;
        switch (button)
        {
            case MapButton.Left:
                if (Waypoints.Found(foundMask, number))
                {
                    IsOpen = false;
                    Pause.Start();
                }

                return WaypointPackets.Travel(number);
            case MapButton.Right:
                return WaypointPackets.Describe(number);
            default:
                return null;
        }
    }
}
