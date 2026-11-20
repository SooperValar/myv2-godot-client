using Godot;
using TheLastGate.Assets;

namespace TheLastGate.Hud;

// The sprites live in the archives rather than the project, so a theme's StyleBoxTexture names its sprite in
// metadata and gets the texture once the HUD has loaded them. Not in the editor: the theme is a shared file
// there, and saving would write the sprite into it.
public static class ThemeSprites
{
    private const string SpriteKey = "sprite";

    public static void Apply(Theme theme, SpriteLoader sprites)
    {
        if (Engine.IsEditorHint())
        {
            return;
        }

        foreach (var type in theme.GetStyleboxTypeList())
        {
            foreach (var name in theme.GetStyleboxList(type))
            {
                if (theme.GetStylebox(name, type) is StyleBoxTexture box
                    && box.HasMeta(SpriteKey)
                    && sprites.GetTexture(box.GetMeta(SpriteKey).AsInt32()) is { } texture)
                {
                    box.Texture = texture;
                }
            }
        }
    }
}
