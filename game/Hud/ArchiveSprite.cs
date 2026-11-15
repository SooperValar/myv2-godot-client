using Godot;
using Godot.Collections;

namespace TheLastGate.Hud;

[Tool]
public partial class ArchiveSprite : TextureRect
{
    private int darkSpriteNumber;
    private int spriteNumber;

    [Export]
    public int SpriteNumber
    {
        get => spriteNumber;
        set
        {
            spriteNumber = value;
            LoadSprite();
        }
    }

    [Export]
    public int DarkSpriteNumber
    {
        get => darkSpriteNumber;
        set
        {
            darkSpriteNumber = value;
            LoadSprite();
        }
    }

    public override void _EnterTree() => LoadSprite();

    public override void _ValidateProperty(Dictionary property)
    {
        if (property["name"].AsStringName() != TextureRect.PropertyName.Texture)
        {
            return;
        }

        var usage = property["usage"].As<PropertyUsageFlags>() & ~PropertyUsageFlags.Storage;
        property["usage"] = (int)(usage | PropertyUsageFlags.ReadOnly);
    }

    public void LoadSprite()
    {
        if (!IsInsideTree() || FindHud() is not { Sprites: { } sprites } hud)
        {
            return;
        }

        Texture = sprites.GetTexture(hud.DarkMode && darkSpriteNumber != 0 ? darkSpriteNumber : spriteNumber);
    }

    private HudView? FindHud()
    {
        for (var node = GetParent(); node != null; node = node.GetParent())
        {
            if (node is HudView hud)
            {
                return hud;
            }
        }

        return null;
    }
}
