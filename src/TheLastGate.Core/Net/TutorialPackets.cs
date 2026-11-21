namespace TheLastGate.Net;

public static class TutorialPackets
{
    private const int SkipValue = 16;

    public static byte[] Seen(int tutorial) => ClientPackets.OneValue(ClientCommand.MessageOfTheDay, tutorial);

    public static byte[] SkipTutorials() => Seen(SkipValue);
}
