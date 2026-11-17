using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using TheLastGate.Game;
using TheLastGate.Hotkeys;
using TheLastGate.World;

namespace TheLastGate.Hud;

// Turns Godot key presses into chords named the way bindings are saved and shown.
public static class HotkeyInput
{
    private static readonly Dictionary<string, Key> KeysByName = Enum.GetValues<Key>()
        .Where(key => Name(key) != null)
        .DistinctBy(key => Name(key))
        .ToDictionary(key => Name(key)!, key => key);

    public static KeyChord? Chord(InputEventKey key)
    {
        if (Name(key.Keycode) is not { } name)
        {
            return null;
        }

        var modifiers = (key.ShiftPressed ? MapKeys.Shift : MapKeys.None)
            | (key.CtrlPressed ? MapKeys.Ctrl : MapKeys.None)
            | (key.AltPressed ? MapKeys.Alt : MapKeys.None);
        return new KeyChord(name, modifiers & ~ModifierOf(name));
    }

    public static bool IsHeld(KeyChord chord) =>
        chord.IsHeld(HeldKeys.Current(), KeysByName.TryGetValue(chord.Key, out var key) && Input.IsKeyPressed(key));

    // A modifier pressed on its own also reports itself as held, so it doesn't count towards its own chord.
    private static MapKeys ModifierOf(string name) => name switch
    {
        KeyNames.Shift => MapKeys.Shift,
        KeyNames.Ctrl => MapKeys.Ctrl,
        KeyNames.Alt => MapKeys.Alt,
        _ => MapKeys.None,
    };

    private static string? Name(Key key) => key switch
    {
        >= Key.A and <= Key.Z or >= Key.Key0 and <= Key.Key9 => ((char)key).ToString(),
        >= Key.F1 and <= Key.F12 => $"F{key - Key.F1 + 1}",
        >= Key.Kp0 and <= Key.Kp9 => $"Keypad {key - Key.Kp0}",
        Key.Space => "Space",
        Key.Tab => "Tab",
        Key.Backspace => "Backspace",
        Key.Insert => "Insert",
        Key.Home => "Home",
        Key.End => "End",
        Key.Pageup => "PgUp",
        Key.Pagedown => "PgDn",
        Key.Up => "Up",
        Key.Down => "Down",
        Key.Left => "Left",
        Key.Right => "Right",
        Key.Minus => "-",
        Key.Equal => "=",
        Key.Bracketleft => "[",
        Key.Bracketright => "]",
        Key.Backslash => "\\",
        Key.Semicolon => ";",
        Key.Apostrophe => "'",
        Key.Comma => ",",
        Key.Period => ".",
        Key.Slash => "/",
        Key.Quoteleft => "`",
        Key.Shift => KeyNames.Shift,
        Key.Ctrl => KeyNames.Ctrl,
        Key.Alt => KeyNames.Alt,
        Key.KpMultiply => "Keypad *",
        Key.KpDivide => "Keypad /",
        Key.KpSubtract => "Keypad -",
        Key.KpAdd => "Keypad +",
        Key.KpPeriod => "Keypad .",
        _ => null,
    };
}
