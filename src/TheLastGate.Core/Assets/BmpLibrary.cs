using System;
using System.Buffers.Binary;
using System.IO;

namespace TheLastGate.Assets;

// bmplib.idx is a table of { int32 width, int32 height, int32 offset } entries, one per sprite
// number below BmpSlotCount, where width 0 means the sprite is absent. bmplib.dat holds raw
// RGB565 pixels, little-endian, with rows tightly packed.
public sealed class BmpLibrary : IDisposable
{
    private const int EntrySize = 3 * sizeof(int);
    private const int MaxDimension = 1000;

    private readonly ArchiveFile data;
    private readonly Entry[] entries = new Entry[SpriteNumbers.BmpSlotCount];

    public BmpLibrary(string indexPath, string dataPath)
    {
        var indexBytes = File.ReadAllBytes(indexPath);
        if (indexBytes.Length < EntrySize * entries.Length)
        {
            throw new InvalidDataException(
                $"bmplib.idx is {indexBytes.Length} bytes, not {EntrySize * entries.Length}. If it's a Git LFS pointer, run git lfs pull.");
        }

        for (var slot = 0; slot < entries.Length; slot++)
        {
            var entryBytes = indexBytes.AsSpan(slot * EntrySize, EntrySize);
            entries[slot] = new Entry(
                BinaryPrimitives.ReadInt32LittleEndian(entryBytes),
                BinaryPrimitives.ReadInt32LittleEndian(entryBytes[4..]),
                BinaryPrimitives.ReadInt32LittleEndian(entryBytes[8..]));
        }

        data = new ArchiveFile(dataPath);
    }

    public void Dispose() => data.Dispose();

    public bool Contains(int spriteNumber) =>
        spriteNumber >= 0 && spriteNumber < entries.Length
        && entries[spriteNumber].Width != 0 && entries[spriteNumber].Height != 0;

    public DecodedSprite? Read(int spriteNumber)
    {
        if (!Contains(spriteNumber))
        {
            return null;
        }

        var entry = entries[spriteNumber];
        if (entry.Width < 0 || entry.Width > MaxDimension || entry.Height < 0 || entry.Height > MaxDimension)
        {
            throw new InvalidDataException(
                $"Sprite {spriteNumber} in bmplib.idx has an invalid size {entry.Width}x{entry.Height}.");
        }

        var pixelCount = entry.Width * entry.Height;
        var sourcePixels = new byte[pixelCount * SpritePixels.Rgb565BytesPerPixel];
        data.ReadExactly(sourcePixels, entry.Offset);

        var rgba = new byte[pixelCount * SpritePixels.Rgba8BytesPerPixel];
        SpritePixels.Rgb565ToRgba8(sourcePixels, rgba);
        return new DecodedSprite(entry.Width, entry.Height, rgba);
    }

    private readonly record struct Entry(int Width, int Height, int Offset);
}
