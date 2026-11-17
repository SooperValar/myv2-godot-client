using System;
using TheLastGate.World;

namespace TheLastGate.Hotkeys;

public readonly record struct KeyChord(string Key, MapKeys Modifiers)
{
    private static readonly (MapKeys Modifier, string Long, string Short)[] Prefixes =
    [
        (MapKeys.Shift, "Shift+", "S-"),
        (MapKeys.Ctrl, "Ctrl+", "C-"),
        (MapKeys.Alt, "Alt+", "A-"),
    ];

    public string LongName => Prefixed(prefix => prefix.Long);

    // The hotbar's label: "C-1".
    public string ShortName => Prefixed(prefix => prefix.Short);

    // A modifier bound alone is held on either side of the keyboard; other keys may have extra modifiers held too.
    public bool IsHeld(MapKeys heldModifiers, bool keyDown) => Key switch
    {
        KeyNames.Shift => heldModifiers.HasFlag(MapKeys.Shift),
        KeyNames.Ctrl => heldModifiers.HasFlag(MapKeys.Ctrl),
        KeyNames.Alt => heldModifiers.HasFlag(MapKeys.Alt),
        _ => keyDown && heldModifiers.HasFlag(Modifiers),
    };

    // Reads only what LongName writes: modifiers in Shift, Ctrl, Alt order, then a known key.
    public static KeyChord? Parse(string text)
    {
        var modifiers = MapKeys.None;
        var rest = text;
        foreach (var (modifier, prefix, _) in Prefixes)
        {
            if (rest.StartsWith(prefix, StringComparison.Ordinal))
            {
                modifiers |= modifier;
                rest = rest[prefix.Length..];
            }
        }

        return KeyNames.IsValid(rest) ? new KeyChord(rest, modifiers) : null;
    }

    private string Prefixed(Func<(MapKeys Modifier, string Long, string Short), string> name)
    {
        var text = "";
        foreach (var prefix in Prefixes)
        {
            if (Modifiers.HasFlag(prefix.Modifier))
            {
                text += name(prefix);
            }
        }

        return text + Key;
    }
}
