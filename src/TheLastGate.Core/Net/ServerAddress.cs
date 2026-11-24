namespace TheLastGate.Net;

public sealed record ServerAddress(string Host, string Proxy, int Port)
{
    // The hosting PC can't reach its own public address through the router, so it falls back to itself.
    public static ServerAddress Live { get; } = new("myv2astonia.duckdns.org", "127.0.0.1", Protocol.DefaultPort);
}
