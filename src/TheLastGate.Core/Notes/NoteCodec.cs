using System;
using System.Collections.Generic;

namespace TheLastGate.Notes;

public enum NoteDecodeResult
{
    Decoded,
    Damaged,
    TooLarge,
}

public static class NoteCodec
{
    public static void WriteVarint(List<byte> bytes, uint value) => throw new NotImplementedException();

    public static bool TryReadVarint(ReadOnlySpan<byte> bytes, ref int offset, out uint value) =>
        throw new NotImplementedException();

    public static void EncodeCategory(NoteCategory category, List<byte> bytes) => throw new NotImplementedException();

    public static NoteDecodeResult DecodeCategory(
        ReadOnlySpan<byte> bytes,
        ref int offset,
        out NoteCategory? category) =>
        throw new NotImplementedException();

    public static bool EncodeTree(NoteSet set, int rootId, List<byte> bytes) => throw new NotImplementedException();

    public static NoteDecodeResult DecodeTree(
        ReadOnlySpan<byte> bytes,
        ref int offset,
        bool singleRoot,
        out NoteSet? tree) =>
        throw new NotImplementedException();
}
