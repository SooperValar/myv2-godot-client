namespace TheLastGate.Net;

public static class DepotPackets
{
    public const int Pages = 8;
    public const int PageSlots = 64;

    private const int DescribeOffset = Pages * PageSlots;

    public static byte[] Take(int depotNumber, int slot) => Depot(depotNumber, slot);

    public static byte[] Describe(int depotNumber, int slot) => Depot(depotNumber, slot + DescribeOffset);

    private static byte[] Depot(int depotNumber, int value) =>
        ClientPackets.ShortAndInt(ClientCommand.Shop, depotNumber, value);
}
