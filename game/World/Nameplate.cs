using Godot;

namespace TheLastGate.World;

// The name and health bar over a character. The scene's layout and colours start as the current client's and can be
// restyled in the editor, as long as the four named parts stay, and any font can be swapped in.
public partial class Nameplate : Node2D
{
    private Control bar = null!;
    private ColorRect barBackground = null!;
    private ColorRect barBorder = null!;
    private ColorRect barFill = null!;
    private float fullBarWidth;
    private Label nameLabel = null!;
    private (string Text, NameplateFont Font, int HealthPercent, bool ShowText, bool ShowBar) shown;

    public override void _Ready()
    {
        nameLabel = GetNode<Label>("%NameLabel");
        bar = GetNode<Control>("%HealthBar");
        barBorder = GetNode<ColorRect>("%BarBorder");
        barBackground = GetNode<ColorRect>("%BarBackground");
        barFill = GetNode<ColorRect>("%BarFill");
        fullBarWidth = barFill.Size.X;
    }

    public void Display(string text, NameplateFont font, int healthPercent, bool showText, bool showBar)
    {
        if (shown == (text, font, healthPercent, showText, showBar))
        {
            return;
        }

        shown = (text, font, healthPercent, showText, showBar);
        nameLabel.Visible = showText && text.Length > 0;
        if (nameLabel.Visible)
        {
            nameLabel.Text = text;
            nameLabel.AddThemeFontOverride("font", font.Font);
            nameLabel.AddThemeFontSizeOverride("font_size", font.Size);
            nameLabel.AddThemeColorOverride("font_color", font.Colour);
            var left = font.CentresLikeTheCurrentClient
                ? NameplateText.LeftOffset(text)
                : -Mathf.RoundToInt(font.Font.GetStringSize(text, fontSize: font.Size).X / 2);
            nameLabel.Position = new Vector2(left, nameLabel.Position.Y);
        }

        bar.Visible = showBar;
        barBorder.Visible = healthPercent != 0;
        barBackground.Visible = healthPercent != 0;
        barFill.Size = new Vector2((int)(fullBarWidth * (healthPercent / 100f)), barFill.Size.Y);
    }
}
