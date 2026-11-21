namespace TheLastGate.Net;

public static class SmithPackets
{
    private const int TakeBackOffset = 16;
    private const int CheckOffset = 8;

    public static byte[] Place(int smithNumber, int slot) => Smith(smithNumber, slot);

    public static byte[] TakeBack(int smithNumber, int slot) => Smith(smithNumber, slot + TakeBackOffset);

    public static byte[] Press(int smithNumber, int button) => Smith(smithNumber, button);

    public static byte[] Check(int smithNumber, int slotOrButton) => Smith(smithNumber, slotOrButton + CheckOffset);

    private static byte[] Smith(int smithNumber, int value) =>
        ClientPackets.ShortAndInt(ClientCommand.Smith, smithNumber, value);
}
