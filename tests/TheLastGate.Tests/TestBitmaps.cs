using System.Buffers.Binary;

namespace TheLastGate.Tests;

// Builds BMP files byte by byte the way image editors save them: bottom-up rows padded to four
// bytes, with 24-bit pixels stored blue, green, red and 8-bit pixels as indexes into a palette.
// Skia can read BMPs but not write them, so the tests can't generate these with it.
public static class TestBitmaps
{
    private const int FileHeaderSize = 14;
    private const int InfoHeaderSize = 40;
    private const int PaletteEntrySize = 4;

    public static byte[] Create(
        int width,
        int height,
        Func<int, int, (byte Red, byte Green, byte Blue)> pixelAt,
        short bitsPerPixel = 24)
    {
        var palette = bitsPerPixel == 8 ? BuildPalette(width, height, pixelAt) : [];
        var bytesPerPixel = bitsPerPixel / 8;
        var rowStride = (width * bytesPerPixel + 3) & ~3;
        var pixelOffset = FileHeaderSize + InfoHeaderSize + palette.Count * PaletteEntrySize;
        var fileBytes = new byte[pixelOffset + rowStride * height];
        var span = fileBytes.AsSpan();

        span[0] = (byte)'B';
        span[1] = (byte)'M';
        BinaryPrimitives.WriteInt32LittleEndian(span[2..], fileBytes.Length);
        BinaryPrimitives.WriteInt32LittleEndian(span[10..], pixelOffset);
        BinaryPrimitives.WriteInt32LittleEndian(span[14..], InfoHeaderSize);
        BinaryPrimitives.WriteInt32LittleEndian(span[18..], width);
        BinaryPrimitives.WriteInt32LittleEndian(span[22..], height);
        BinaryPrimitives.WriteInt16LittleEndian(span[26..], 1);
        BinaryPrimitives.WriteInt16LittleEndian(span[28..], bitsPerPixel);
        BinaryPrimitives.WriteInt32LittleEndian(span[46..], palette.Count);

        for (var paletteIndex = 0; paletteIndex < palette.Count; paletteIndex++)
        {
            var (red, green, blue) = palette[paletteIndex];
            var entryStart = FileHeaderSize + InfoHeaderSize + paletteIndex * PaletteEntrySize;
            fileBytes[entryStart] = blue;
            fileBytes[entryStart + 1] = green;
            fileBytes[entryStart + 2] = red;
        }

        for (var row = 0; row < height; row++)
        {
            var rowStart = pixelOffset + (height - 1 - row) * rowStride;
            for (var column = 0; column < width; column++)
            {
                var colour = pixelAt(column, row);
                var pixelStart = rowStart + column * bytesPerPixel;
                if (bitsPerPixel == 8)
                {
                    fileBytes[pixelStart] = (byte)palette.IndexOf(colour);
                    continue;
                }

                fileBytes[pixelStart] = colour.Blue;
                fileBytes[pixelStart + 1] = colour.Green;
                fileBytes[pixelStart + 2] = colour.Red;
            }
        }

        return fileBytes;
    }

    private static List<(byte Red, byte Green, byte Blue)> BuildPalette(
        int width,
        int height,
        Func<int, int, (byte Red, byte Green, byte Blue)> pixelAt)
    {
        var palette = new List<(byte Red, byte Green, byte Blue)>();
        for (var row = 0; row < height; row++)
        {
            for (var column = 0; column < width; column++)
            {
                var colour = pixelAt(column, row);
                if (!palette.Contains(colour))
                {
                    palette.Add(colour);
                }
            }
        }

        return palette;
    }
}
