namespace TheLastGate.Net;

public static class ExitPackets
{
    public static byte[] Exit() => ClientPackets.OneValue(ClientCommand.Exit, 0);
}
