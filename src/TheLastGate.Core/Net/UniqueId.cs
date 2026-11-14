using System;

namespace TheLastGate.Net;

public readonly record struct UniqueId(uint Low, uint High)
{
    public const int Size = 8;

    public static UniqueId? Parse(ReadOnlySpan<byte> bytes) => throw new NotImplementedException();

    public static UniqueId CreateRandom(Random random) => throw new NotImplementedException();

    public byte[] ToBytes() => throw new NotImplementedException();
}
