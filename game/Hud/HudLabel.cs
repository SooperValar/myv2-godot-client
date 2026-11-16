using Godot;
using Godot.Collections;
using TheLastGate.Assets;

namespace TheLastGate.Hud;

[Tool]
public partial class HudLabel : Label
{
    private TextColour colour = TextColour.Yellow;

    [Export]
    public TextColour Colour
    {
        get => colour;
        set
        {
            colour = value;
            ApplyFont();
        }
    }

    public override void _EnterTree() => ApplyFont();

    public override void _ValidateProperty(Dictionary property)
    {
        var name = property["name"].AsString();
        if (name != "theme_override_fonts/font" && name != "theme_override_font_sizes/font_size"
            && name != "theme_override_colors/font_color")
        {
            return;
        }

        property["usage"] = (int)(property["usage"].As<PropertyUsageFlags>() & ~PropertyUsageFlags.Storage);
    }

    public void ApplyFont()
    {
        if (!IsInsideTree() || HudView.Containing(this)?.Fonts is not { } fonts)
        {
            return;
        }

        var font = fonts.For(colour);
        AddThemeFontOverride("font", font.Font);
        AddThemeFontSizeOverride("font_size", font.Size);
        AddThemeColorOverride("font_color", font.Tint);
    }
}
