using Godot;

namespace TheLastGate.Hud;

public partial class PortraitBar : Control
{
    private ColorRect background = null!;
    private ColorRect current = null!;
    private Color ownColour;
    private ColorRect reserved = null!;

    [Export] public Color LookColour { get; set; }

    public override void _Ready()
    {
        background = GetNode<ColorRect>("Background");
        reserved = GetNode<ColorRect>("Reserved");
        current = GetNode<ColorRect>("Current");
        ownColour = current.Color;
    }

    public void Show(BarWidths widths, bool showingLook)
    {
        background.Size = new Vector2(widths.Background, Size.Y);
        reserved.Position = new Vector2(Size.X - widths.Reserved, 0);
        reserved.Size = new Vector2(widths.Reserved, Size.Y);
        current.Size = new Vector2(widths.Current, Size.Y);
        current.Color = showingLook ? LookColour : ownColour;
    }
}
