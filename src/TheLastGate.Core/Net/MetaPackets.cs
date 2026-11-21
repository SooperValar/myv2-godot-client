namespace TheLastGate.Net;

public static class MetaPackets
{
    public static byte[] Request() => ClientPackets.ShortAndInt(ClientCommand.Meta, 0, 0);
}
