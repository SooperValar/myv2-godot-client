using System;
using System.Buffers.Binary;

namespace TheLastGate.Characters;

// { uint32 usnr, uint32 pass1, uint32 pass2, char name[40], int32 race }, little-endian.
public sealed record LoginKey(uint UserNumber, uint Pass1, uint Pass2, string Name, int Race)
{
    public const int Size = 56;

    private const int NameOffset = 12;
    private const int NameLength = 40;
    private const int RaceOffset = NameOffset + NameLength;

    public static LoginKey Parse(ReadOnlySpan<byte> keyBytes) =>
        new(
            BinaryPrimitives.ReadUInt32LittleEndian(keyBytes),
            BinaryPrimitives.ReadUInt32LittleEndian(keyBytes[4..]),
            BinaryPrimitives.ReadUInt32LittleEndian(keyBytes[8..]),
            NulTerminatedText.Read(keyBytes.Slice(NameOffset, NameLength)),
            BinaryPrimitives.ReadInt32LittleEndian(keyBytes[RaceOffset..]));

    public void Write(Span<byte> keyBytes)
    {
        BinaryPrimitives.WriteUInt32LittleEndian(keyBytes, UserNumber);
        BinaryPrimitives.WriteUInt32LittleEndian(keyBytes[4..], Pass1);
        BinaryPrimitives.WriteUInt32LittleEndian(keyBytes[8..], Pass2);
        NulTerminatedText.Write(keyBytes.Slice(NameOffset, NameLength), Name);
        BinaryPrimitives.WriteInt32LittleEndian(keyBytes[RaceOffset..], Race);
    }
}
