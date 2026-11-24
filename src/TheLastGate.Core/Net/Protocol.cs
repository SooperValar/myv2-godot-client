namespace TheLastGate.Net;

public static class Protocol
{
    public const uint NetworkingVersion = 0x020F09;
    public const int DefaultPort = 5555;
    public const int PacketSize = 16;

    // Versions pack major, minor and patch into the low three bytes.
    public static string VersionText(uint version) =>
        $"{version >> 16}.{(version >> 8) & 255:00}.{version & 255:00}";
}
