using System;
using System.Buffers.Binary;

namespace TheLastGate.Net;

public readonly record struct UniqueId(uint Low, uint High)
{
    public const int Size = 8;

    public static UniqueId? Parse(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < Size)
        {
            return null;
        }

        return new UniqueId(
            BinaryPrimitives.ReadUInt32LittleEndian(bytes),
            BinaryPrimitives.ReadUInt32LittleEndian(bytes[sizeof(uint)..]));
    }

    // A zero id asks the server to issue a new one, so neither half is ever zero.
    public static UniqueId CreateRandom(Random random) => new(NonZeroHalf(random), NonZeroHalf(random));

    public byte[] ToBytes()
    {
        var bytes = new byte[Size];
        BinaryPrimitives.WriteUInt32LittleEndian(bytes, Low);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(sizeof(uint)), High);
        return bytes;
    }

    private static uint NonZeroHalf(Random random) => (uint)random.NextInt64(1, 1L << 32);
}
