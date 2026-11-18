using Godot;

namespace TheLastGate.Assets;

// Puts a font on every window under a node whose scene theme sets one, so a swapped font reaches every window. The
// scene theme is kept, and each swap starts again from it; text keeps the theme's colours, which tint white letters.
public static class WindowFonts
{
    private const string SceneThemeMeta = "scene_theme";

    public static void Apply(Node node, Font font, int size)
    {
        if (node is Control control && SceneTheme(control) is { } sceneTheme)
        {
            control.Theme = WithFont(sceneTheme, font, size);
        }

        foreach (var child in node.GetChildren())
        {
            Apply(child, font, size);
        }
    }

    public static Theme WithFont(Theme theme, Font font, int size)
    {
        var themed = (Theme)theme.Duplicate();
        themed.DefaultFont = font;
        themed.DefaultFontSize = size;
        return themed;
    }

    // Only a theme that sets a font is swapped; the rest take theirs from the window around them.
    private static Theme? SceneTheme(Control control)
    {
        if (!control.HasMeta(SceneThemeMeta))
        {
            if (control.Theme is not { } theme || !theme.HasDefaultFont())
            {
                return null;
            }

            control.SetMeta(SceneThemeMeta, theme);
        }

        return control.GetMeta(SceneThemeMeta).As<Theme>();
    }
}
