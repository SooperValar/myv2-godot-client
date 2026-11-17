using Godot;

namespace TheLastGate.Map;

// The current client's close button cross: two 7-pixel diagonals in the middle of the button, lighter while hovered.
[Tool]
public partial class PixelCross : Control
{
    private const int CrossSize = 7;

    private static readonly Color Gold = Color.Color8(243, 219, 106);
    private static readonly Color HoveredGold = Color.Color8(217, 191, 79);

    private bool hovered;

    public bool Hovered
    {
        get => hovered;
        set
        {
            hovered = value;
            QueueRedraw();
        }
    }

    public override void _Draw()
    {
        var colour = hovered ? HoveredGold : Gold;
        var left = Mathf.Floor((Size.X - CrossSize) / 2);
        var top = Mathf.Floor((Size.Y - CrossSize) / 2);
        for (var step = 0; step < CrossSize; step++)
        {
            DrawRect(new Rect2(left + step, top + step, 1, 1), colour);
            DrawRect(new Rect2(left + CrossSize - 1 - step, top + step, 1, 1), colour);
        }
    }
}
