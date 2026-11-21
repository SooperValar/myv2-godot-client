namespace TheLastGate.Net;

public static class WaypointPackets
{
    private const int DescribeOffset = 32;

    public static byte[] Travel(int waypoint) => ClientPackets.OneValue(ClientCommand.Waypoints, waypoint);

    public static byte[] Describe(int waypoint) =>
        ClientPackets.OneValue(ClientCommand.Waypoints, waypoint + DescribeOffset);
}
