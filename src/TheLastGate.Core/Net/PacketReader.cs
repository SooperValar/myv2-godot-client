using System;
using System.Buffers.Binary;

namespace TheLastGate.Net;

public ref struct PacketReader(ReadOnlySpan<byte> bytes)
{
    private readonly ReadOnlySpan<byte> bytes = bytes;

    public int Position { get; private set; }
    public readonly int Remaining => bytes.Length - Position;

    public byte ReadByte() => Take(1)[0];
    public ushort ReadUInt16() => BinaryPrimitives.ReadUInt16LittleEndian(Take(2));
    public short ReadInt16() => BinaryPrimitives.ReadInt16LittleEndian(Take(2));
    public uint ReadUInt32() => BinaryPrimitives.ReadUInt32LittleEndian(Take(4));
    public ReadOnlySpan<byte> ReadBytes(int count) => Take(count);
    public void Skip(int count) => Take(count);

    private ReadOnlySpan<byte> Take(int count)
    {
        if (count < 0 || count > Remaining)
        {
            throw new ProtocolDesyncException(
                $"Needed {count} bytes at offset {Position}, but only {Remaining} are left.");
        }

        var taken = bytes.Slice(Position, count);
        Position += count;
        return taken;
    }
}
