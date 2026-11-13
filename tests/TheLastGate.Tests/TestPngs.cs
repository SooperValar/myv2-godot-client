using SkiaSharp;

namespace TheLastGate.Tests;

public static class TestPngs
{
    // A solid-colour PNG; different shades give different file contents.
    public static byte[] Create(int width, int height, byte shade)
    {
        using var bitmap = new SKBitmap(width, height);
        bitmap.Erase(new SKColor(shade, shade, shade));
        using var data = bitmap.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }
}
