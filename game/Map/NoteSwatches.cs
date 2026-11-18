using System.Collections.Generic;
using Godot;

namespace TheLastGate.Map;

// Small squares of a note colour, for icons in the category list and the colour menus. A dimmed swatch is the one a
// hidden category shows.
public static class NoteSwatches
{
    private const float DimmedAlpha = 0.35f;

    private static readonly Dictionary<(uint Rgb, int Size, bool Dimmed), ImageTexture> Made = [];
    private static readonly Dictionary<int, ImageTexture> Blanks = [];

    public static ImageTexture Of(uint rgb, int size, bool dimmed = false)
    {
        if (Made.TryGetValue((rgb, size, dimmed), out var made))
        {
            return made;
        }

        var colour = Color.Color8((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
        var swatch = Filled(size, dimmed ? colour with { A = DimmedAlpha } : colour);
        Made[(rgb, size, dimmed)] = swatch;
        return swatch;
    }

    // Keeps a row's text in line with the rows that have a swatch.
    public static ImageTexture Blank(int size)
    {
        if (!Blanks.TryGetValue(size, out var blank))
        {
            blank = Filled(size, Colors.Transparent);
            Blanks[size] = blank;
        }

        return blank;
    }

    private static ImageTexture Filled(int size, Color colour)
    {
        var image = Image.CreateEmpty(size, size, false, Image.Format.Rgba8);
        image.Fill(colour);
        return ImageTexture.CreateFromImage(image);
    }
}
