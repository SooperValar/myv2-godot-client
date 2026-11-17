using System;
using Godot;

namespace TheLastGate.Map;

// The current client's resize corner: a 9-pixel staircase that lightens when hovered and turns gold while dragged.
[Tool]
public partial class ResizeGrip : Control
{
    private const int GripSize = 9;

    private static readonly Color Normal = Color.Color8(117, 106, 68);
    private static readonly Color HoveredColour = Color.Color8(168, 147, 90);
    private static readonly Color Gold = Color.Color8(243, 219, 106);

    private bool dragging;
    private bool hovered;

    public event Action<Vector2>? Dragged;

    public override void _Ready()
    {
        MouseEntered += () => SetHovered(true);
        MouseExited += () => SetHovered(false);
    }

    public override void _GuiInput(InputEvent inputEvent)
    {
        switch (inputEvent)
        {
            case InputEventMouseButton { ButtonIndex: MouseButton.Left } click:
                dragging = click.Pressed;
                QueueRedraw();
                AcceptEvent();
                break;
            case InputEventMouseMotion motion when dragging:
                Dragged?.Invoke(motion.Relative);
                AcceptEvent();
                break;
        }
    }

    public override void _Draw()
    {
        var colour = !hovered && !dragging ? Normal : dragging ? Gold : HoveredColour;
        for (var row = 0; row < GripSize; row++)
        {
            DrawRect(new Rect2(GripSize - (row + 1), row, row + 1, 1), colour);
        }
    }

    private void SetHovered(bool isHovered)
    {
        hovered = isHovered;
        QueueRedraw();
    }
}
