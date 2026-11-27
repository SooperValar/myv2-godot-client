using TheLastGate.State;

namespace TheLastGate.Net;

public static class HelpPackets
{
    public static byte[] For(HelpPageNumber page, int argument) =>
        ClientPackets.ShortAndInt(ClientCommand.Help, (int)page, argument);
}
