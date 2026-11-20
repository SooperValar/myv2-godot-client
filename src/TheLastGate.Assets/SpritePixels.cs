using System;
using System.Buffers.Binary;

namespace TheLastGate.Assets;

public static class SpritePixels
{
    public const int Rgb565BytesPerPixel = 2;
    public const int Rgba8BytesPerPixel = 4;

    private const ushort MagentaRgb565 = 0xF81F;

    // Channels are widened by repeating their high bits into the low bits, so 0x1F becomes 0xFF
    // rather than 0xF8. This matches how SDL expands 565 surfaces. Magenta becomes transparent.
    public static void Rgb565ToRgba8(ReadOnlySpan<byte> source, Span<byte> destination)
    {
        var pixelCount = source.Length / Rgb565BytesPerPixel;
        if (destination.Length < pixelCount * Rgba8BytesPerPixel)
        {
            throw new ArgumentException("Destination is too small for the source pixels.", nameof(destination));
        }

        for (var pixelIndex = 0; pixelIndex < pixelCount; pixelIndex++)
        {
            var pixel = BinaryPrimitives.ReadUInt16LittleEndian(source[(pixelIndex * Rgb565BytesPerPixel)..]);
            var red = pixel >> 11;
            var green = (pixel >> 5) & 0x3F;
            var blue = pixel & 0x1F;

            var target = destination.Slice(pixelIndex * Rgba8BytesPerPixel, Rgba8BytesPerPixel);
            target[0] = (byte)((red << 3) | (red >> 2));
            target[1] = (byte)((green << 2) | (green >> 4));
            target[2] = (byte)((blue << 3) | (blue >> 2));
            target[3] = pixel == MagentaRgb565 ? (byte)0 : (byte)255;
        }
    }

    // Only fully opaque magenta becomes transparent. Images without an alpha channel decode as
    // opaque, so for them this is an RGB-only match.
    public static void ApplyMagentaKey(Span<byte> rgba)
    {
        for (var offset = 0; offset + Rgba8BytesPerPixel <= rgba.Length; offset += Rgba8BytesPerPixel)
        {
            if (rgba[offset] == 255 && rgba[offset + 1] == 0 && rgba[offset + 2] == 255 && rgba[offset + 3] == 255)
            {
                rgba[offset + 3] = 0;
            }
        }
    }
}
