using System;
using System.Diagnostics;

namespace TheLastGate.Map;

public enum MinimapButton
{
    ZoomIn,
    ZoomOut,
    Map,
}

// The HUD's 128x128 map around the player: map x runs down the screen and map y across, each tile a block of
// magnification x magnification pixels.
public sealed class MinimapView
{
    public const int Pixels = 128;

    // Only the top half of the stored map is ever part of the world.
    private const int WorldRows = 1024;
    private const int SmallestMagnification = 1;
    private const int LargestMagnification = 4;
    private const int BytesPerPixel = 4;
    private const uint White = 0xFFFFFF;

    public int Magnification { get; private set; } = 2;

    public static string Help(MinimapButton button) => button switch
    {
        MinimapButton.ZoomIn => "Increase minimap magnification.",
        MinimapButton.ZoomOut => "Decrease minimap magnification.",
        MinimapButton.Map => "Open the world map.",
        _ => throw new UnreachableException(),
    };

    public void ZoomIn(bool toTheEnd) =>
        Magnification = toTheEnd ? LargestMagnification : Math.Min(Magnification + 1, LargestMagnification);

    public void ZoomOut(bool toTheEnd) =>
        Magnification = toTheEnd ? SmallestMagnification : Math.Max(Magnification - 1, SmallestMagnification);

    public void Scroll(int notches) =>
        Magnification = Math.Clamp(Magnification + notches, SmallestMagnification, LargestMagnification);

    // The view spans ceil(128 / magnification) tiles, so it can reach the very last row and column; the current client
    // stopped one short of them.
    public (int X, int Y) Origin(int playerX, int playerY)
    {
        var tilesAcross = (Pixels + Magnification - 1) / Magnification;
        var halfway = Pixels / 2 / Magnification;
        return (
            Math.Clamp(playerX - halfway, 0, WorldRows - tilesAcross),
            Math.Clamp(playerY - halfway, 0, ExploredMap.Size - tilesAcross));
    }

    // The player is drawn white on top, never stored into the map.
    public void Render(ExploredMap map, int playerX, int playerY, Span<byte> rgba)
    {
        var (originX, originY) = Origin(playerX, playerY);
        for (var row = 0; row < Pixels; row++)
        {
            var tileX = originX + row / Magnification;
            for (var column = 0; column < Pixels; column++)
            {
                var tileY = originY + column / Magnification;
                var colour = tileX == playerX && tileY == playerY ? White : map.ColourAt(tileX, tileY);
                var pixel = rgba.Slice((row * Pixels + column) * BytesPerPixel, BytesPerPixel);
                pixel[0] = (byte)(colour >> 16);
                pixel[1] = (byte)(colour >> 8);
                pixel[2] = (byte)colour;
                pixel[3] = 255;
            }
        }
    }
}
