using System.Linq;
using Godot;
using TheLastGate.Assets;
using TheLastGate.Hud;

namespace TheLastGate.Planner;

// The planner's windows are framed by the game's window sprite and written in the HUD's font.
public static class PlannerLook
{
    private const int FrameSprite = 18128;

    private static readonly Color LightBackground = Color.Color8(42, 15, 10, 240);
    private static readonly Color DarkBackground = Color.Color8(23, 0, 5);

    public static void ShowBackground(Control window, bool darkMode) =>
        ((StyleBoxFlat)window.GetThemeStylebox("panel")).BgColor = darkMode ? DarkBackground : LightBackground;

    // The text is in the HUD's font, so a font set swapped in the options reaches the planner too. Every label keeps
    // the planner's own colour, which tints the font set's white letters.
    public static void ApplyFont(Control window, ref Theme? sceneTheme)
    {
        if (HudView.Containing(window)?.Fonts is not { } fonts)
        {
            return;
        }

        sceneTheme ??= window.Theme;
        var font = fonts.For(TextColour.White);
        var themed = (Theme)sceneTheme.Duplicate();
        themed.DefaultFont = font.Font;
        themed.DefaultFontSize = font.Size;
        window.Theme = themed;
    }

    // The window sprite lives in the sprite archives, so it is put on when the HUD has them.
    public static void ApplyFrameSprite(Control window)
    {
        if (HudView.Containing(window)?.Sprites?.GetTexture(FrameSprite) is not { } texture)
        {
            return;
        }

        foreach (var frame in window.FindChildren("Frame", nameof(NinePatchRect), true, false).Cast<NinePatchRect>())
        {
            frame.Texture = texture;
        }
    }
}
