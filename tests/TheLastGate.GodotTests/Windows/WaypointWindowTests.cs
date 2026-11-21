using GdUnit4;
using TheLastGate.Net;
using TheLastGate.State;
using TheLastGate.Windows;
using TheLastGate.World;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Windows;

[TestSuite]
public class WaypointWindowTests
{
    private const int LynboreTavern = 0;
    private const int LizardTempleSouth = 20;
    private const int NothingFound = 0;

    private const int NoSelection = -1;
    private const int AllFound = (1 << Waypoints.Count) - 1;

    private WaypointWindow window = new(new WindowPause());

    [BeforeTest]
    public void CloseTheWindow() => window = new WaypointWindow(new WindowPause());

    [TestCase]
    public void OpenAndClose_ShowAndHideTheWindow()
    {
        window.Open();
        AssertThat(window.IsOpen).IsTrue();

        AssertThat(window.Close()).IsTrue();
        AssertThat(window.IsOpen).IsFalse();
        AssertThat(window.Close()).IsFalse();
    }

    [TestCase(3, 3)]
    [TestCase(-1, 0)]
    [TestCase(25, Waypoints.Count - WaypointWindow.VisibleRows)]
    public void ScrollBy_StaysWithinTheList(int rows, int scroll)
    {
        window.ScrollBy(rows);

        AssertThat(window.Scroll).IsEqual(scroll);
    }

    [TestCase]
    public void At_CountsRowsFromTheScrollPosition()
    {
        window.ScrollBy(7);

        AssertThat(window.At(0).Number).IsEqual(LizardTempleSouth);
    }

    [TestCase]
    public void Scroll_IsKeptWhenTheWindowReopens()
    {
        window.Open();
        window.ScrollBy(5);
        window.Close();

        window.Open();

        AssertThat(window.Scroll).IsEqual(5);
    }

    [TestCase]
    public void Choose_TravelsToAFoundWaypointThenClosesWithThePause()
    {
        window.Open();
        window.ScrollBy(7);

        var packet = window.Choose(0, MapButton.Left, 1 << LizardTempleSouth);

        AssertThat(packet).IsEqual(ClientPackets.OneValue(ClientCommand.Waypoints, LizardTempleSouth));
        AssertThat(window.IsOpen).IsFalse();
        AssertThat(window.Pause.Active).IsTrue();
    }

    [TestCase]
    public void Choose_AsksAboutAnUnknownWaypointAndStaysOpen()
    {
        window.Open();

        var packet = window.Choose(0, MapButton.Left, NothingFound);

        AssertThat(packet).IsEqual(ClientPackets.OneValue(ClientCommand.Waypoints, LynboreTavern));
        AssertThat(window.IsOpen).IsTrue();
        AssertThat(window.Pause.Active).IsFalse();
    }

    [TestCase]
    public void Choose_DescribesAWaypointOnARightClick()
    {
        window.Open();
        window.ScrollBy(7);

        var packet = window.Choose(0, MapButton.Right, 1 << LizardTempleSouth);

        AssertThat(packet).IsEqual(ClientPackets.OneValue(ClientCommand.Waypoints, LizardTempleSouth + 32));
        AssertThat(window.IsOpen).IsTrue();
    }

    [TestCase]
    public void Choose_SendsNothingForOtherButtons()
    {
        window.Open();

        AssertThat(window.Choose(0, MapButton.None, NothingFound)).IsNull();
    }

    [TestCase]
    public void SetSearch_ShowsOnlyFoundWaypointsWhoseNameMatches()
    {
        window.SetSearch("pent", AllFound);

        AssertThat(window.ShownCount).IsEqual(7);
        AssertThat(window.At(0).Number).IsEqual(13);
        AssertThat(window.ScrollLimit).IsEqual(0);
        AssertThat(window.HasRow(7)).IsFalse();
    }

    [TestCase]
    public void SetSearch_LeavesOutUnfoundWaypoints()
    {
        window.SetSearch("lynbore", 1 << LynboreTavern);

        AssertThat(window.ShownCount).IsEqual(1);
        AssertThat(window.At(0).Number).IsEqual(LynboreTavern);
    }

    [TestCase]
    public void SetSearch_SameTextKeepsTheScroll()
    {
        window.ScrollBy(5);

        window.SetSearch("", AllFound);

        AssertThat(window.Scroll).IsEqual(5);
    }

    [TestCase]
    public void Choose_AnEmptyRowSendsNothing()
    {
        window.SetSearch("tower", AllFound);

        AssertThat(window.Choose(1, MapButton.Left, AllFound)).IsNull();
    }

    [TestCase]
    public void SetSearch_SelectsTheTopMatch()
    {
        window.SetSearch("pent", AllFound);

        AssertThat(window.Selected ?? NoSelection).IsEqual(0);
        AssertThat(window.IsSelected(0)).IsTrue();
    }

    [TestCase]
    public void MoveSelection_WithNothingSelectedPicksTheTopRowInView()
    {
        window.ScrollBy(5);

        window.MoveSelection(1);

        AssertThat(window.Selected ?? NoSelection).IsEqual(5);
        AssertThat(window.IsSelected(0)).IsTrue();
    }

    [TestCase]
    public void MoveSelection_ScrollsToKeepTheSelectionInView()
    {
        window.MoveSelection(1);
        window.MoveSelection(WaypointWindow.VisibleRows);

        AssertThat(window.Selected ?? NoSelection).IsEqual(WaypointWindow.VisibleRows);
        AssertThat(window.Scroll).IsEqual(1);

        window.MoveSelection(-WaypointWindow.VisibleRows);

        AssertThat(window.Scroll).IsEqual(0);
    }

    [TestCase]
    public void MoveSelection_StopsAtTheEndsOfTheList()
    {
        window.SetSearch("pent", AllFound);

        window.MoveSelection(-3);
        AssertThat(window.Selected ?? NoSelection).IsEqual(0);

        window.MoveSelection(20);
        AssertThat(window.Selected ?? NoSelection).IsEqual(6);
    }

    [TestCase]
    public void ClearSearch_ShowsEveryWaypointWithNothingSelected()
    {
        window.SetSearch("pent", AllFound);

        window.ClearSearch(AllFound);

        AssertThat(window.ShownCount).IsEqual(Waypoints.Count);
        AssertThat(window.Selected ?? NoSelection).IsEqual(NoSelection);
        AssertThat(window.ChooseSelected(AllFound)).IsNull();
    }

    [TestCase]
    public void ChooseSelected_TravelsToTheSelectedWaypoint()
    {
        window.SetSearch("tower", AllFound);

        AssertThat(window.ChooseSelected(AllFound)).IsEqual(ClientPackets.OneValue(ClientCommand.Waypoints, 23));
    }
}
