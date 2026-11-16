using System.Collections.Generic;
using Godot;

namespace TheLastGate.Assets;

// Builds Godot bitmap fonts from the current client's font sheets: one sheet per colour, 96 letters of 6x9 pixels from
// the second pixel row, for the characters from space to 127. Grey has no sheet, so it is the white sheet tinted.
public sealed class ClassicFonts(SpriteLoader sprites) : IFontSet
{
    private const int FirstFontSprite = 18100;
    private const int GlyphWidth = 6;
    private const int GlyphHeight = 9;
    private const int GlyphTop = 1;
    private const int FirstCharacter = 32;
    private const int LastCharacter = 127;

    private const int FontSize = 9;

    private static readonly Color Grey = Color.Color8(72, 81, 81);

    private readonly Dictionary<TextColour, GameFont> fonts = [];

    public bool CentresLikeTheCurrentClient => true;

    public GameFont For(TextColour colour)
    {
        if (fonts.TryGetValue(colour, out var known))
        {
            return known;
        }

        if (colour == TextColour.Grey)
        {
            return fonts[colour] = For(TextColour.White) with { Tint = Grey };
        }

        var built = new FontFile
        {
            FixedSize = FontSize,
            Antialiasing = TextServer.FontAntialiasing.None,
            GenerateMipmaps = false,
            Hinting = TextServer.Hinting.None,
            SubpixelPositioning = TextServer.SubpixelPositioning.Disabled,
        };
        var size = new Vector2I(FontSize, 0);
        if (sprites.GetTexture(FirstFontSprite + (int)colour) is { } sheet)
        {
            built.SetTextureImage(0, size, 0, sheet.GetImage());
        }

        built.SetCacheAscent(0, FontSize, GlyphHeight);
        built.SetCacheDescent(0, FontSize, 0);
        for (var character = FirstCharacter; character <= LastCharacter; character++)
        {
            built.SetGlyphAdvance(0, FontSize, character, new Vector2(GlyphWidth, 0));
            built.SetGlyphOffset(0, size, character, new Vector2(0, -GlyphHeight));
            built.SetGlyphSize(0, size, character, new Vector2(GlyphWidth, GlyphHeight));
            built.SetGlyphUVRect(
                0,
                size,
                character,
                new Rect2((character - FirstCharacter) * GlyphWidth, GlyphTop, GlyphWidth, GlyphHeight));
            built.SetGlyphTextureIdx(0, size, character, 0);
        }

        var font = new GameFont(built, FontSize, Colors.White);
        fonts[colour] = font;
        return font;
    }
}
