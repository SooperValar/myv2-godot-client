using System;

namespace TheLastGate.Net;

public ref struct TickCommandReader(ReadOnlySpan<byte> payload)
{
    private readonly ReadOnlySpan<byte> payload = payload;
    private int commandNumber;
    private int offset;

    public bool TryReadNext(out ReadOnlySpan<byte> command)
    {
        if (offset == payload.Length)
        {
            command = default;
            return false;
        }

        commandNumber++;
        var remaining = payload[offset..];
        int length;
        try
        {
            length = ServerCommandLength.Of(remaining);
        }
        catch (ProtocolDesyncException exception)
        {
            throw Desync(remaining[0], exception.Message);
        }

        if (length > remaining.Length)
        {
            throw Desync(remaining[0], $"It needs {length} bytes, but only {remaining.Length} are left.");
        }

        command = remaining[..length];
        offset += length;
        return true;
    }

    private readonly ProtocolDesyncException Desync(byte commandId, string detail) =>
        new($"Server command {commandNumber} (id {commandId}) at offset {offset} of {payload.Length}: {detail}");
}
