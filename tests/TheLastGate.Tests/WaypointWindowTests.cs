using TheLastGate.Net;
using TheLastGate.State;
using TheLastGate.Windows;
using TheLastGate.World;

namespace TheLastGate.Tests;

public class WaypointWindowTests
{
    private const int LynboreTavern = 0;
    private const int LizardTempleSouth = 20;
    private const int NothingFound = 0;

    private const int AllFound = (1 << Waypoints.Count) - 1;

    private readonly GameState state = new();
    private readonly WaypointWindow window = new(new WindowPause());

    [Fact]
    public void Waypoints_ListsEveryNumberOnceInTheWindowsOrder()
    {
        Assert.Equal(Waypoints.Count, Waypoints.All.Count);
        Assert.Equal(Enumerable.Range(0, Waypoints.Count), Waypoints.All.Select(waypoint => waypoint.Number).Order());
        Assert.Equal(new Waypoint(0, "Lynbore, Tavern", "\"Humble beginnings.\""), Waypoints.All[0]);
        Assert.Equal(new Waypoint(20, "Lizard Temple, South", "\"Where reptiles implore.\""), Waypoints.All[7]);
        Assert.Equal(new Waypoint(26, "The Burning Plains", "\"The archon waits.\""), Waypoints.All[^1]);
    }

    [Theory]
    [InlineData(0b101, 0, true)]
    [InlineData(0b101, 1, false)]
    [InlineData(0b101, 2, true)]
    [InlineData(1 << 26, 26, true)]
    public void Found_ReadsTheWaypointsBit(int foundMask, int number, bool found)
    {
        Assert.Equal(found, Waypoints.Found(foundMask, number));
    }

    [Fact]
    public void OpenAndClose_ShowAndHideTheWindow()
    {
        window.Open();
        Assert.True(window.IsOpen);

        Assert.True(window.Close());
        Assert.False(window.IsOpen);
        Assert.False(window.Close());
    }

    [Theory]
    [InlineData(3, 3)]
    [InlineData(-1, 0)]
    [InlineData(25, Waypoints.Count - WaypointWindow.VisibleRows)]
    public void ScrollBy_StaysWithinTheList(int rows, int scroll)
    {
        window.ScrollBy(rows);

        Assert.Equal(scroll, window.Scroll);
    }

    [Fact]
    public void At_CountsRowsFromTheScrollPosition()
    {
        window.ScrollBy(7);

        Assert.Equal(LizardTempleSouth, window.At(0).Number);
    }

    [Fact]
    public void Scroll_IsKeptWhenTheWindowReopens()
    {
        window.Open();
        window.ScrollBy(5);
        window.Close();

        window.Open();

        Assert.Equal(5, window.Scroll);
    }

    [Fact]
    public void Choose_TravelsToAFoundWaypointThenClosesWithThePause()
    {
        window.Open();
        window.ScrollBy(7);

        var packet = window.Choose(0, MapButton.Left, 1 << LizardTempleSouth);

        Assert.Equal(ClientPackets.OneValue(ClientCommand.Waypoints, LizardTempleSouth), packet);
        Assert.False(window.IsOpen);
        Assert.True(window.Pause.Active);
    }

    [Fact]
    public void Choose_AsksAboutAnUnknownWaypointAndStaysOpen()
    {
        window.Open();

        var packet = window.Choose(0, MapButton.Left, NothingFound);

        Assert.Equal(ClientPackets.OneValue(ClientCommand.Waypoints, LynboreTavern), packet);
        Assert.True(window.IsOpen);
        Assert.False(window.Pause.Active);
    }

    [Fact]
    public void Choose_DescribesAWaypointOnARightClick()
    {
        window.Open();
        window.ScrollBy(7);

        var packet = window.Choose(0, MapButton.Right, 1 << LizardTempleSouth);

        Assert.Equal(ClientPackets.OneValue(ClientCommand.Waypoints, LizardTempleSouth + 32), packet);
        Assert.True(window.IsOpen);
    }

    [Fact]
    public void Choose_SendsNothingForOtherButtons()
    {
        window.Open();

        Assert.Null(window.Choose(0, MapButton.None, NothingFound));
    }

    [Fact]
    public void Escape_ClosesTheWindowWithoutAPause()
    {
        state.WaypointWindow.Open();

        Assert.True(state.CloseWindowsByEscape());

        Assert.False(state.WaypointWindow.IsOpen);
        Assert.False(state.Pause.Active);
    }

    [Fact]
    public void SetSearch_ShowsOnlyFoundWaypointsWhoseNameMatches()
    {
        window.SetSearch("pent", AllFound);

        Assert.Equal(7, window.ShownCount);
        Assert.Equal(13, window.At(0).Number);
        Assert.Equal(0, window.ScrollLimit);
        Assert.False(window.HasRow(7));
    }

    [Fact]
    public void SetSearch_LeavesOutUnfoundWaypoints()
    {
        window.SetSearch("lynbore", 1 << LynboreTavern);

        Assert.Equal(1, window.ShownCount);
        Assert.Equal(LynboreTavern, window.At(0).Number);
    }

    [Fact]
    public void SetSearch_SameTextKeepsTheScroll()
    {
        window.ScrollBy(5);

        window.SetSearch("", AllFound);

        Assert.Equal(5, window.Scroll);
    }

    [Fact]
    public void Choose_AnEmptyRowSendsNothing()
    {
        window.SetSearch("tower", AllFound);

        Assert.Null(window.Choose(1, MapButton.Left, AllFound));
    }

    [Fact]
    public void SetSearch_SelectsTheTopMatch()
    {
        window.SetSearch("pent", AllFound);

        Assert.Equal(0, window.Selected);
        Assert.True(window.IsSelected(0));
    }

    [Fact]
    public void MoveSelection_WithNothingSelectedPicksTheTopRowInView()
    {
        window.ScrollBy(5);

        window.MoveSelection(1);

        Assert.Equal(5, window.Selected);
        Assert.True(window.IsSelected(0));
    }

    [Fact]
    public void MoveSelection_ScrollsToKeepTheSelectionInView()
    {
        window.MoveSelection(1);
        window.MoveSelection(WaypointWindow.VisibleRows);

        Assert.Equal(WaypointWindow.VisibleRows, window.Selected);
        Assert.Equal(1, window.Scroll);

        window.MoveSelection(-WaypointWindow.VisibleRows);

        Assert.Equal(0, window.Scroll);
    }

    [Fact]
    public void MoveSelection_StopsAtTheEndsOfTheList()
    {
        window.SetSearch("pent", AllFound);

        window.MoveSelection(-3);
        Assert.Equal(0, window.Selected);

        window.MoveSelection(20);
        Assert.Equal(6, window.Selected);
    }

    [Fact]
    public void ClearSearch_ShowsEveryWaypointWithNothingSelected()
    {
        window.SetSearch("pent", AllFound);

        window.ClearSearch(AllFound);

        Assert.Equal(Waypoints.Count, window.ShownCount);
        Assert.Null(window.Selected);
        Assert.Null(window.ChooseSelected(AllFound));
    }

    [Fact]
    public void ChooseSelected_TravelsToTheSelectedWaypoint()
    {
        window.SetSearch("tower", AllFound);

        Assert.Equal(ClientPackets.OneValue(ClientCommand.Waypoints, 23), window.ChooseSelected(AllFound));
    }

    private void Process(params TestCommand[] commands) =>
        new TickProcessor(state).Process(TestCommand.Payload(commands));
}
