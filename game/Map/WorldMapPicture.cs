using System;
using TheLastGate.State;

namespace TheLastGate.Map;

// The explored map as one picture for the world map window: map y across, map x down. Unexplored tiles are black.
public static class WorldMapPicture
{
    public const int Width = WorldMap.WorldTiles;
    public const int Height = WorldMap.WorldTiles;

    private const int BytesPerPixel = 4;

    public static void Render(ExploredMap map, Span<byte> rgba)
    {
        for (var tileX = 0; tileX < Height; tileX++)
        {
            for (var tileY = 0; tileY < Width; tileY++)
            {
                var colour = map.ColourAt(tileX, tileY);
                var pixel = rgba.Slice((tileX * Width + tileY) * BytesPerPixel, BytesPerPixel);
                pixel[0] = (byte)(colour >> 16);
                pixel[1] = (byte)(colour >> 8);
                pixel[2] = (byte)colour;
                pixel[3] = 255;
            }
        }
    }
}
