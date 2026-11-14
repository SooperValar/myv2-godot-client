using System.Buffers.Binary;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TheLastGate.Net;

public sealed class TickFrameReader(Stream stream)
{
    private const int HeaderSize = 2;
    private const int LengthMask = 0x7FFF;
    private const int CompressedFlag = 0x8000;

    private readonly byte[] header = new byte[HeaderSize];

    public async ValueTask<TickFrame?> ReadAsync(CancellationToken cancellationToken)
    {
        var headerBytesRead = await stream.ReadAtLeastAsync(header, HeaderSize, false, cancellationToken);
        if (headerBytesRead == 0)
        {
            return null;
        }

        if (headerBytesRead < HeaderSize)
        {
            throw new EndOfStreamException("The server's stream ended inside a tick frame header.");
        }

        var headerValue = BinaryPrimitives.ReadUInt16LittleEndian(header);
        var frameLength = headerValue & LengthMask;
        if (frameLength < HeaderSize)
        {
            throw new ProtocolDesyncException(
                $"A tick frame's length is {frameLength}, shorter than its {HeaderSize}-byte header.");
        }

        var body = new byte[frameLength - HeaderSize];
        await stream.ReadExactlyAsync(body, cancellationToken);
        return new TickFrame((headerValue & CompressedFlag) != 0, body);
    }
}
