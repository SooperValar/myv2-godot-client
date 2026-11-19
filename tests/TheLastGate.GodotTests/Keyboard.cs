using Godot;

namespace TheLastGate.GodotTests;

// Keys go through Input.ParseInputEvent, the path real key presses take. Viewport.PushInput skips part of it, and the
// chat history bug only showed on the real path.
public static class Keyboard
{
    public static void Press(Key key, bool ctrl = false, char typed = '\0', bool shift = false)
    {
        Send(
            new InputEventKey
                { Keycode = key, CtrlPressed = ctrl, ShiftPressed = shift, Unicode = typed, Pressed = true });
        Send(new InputEventKey { Keycode = key, CtrlPressed = ctrl, ShiftPressed = shift });
    }

    public static void HoldUntilItRepeats(Key key)
    {
        Send(new InputEventKey { Keycode = key, Pressed = true });
        Send(new InputEventKey { Keycode = key, Pressed = true, Echo = true });
        Send(new InputEventKey { Keycode = key });
    }

    public static void Type(string text)
    {
        foreach (var letter in text)
        {
            Press(Key.A + (char.ToUpperInvariant(letter) - 'A'), typed: letter);
        }
    }

    private static void Send(InputEventKey key)
    {
        key.PhysicalKeycode = key.Keycode;
        Input.ParseInputEvent(key);
        Input.FlushBufferedEvents();
    }
}
