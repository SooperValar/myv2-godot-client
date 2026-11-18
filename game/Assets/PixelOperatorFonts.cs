using Godot;

namespace TheLastGate.Assets;

// The launcher's own fonts, as its theme draws them in the editor.
public sealed class PixelOperatorFonts : ILauncherFontSet
{
    private const int FontSize = 16;

    public Font Regular { get; } = GD.Load<Font>("res://fonts/pixeloperator.ttf");

    public Font Bold { get; } = GD.Load<Font>("res://fonts/pixeloperator-bold.ttf");

    public int Size => FontSize;
}
