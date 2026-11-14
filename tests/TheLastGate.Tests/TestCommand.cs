using System.Buffers.Binary;
using System.Text;
using TheLastGate.Net;

namespace TheLastGate.Tests;

public sealed class TestCommand
{
    private readonly List<byte> bytes;

    private TestCommand(byte commandId) => bytes = [commandId];

    public static TestCommand Of(ServerCommand commandId) => new((byte)commandId);

    public static TestCommand Of(byte commandId) => new(commandId);

    public TestCommand Byte(byte value)
    {
        bytes.Add(value);
        return this;
    }

    public TestCommand Bytes(params byte[] values)
    {
        bytes.AddRange(values);
        return this;
    }

    public TestCommand UInt16(ushort value)
    {
        var field = new byte[sizeof(ushort)];
        BinaryPrimitives.WriteUInt16LittleEndian(field, value);
        return Bytes(field);
    }

    public TestCommand UInt32(uint value)
    {
        var field = new byte[sizeof(uint)];
        BinaryPrimitives.WriteUInt32LittleEndian(field, value);
        return Bytes(field);
    }

    public TestCommand Text(string text, int fieldLength)
    {
        var field = new byte[fieldLength];
        Encoding.Latin1.GetBytes(text).AsSpan(0, Math.Min(text.Length, fieldLength)).CopyTo(field);
        return Bytes(field);
    }

    public byte[] ToArray()
    {
        var padded = new byte[64];
        bytes.CopyTo(padded);
        var length = ServerCommandLength.Of(padded);
        if (bytes.Count > length)
        {
            throw new InvalidOperationException($"Added {bytes.Count} bytes to a {length}-byte command.");
        }

        return padded[..length];
    }

    public static byte[] Payload(params TestCommand[] commands) =>
        commands.SelectMany(command => command.ToArray()).ToArray();
}
