using System.IO.Compression;

namespace TheLastGate.Tests;

public sealed class TestTickCompressor : IDisposable
{
    private readonly MemoryStream compressed = new();
    private readonly ZLibStream zlib;

    public TestTickCompressor() => zlib = new ZLibStream(compressed, CompressionLevel.Optimal, true);

    public void Dispose() => zlib.Dispose();

    // Flush ends the data written so far with a zlib sync flush, as the server does after each tick.
    public byte[] Compress(byte[] tick)
    {
        var frameStart = (int)compressed.Length;
        zlib.Write(tick);
        zlib.Flush();
        return compressed.ToArray()[frameStart..];
    }
}
