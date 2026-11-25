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

    private IReadOnlyList<WaypointRow> all = [];
    private IReadOnlyList<WaypointRow> shown = [];

    public WaypointWindow(WindowPause pause) => Pause = pause;

    public WindowPause Pause { get; }

    public bool IsOpen { get; private set; }

    public int Scroll { get; private set; }

    public string Search { get; private set; } = "";

    public int ShownCount => shown.Count;

    public int? Selected { get; private set; }

    public int ScrollLimit => Math.Max(0, shown.Count - VisibleRows);

    // Unfound waypoints are left out of a search so their names stay secret.
    public static bool Matches(WaypointRow waypoint, string search) =>
        waypoint.Found && waypoint.Name.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase);

    // The server sends the whole list whenever the player clicks a waypoint, and the list opens on the one he stands at.
    public void Show(IReadOnlyList<WaypointRow> rows)
    {
        all = rows;
        shown = rows;
        Search = "";
        Selected = null;
        var here = rows.ToList().FindIndex(row => row.YouAreHere);
        Scroll = here < 0 ? 0 : Math.Clamp(here - VisibleRows / 2, 0, ScrollLimit);
        IsOpen = true;
    }

    public bool Close()
    {
        var wasOpen = IsOpen;
        IsOpen = false;
        return wasOpen;
    }

    public void SetSearch(string search)
    {
        if (search == Search)
        {
            return;
        }

        Search = search;
        shown = search.Trim().Length == 0 ? all : all.Where(waypoint => Matches(waypoint, search)).ToList();
        Scroll = 0;
        Selected = search.Trim().Length > 0 && shown.Count > 0 ? 0 : null;
    }

    public void ClearSearch()
    {
        SetSearch("");
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

    public WaypointRow At(int row) => shown[row + Scroll];

    public string Hint(int? hoveredRow)
    {
        var index = hoveredRow is { } row && HasRow(row) ? row + Scroll : Selected;
        if (index is not { } shownIndex)
        {
            return $"Waypoints found: {all.Count(waypoint => waypoint.Found)} of {all.Count}. Type to search.";
        }

        var waypoint = shown[shownIndex];
        return !waypoint.Found ? "You have not found this waypoint yet."
            : waypoint.YouAreHere ? "You are here."
            : $"Travel to {waypoint.Name}.";
    }

    public byte[]? ChooseSelected() => Selected is { } index ? ChooseIndex(index, MapButton.Left) : null;

    public byte[]? Choose(int row, MapButton button) => HasRow(row) ? ChooseIndex(row + Scroll, button) : null;

    private byte[]? ChooseIndex(int index, MapButton button)
    {
        var waypoint = shown[index];
        if (button != MapButton.Left || !waypoint.Found || waypoint.YouAreHere)
        {
            return null;
        }

        IsOpen = false;
        Pause.Start();
        return WaypointPackets.Travel(waypoint.Number);
    }
}
