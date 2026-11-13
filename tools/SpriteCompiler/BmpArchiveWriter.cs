using System.Buffers.Binary;
using TheLastGate.Assets;

namespace TheLastGate.Tools.SpriteCompiler;

// Writes bmplib: the pixel data streams out as sprites are added, and the index of width, height
// and data offset per sprite is written on Finish.
public sealed class BmpArchiveWriter(Stream indexStream, Stream dataStream)
{
    private const int EntrySize = 3 * sizeof(int);

    private readonly byte[] indexBytes = new byte[EntrySize * SpriteNumbers.BmpSlotCount];
    private int lastSpriteNumber = -1;

    public long DataLength { get; private set; }

    public void Add(int spriteNumber, int width, int height, ReadOnlySpan<ushort> pixels)
    {
        if (spriteNumber <= lastSpriteNumber || spriteNumber >= SpriteNumbers.BmpSlotCount)
        {
            throw new ArgumentException(
                $"Sprite {spriteNumber} must be above {lastSpriteNumber} and below {SpriteNumbers.BmpSlotCount}.",
                nameof(spriteNumber));
        }

        if (pixels.Length != width * height)
        {
            throw new ArgumentException(
                $"Sprite {spriteNumber} has {pixels.Length} pixels, not {width}x{height}.",
                nameof(pixels));
        }

        var pixelBytes = (long)pixels.Length * sizeof(ushort);
        if (DataLength + pixelBytes > int.MaxValue)
        {
            throw new SpriteCompileException($"bmplib.dat would pass {int.MaxValue} bytes at sprite {spriteNumber}.");
        }

        var entry = indexBytes.AsSpan(spriteNumber * EntrySize, EntrySize);
        BinaryPrimitives.WriteInt32LittleEndian(entry, width);
        BinaryPrimitives.WriteInt32LittleEndian(entry[4..], height);
        BinaryPrimitives.WriteInt32LittleEndian(entry[8..], (int)DataLength);

        var pixelData = new byte[pixelBytes];
        for (var pixelIndex = 0; pixelIndex < pixels.Length; pixelIndex++)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(pixelData.AsSpan(pixelIndex * sizeof(ushort)), pixels[pixelIndex]);
        }

        dataStream.Write(pixelData);
        DataLength += pixelBytes;
        lastSpriteNumber = spriteNumber;
    }

    public void Finish() => indexStream.Write(indexBytes);
}
