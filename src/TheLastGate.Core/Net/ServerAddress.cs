namespace TheLastGate.Net;

public sealed record ServerAddress(string Host, string Proxy, int Port)
{
    public static ServerAddress Live { get; } = new("tlg.abyss.codes", "thelastgate.ddns.net", Protocol.DefaultPort);
}
