using Godot;
using TheLastGate.World;

namespace TheLastGate.Hud;

public partial class ClickArea : Control
{
    [Signal]
    public delegate void ReleasedEventHandler(MapButton button);

    public override void _GuiInput(InputEvent inputEvent)
    {
        if (inputEvent is not InputEventMouseButton { Pressed: false } click)
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
}
