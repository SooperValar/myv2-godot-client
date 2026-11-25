namespace TheLastGate.Net;

public static class WaypointPackets
{
    public static byte[] Travel(int waypoint) => ClientPackets.OneValue(ClientCommand.Waypoints, waypoint);
}
