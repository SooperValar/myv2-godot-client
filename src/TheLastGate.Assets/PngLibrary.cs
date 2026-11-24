using System;
using System.Buffers.Binary;
using System.IO;
using System.Linq;

namespace TheLastGate.Assets;

// pnglib.idx is an int32 offset into pnglib.dat per sprite number, where 0 means the sprite is
// absent. pnglib.dat is "SCAR" followed by PNG files stored back to back with no length prefix,
// so each PNG runs from its offset to the next stored PNG's offset, or to the end of the file.
// An index shorter than MaxSprite entries, like the MyV2 client's, leaves the sprites past its end absent.
public sealed class PngLibrary : IDisposable
{
    private const int MagicSize = 4;

    private readonly ArchiveFile data;
    private readonly int[] lengths = new int[SpriteNumbers.MaxSprite];
    private readonly int[] offsets = new int[SpriteNumbers.MaxSprite];

    public PngLibrary(string indexPath, string dataPath)
    {
        var indexBytes = File.ReadAllBytes(indexPath);
        if (LfsPointer.IsPointer(indexBytes))
        {
            throw new InvalidDataException(LfsPointer.PulledMessage(Path.GetFileName(indexPath)));
        }

        var indexedSlots = Math.Min(offsets.Length, indexBytes.Length / sizeof(int));
        data = new ArchiveFile(dataPath);
        try
        {
            for (var slot = 0; slot < indexedSlots; slot++)
            {
                offsets[slot] = BinaryPrimitives.ReadInt32LittleEndian(indexBytes.AsSpan(slot * sizeof(int)));
            }

            MeasurePngs();
        }
        catch
        {
            data.Dispose();
            throw;
        }
    }

    public void Dispose() => data.Dispose();

    public bool Contains(int spriteNumber) =>
        spriteNumber >= 0 && spriteNumber < offsets.Length && offsets[spriteNumber] != 0;

    // Returns the complete PNG file for the sprite, or null if the archive doesn't have it.
    public byte[]? ReadPng(int spriteNumber)
    {
        if (!Contains(spriteNumber))
        {
            return null;
        }

        var pngBytes = new byte[lengths[spriteNumber]];
        data.ReadExactly(pngBytes, offsets[spriteNumber]);
        return pngBytes;
    }

    private void MeasurePngs()
    {
        var startsInFileOrder = offsets.Where(offset => offset != 0).Distinct().Order().ToArray();
        for (var slot = 0; slot < offsets.Length; slot++)
        {
            var start = offsets[slot];
            if (start == 0)
            {
                continue;
            }

            if (start < MagicSize || start >= data.Length)
            {
                throw new InvalidDataException(
                    $"Sprite {slot} in pnglib.idx points outside pnglib.dat (offset {start}).");
            }

            var position = Array.BinarySearch(startsInFileOrder, start);
            var end = position + 1 < startsInFileOrder.Length ? startsInFileOrder[position + 1] : data.Length;
            lengths[slot] = (int)(end - start);
        }
    }
}
