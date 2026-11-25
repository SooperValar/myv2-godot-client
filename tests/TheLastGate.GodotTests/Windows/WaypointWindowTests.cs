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
    private const int NoSelection = -1;
    private const int RowCount = 20;
    private const int HereIndex = 12;
    private const int UnfoundIndex = 3;

    private WaypointWindow window = new(new WindowPause());

    // Twenty waypoints numbered 1-20, all found but the fourth; the player stands at the thirteenth. Odd numbers are
    // in Aston, even ones in the Wolfwood.
    private static IReadOnlyList<WaypointRow> Rows() =>
        Enumerable.Range(0, RowCount)
            .Select(index => index == UnfoundIndex
                ? new WaypointRow(0, "", "", 0, false, false)
                : new WaypointRow(
                    index + 1,
                    (index % 2 == 0 ? "Aston, Place " : "Wolfwood, Place ") + (index + 1),
                    "\"A line.\"",
                    0,
                    true,
                    index == HereIndex))
            .ToList();

    [BeforeTest]
    public void ShowTheList()
    {
        window = new WaypointWindow(new WindowPause());
        window.Show(Rows());
    }

    [TestCase]
    public void ShowAndClose_OpenAndHideTheWindow()
    {
        AssertThat(window.IsOpen).IsTrue();

        AssertThat(window.Close()).IsTrue();
        AssertThat(window.IsOpen).IsFalse();
        AssertThat(window.Close()).IsFalse();
    }

    [TestCase]
    public void Show_ScrollsToTheWaypointThePlayerStandsAt()
    {
        AssertThat(window.Scroll).IsEqual(HereIndex - WaypointWindow.VisibleRows / 2);
        AssertThat(window.At(WaypointWindow.VisibleRows / 2).YouAreHere).IsTrue();
    }

    [TestCase]
    public void Show_StartsAtTheTopWhenThePlayerIsAtNone()
    {
        window.Show([new WaypointRow(1, "Aston", "", 0, true, false)]);

        AssertThat(window.Scroll).IsEqual(0);
    }

    [TestCase(3, 11)]
    [TestCase(-20, 0)]
    [TestCase(25, RowCount - WaypointWindow.VisibleRows)]
    public void ScrollBy_StaysWithinTheList(int rows, int scroll)
    {
        window.ScrollBy(rows);

        AssertThat(window.Scroll).IsEqual(scroll);
    }

    [TestCase]
    public void Choose_TravelsToAFoundWaypointThenClosesWithThePause()
    {
        window.ScrollTo(0);

        var packet = window.Choose(1, MapButton.Left);

        AssertThat(packet).IsEqual(ClientPackets.OneValue(ClientCommand.Waypoints, 2));
        AssertThat(window.IsOpen).IsFalse();
        AssertThat(window.Pause.Active).IsTrue();
    }

    [TestCase]
    public void Choose_SendsNothingForAnUnfoundWaypoint()
    {
        window.ScrollTo(0);

        AssertThat(window.Choose(UnfoundIndex, MapButton.Left)).IsNull();
        AssertThat(window.IsOpen).IsTrue();
    }

    [TestCase]
    public void Choose_SendsNothingForTheWaypointThePlayerStandsAt()
    {
        AssertThat(window.Choose(WaypointWindow.VisibleRows / 2, MapButton.Left)).IsNull();
        AssertThat(window.IsOpen).IsTrue();
    }

    [TestCase]
    public void Choose_SendsNothingForOtherButtons()
    {
        window.ScrollTo(0);

        AssertThat(window.Choose(0, MapButton.Right)).IsNull();
        AssertThat(window.Choose(0, MapButton.None)).IsNull();
    }

    [TestCase]
    public void SetSearch_ShowsOnlyFoundWaypointsWhoseNameMatches()
    {
        window.SetSearch("wolf");

        AssertThat(window.ShownCount).IsEqual(9);
        AssertThat(window.At(0).Number).IsEqual(2);
        AssertThat(window.ScrollLimit).IsEqual(1);
    }

    [TestCase]
    public void SetSearch_SelectsTheTopMatch()
    {
        window.SetSearch("place 5");

        AssertThat(window.Selected ?? NoSelection).IsEqual(0);
        AssertThat(window.ChooseSelected()).IsEqual(WaypointPackets.Travel(5));
    }

    [TestCase]
    public void ClearSearch_ShowsEveryWaypointWithNothingSelected()
    {
        window.SetSearch("wolf");

        window.ClearSearch();

        AssertThat(window.ShownCount).IsEqual(RowCount);
        AssertThat(window.Selected ?? NoSelection).IsEqual(NoSelection);
        AssertThat(window.ChooseSelected()).IsNull();
    }

    [TestCase]
    public void MoveSelection_ScrollsToKeepTheSelectionInView()
    {
        window.ScrollTo(0);
        window.MoveSelection(1);
        window.MoveSelection(WaypointWindow.VisibleRows);

        AssertThat(window.Selected ?? NoSelection).IsEqual(WaypointWindow.VisibleRows);
        AssertThat(window.Scroll).IsEqual(1);
    }

    [TestCase]
    public void Hint_CountsTheFoundWaypointsWhenNothingIsPointedAt()
    {
        AssertThat(window.Hint(null)).IsEqual("Waypoints found: 19 of 20. Type to search.");
    }

    [TestCase]
    public void Hint_SaysWhatAClickWouldDo()
    {
        window.ScrollTo(0);

        AssertThat(window.Hint(0)).IsEqual("Travel to Aston, Place 1.");
        AssertThat(window.Hint(UnfoundIndex)).IsEqual("You have not found this waypoint yet.");
        window.ScrollTo(HereIndex);
        AssertThat(window.Hint(0)).IsEqual("You are here.");
    }
}
