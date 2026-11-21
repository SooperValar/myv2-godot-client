using TheLastGate.Net;

namespace TheLastGate.Tests;

public class WaypointPacketsTests
{
    [Fact]
    public void Travel_SendsTheWaypoint()
    {
        byte[] expected = [35, 7, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

        Assert.Equal(expected, WaypointPackets.Travel(7));
    }

    [Fact]
    public void Describe_SendsTheWaypointPast32()
    {
        byte[] expected = [35, 39, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

        Assert.Equal(expected, WaypointPackets.Describe(7));
    }
}
