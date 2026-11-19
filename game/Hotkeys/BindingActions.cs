using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using TheLastGate.World;

namespace TheLastGate.Hotkeys;

public static class BindingActions
{
    private const int AnyDevice = -1;

    private static readonly Dictionary<string, Key> KeysByName = Enum.GetValues<Key>()
        .Where(key => Name(key) != null)
        .DistinctBy(key => Name(key))
        .ToDictionary(key => Name(key)!, key => key);

    // Godot fills the InputMap from the Input Map tab at startup, and the player's saved changes go in later, so the
    // first look at it is the defaults.
    private static readonly IReadOnlyDictionary<string, KeyChord?> ProjectDefaults = Current();

    public static IReadOnlyDictionary<string, KeyChord?> Defaults() => ProjectDefaults;

    public static IReadOnlyDictionary<string, KeyChord?> Current() =>
        Bindings.All.ToDictionary(binding => binding.Id, binding => For(binding.Id));

    // The editor's InputMap holds its own actions, so an editor preview reads the project's Input Map instead.
    public static KeyChord? For(string id)
    {
        if (Engine.IsEditorHint())
        {
            return FirstChord(ProjectEvents(id));
        }

        return InputMap.HasAction(id) ? FirstChord(InputMap.ActionGetEvents(id)) : null;
    }

    public static void Apply(IEnumerable<BindingChange> changes)
    {
        foreach (var (id, chord) in changes)
        {
            if (!InputMap.HasAction(id))
            {
                InputMap.AddAction(id);
            }

            InputMap.ActionEraseEvents(id);
            if (chord is { } bound)
            {
                InputMap.ActionAddEvent(id, ToEvent(bound));
            }
        }
    }

    public static void ResetAll() =>
        Apply(ProjectDefaults.Select(binding => new BindingChange(binding.Key, binding.Value)));

    public static InputEventKey ToEvent(KeyChord chord) => new()
    {
        Device = AnyDevice,
        Keycode = KeysByName[chord.Key],
        ShiftPressed = chord.Modifiers.HasFlag(MapKeys.Shift),
        CtrlPressed = chord.Modifiers.HasFlag(MapKeys.Ctrl),
        AltPressed = chord.Modifiers.HasFlag(MapKeys.Alt),
    };

    public static KeyChord? ToChord(InputEventKey key)
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

    private static IEnumerable<InputEvent> ProjectEvents(string id) =>
        ProjectSettings.GetSetting($"input/{id}").AsGodotDictionary().TryGetValue("events", out var events)
            ? events.AsGodotArray<InputEvent>()
            : [];

    private static KeyChord? FirstChord(IEnumerable<InputEvent> events) =>
        events.OfType<InputEventKey>().Select(ToChord).FirstOrDefault(chord => chord != null);

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
