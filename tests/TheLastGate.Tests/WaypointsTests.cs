using TheLastGate.State;

namespace TheLastGate.Tests;

public class WaypointsTests
{
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
}
