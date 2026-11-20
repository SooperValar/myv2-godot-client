using System;

namespace TheLastGate.Map;

public static class SpriteColours
{
    private const int BytesPerPixel = 4;

    // Magenta is the sprites' see-through colour, so it and fully transparent pixels don't count.
    public static uint Average(ReadOnlySpan<byte> rgbaPixels)
    {
        long red = 0;
        long green = 0;
        long blue = 0;
        var counted = 0;
        for (var offset = 0; offset + BytesPerPixel <= rgbaPixels.Length; offset += BytesPerPixel)
        {
            var pixel = rgbaPixels.Slice(offset, BytesPerPixel);
            if (pixel[3] == 0 || (pixel[0] == 255 && pixel[1] == 0 && pixel[2] == 255))
            {
                continue;
            }

            red += pixel[0];
            green += pixel[1];
            blue += pixel[2];
            counted++;
        }

        return counted == 0
            ? 0
            : MinimapColours.FromRgb((byte)(red / counted), (byte)(green / counted), (byte)(blue / counted));
    }
}
