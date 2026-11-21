using TheLastGate.State;

namespace TheLastGate.Net;

public static class ModePackets
{
    public static byte[] For(SpeedMode mode) => ClientPackets.ShortAndInt(ClientCommand.Mode, (int)mode, 0);
}
