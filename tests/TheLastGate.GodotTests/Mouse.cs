using Godot;

namespace TheLastGate.GodotTests;

public static class Mouse
{
    public static void Click(Vector2 position)
    {
        Send(new InputEventMouseButton { Pressed = true, ButtonMask = MouseButtonMask.Left }, position);
        Send(new InputEventMouseButton(), position);
    }

    private static void Send(InputEventMouseButton button, Vector2 position)
    {
        button.ButtonIndex = MouseButton.Left;
        button.Position = position;
        button.GlobalPosition = position;
        Input.ParseInputEvent(button);
        Input.FlushBufferedEvents();
    }
}
