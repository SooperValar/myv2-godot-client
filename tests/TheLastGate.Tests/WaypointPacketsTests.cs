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
}
