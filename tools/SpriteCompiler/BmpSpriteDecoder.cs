using SkiaSharp;

namespace TheLastGate.Tools.SpriteCompiler;

// Decodes any BMP that Skia can read into the RGB565 pixels bmplib stores.
public static class BmpSpriteDecoder
{
    // The client treats a bmplib sprite wider or taller than this as a corrupt index.
    public const int MaxDimension = 1000;

    private const int BytesPerColour = 4;

    public static BmpDecodeResult Decode(byte[] fileBytes)
    {
        using var data = SKData.CreateCopy(fileBytes);
        using var codec = SKCodec.Create(data);
        if (codec?.EncodedFormat != SKEncodedImageFormat.Bmp)
        {
            return BmpDecodeResult.Rejected("not a BMP file");
        }

        var width = codec.Info.Width;
        var height = codec.Info.Height;
        if (width > MaxDimension || height > MaxDimension)
        {
            return BmpDecodeResult.Rejected($"size {width}x{height} is larger than {MaxDimension}x{MaxDimension}");
        }

        var colourInfo = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul);
        var result = codec.GetPixels(colourInfo, out var colours);
        if (result != SKCodecResult.Success)
        {
            return BmpDecodeResult.Rejected($"can't be read as a BMP ({result})");
        }

        return new BmpDecodeResult(width, height, ToRgb565(colours), null);
    }

    // Truncates each channel rather than rounding, which is how bmplib has always been built.
    private static ushort[] ToRgb565(byte[] colours)
    {
        var pixels = new ushort[colours.Length / BytesPerColour];
        for (var pixelIndex = 0; pixelIndex < pixels.Length; pixelIndex++)
        {
            var red = colours[pixelIndex * BytesPerColour];
            var green = colours[pixelIndex * BytesPerColour + 1];
            var blue = colours[pixelIndex * BytesPerColour + 2];
            pixels[pixelIndex] = (ushort)(((red >> 3) << 11) | ((green >> 2) << 5) | (blue >> 3));
        }

        return pixels;
    }
}
