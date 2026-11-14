using System.Net;
using System.Net.Sockets;
using TheLastGate.Net;

namespace TheLastGate.Tests;

public sealed class LoopbackServer : IDisposable
{
    private const int CompressedFlag = 0x8000;
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    private readonly TestTickCompressor compressor = new();
    private readonly TcpListener listener = new(IPAddress.Loopback, 0);
    private TcpClient? client;

    public LoopbackServer() => listener.Start();

    public int Port => ((IPEndPoint)listener.LocalEndpoint).Port;

    private NetworkStream Stream =>
        client?.GetStream() ?? throw new InvalidOperationException("No client has connected yet.");

    public void Dispose()
    {
        client?.Dispose();
        listener.Stop();
        compressor.Dispose();
    }

    public async Task AcceptAsync()
    {
        using var timeout = new CancellationTokenSource(Timeout);
        client = await listener.AcceptTcpClientAsync(timeout.Token);
    }

    public Task SendTickAsync(byte[] payload, bool compressed)
    {
        var body = compressed ? compressor.Compress(payload) : payload;
        var header = (body.Length + 2) | (compressed ? CompressedFlag : 0);
        return SendRawAsync([(byte)header, (byte)(header >> 8), .. body]);
    }

    public async Task SendRawAsync(byte[] bytes)
    {
        using var timeout = new CancellationTokenSource(Timeout);
        await Stream.WriteAsync(bytes, timeout.Token);
    }

    public async Task<byte[]> ReadPacketAsync()
    {
        var packet = new byte[Protocol.PacketSize];
        using var timeout = new CancellationTokenSource(Timeout);
        await Stream.ReadExactlyAsync(packet, timeout.Token);
        return packet;
    }

    public void CloseConnection() => client?.Dispose();
}
