using Godot;
using TheLastGate.World;

namespace TheLastGate.Game;

public static class HeldKeys
{
    public static MapKeys Current() =>
        (Input.IsKeyPressed(Key.Shift) ? MapKeys.Shift : MapKeys.None)
        | (Input.IsKeyPressed(Key.Ctrl) ? MapKeys.Ctrl : MapKeys.None)
        | (Input.IsKeyPressed(Key.Alt) ? MapKeys.Alt : MapKeys.None);
}
