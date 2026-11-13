using System.Buffers.Binary;

namespace TheLastGate.Tests;

// Reads archive index entries straight from the bytes, so the writer tests don't depend on the
// client's archive readers. bmplib.idx holds width, height and data offset (int32 each) per
// sprite; pnglib.idx holds one int32 offset per sprite, 0 when absent.
public static class TestArchives
{
    private const int BmpEntrySize = 3 * sizeof(int);

    public static (int Width, int Height, int Offset) BmpEntry(byte[] indexBytes, int spriteNumber)
    {
        var entry = indexBytes.AsSpan(spriteNumber * BmpEntrySize, BmpEntrySize);
        return (
            BinaryPrimitives.ReadInt32LittleEndian(entry),
            BinaryPrimitives.ReadInt32LittleEndian(entry[4..]),
            BinaryPrimitives.ReadInt32LittleEndian(entry[8..]));
    }

    public static int PngOffset(byte[] indexBytes, int spriteNumber) =>
        BinaryPrimitives.ReadInt32LittleEndian(indexBytes.AsSpan(spriteNumber * sizeof(int)));
}
