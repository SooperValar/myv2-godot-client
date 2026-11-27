namespace TheLastGate.Net;

public static class Protocol
{
    // 3.0.0 and up tells the MyV2 server it may send the messages only this client reads, such as the zone name.
    public const uint NetworkingVersion = 0x030100;
    public const int DefaultPort = 5555;
    public const int PacketSize = 16;

    // Versions pack major, minor and patch into the low three bytes.
    public static string VersionText(uint version) =>
        $"{version >> 16}.{(version >> 8) & 255:00}.{version & 255:00}";
}
