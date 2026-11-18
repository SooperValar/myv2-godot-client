using Godot;

namespace TheLastGate.Hud;

// The box the current client draws round the Planner button below the skill list: a filled bar one pixel in from a
// one-pixel edge, in its 16-bit HUD colours.
[Tool]
public partial class PlannerButtonBox : Control
{
    private const int BoxWidth = 55;
    private const int BoxHeight = 15;

    private static readonly Color LightFill = FromRgb565(0x2800);
    private static readonly Color LightEdge = FromRgb565(0x7301);
    private static readonly Color DarkFill = FromRgb565(0x1020);
    private static readonly Color DarkEdge = FromRgb565(0x62E7);

    private bool darkMode;

    [Export]
    public bool DarkMode
    {
        get => darkMode;
        set
        {
            darkMode = value;
            QueueRedraw();
        }
    }

    // The current client's showbox draws its right and bottom edges one pixel past the width and height.
    public override void _Draw()
    {
        var fill = DarkMode ? DarkFill : LightFill;
        var edge = DarkMode ? DarkEdge : LightEdge;
        DrawRect(new Rect2(1, 1, BoxWidth - 1, BoxHeight - 1), fill);
        DrawRect(new Rect2(0, 0, BoxWidth, 1), edge);
        DrawRect(new Rect2(0, BoxHeight, BoxWidth, 1), edge);
        DrawRect(new Rect2(0, 0, 1, BoxHeight), edge);
        DrawRect(new Rect2(BoxWidth, 0, 1, BoxHeight), edge);
    }

    private static Color FromRgb565(int colour) =>
        new(((colour >> 11) & 0x1F) / 31f, ((colour >> 5) & 0x3F) / 63f, (colour & 0x1F) / 31f);
}
