using Godot;
using TheLastGate.Assets;

namespace TheLastGate.World;

public partial class SpriteProbeView : PanelContainer
{
    private Label readout = null!;

    public WorldView World { get; set; } = null!;

    public static bool IsAvailable => OS.HasFeature("editor");

    public override void _Ready() => readout = GetNode<Label>("%Readout");

    public void UseFont(GameFont font)
    {
        readout.AddThemeFontOverride("font", font.Font);
        readout.AddThemeFontSizeOverride("font_size", font.Size);
        readout.AddThemeColorOverride("font_color", font.Tint);
    }

    public override void _Process(double delta)
    {
        var sprites = World.SpritesUnder(GetViewport().GetMousePosition());
        Visible = sprites != null;
        if (sprites is { } shown)
        {
            readout.Text = $"floor {Number(shown.Floor)}   object {Number(shown.Object)}   "
                + $"character {Number(shown.Character)}";
        }
    }

    private static string Number(int spriteNumber) => spriteNumber == 0 ? "-" : spriteNumber.ToString();
}
