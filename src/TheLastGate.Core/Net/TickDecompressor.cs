using System;

namespace TheLastGate.Net;

public sealed class TickDecompressor : IDisposable
{
    public const int MaxPayloadSize = 65536;

    public void Dispose()
    {
    }

    public int Decompress(ReadOnlyMemory<byte> compressedFrame, Span<byte> output) =>
        throw new NotImplementedException();
}
