using Godot;
using TheLastGate.World;

namespace TheLastGate.Hud;

public partial class ClickArea : Control
{
    [Signal]
    public delegate void ReleasedEventHandler(MapButton button);

    public override void _GuiInput(InputEvent inputEvent)
    {
        if (inputEvent is not InputEventMouseButton click
            || click.ButtonIndex is MouseButton.WheelUp or MouseButton.WheelDown)
        {
            return;
        }

        AcceptEvent();
        if (click.Pressed || !Contains(click.Position))
        {
            return;
        }

        var button = click.ButtonIndex switch
        {
            MouseButton.Left => MapButton.Left,
            MouseButton.Right => MapButton.Right,
            _ => MapButton.None,
        };
        if (button != MapButton.None)
        {
            EmitSignal(SignalName.Released, Variant.From(button));
        }
    }

    protected virtual bool Contains(Vector2 point) => new Rect2(Vector2.Zero, Size).HasPoint(point);
}
