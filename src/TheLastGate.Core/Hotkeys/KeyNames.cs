using System.Collections.Generic;
using System.Linq;

namespace TheLastGate.Hotkeys;

// The names keys are bound, shown and saved under: the current client's names, plus the whole keypad.
public static class KeyNames
{
    public const string Shift = "Shift";
    public const string Ctrl = "Ctrl";
    public const string Alt = "Alt";

    private static readonly string[] Special =
    [
        "Space", "Tab", "Backspace", "Insert", "Home", "End", "PgUp", "PgDn", "Up", "Down", "Left", "Right",
        "-", "=", "[", "]", "\\", ";", "'", ",", ".", "/", "`", Shift, Ctrl, Alt,
        "Keypad *", "Keypad /", "Keypad -", "Keypad +", "Keypad .",
    ];

    private static readonly HashSet<string> Valid =
    [
        .. Enumerable.Range('A', 26).Select(letter => ((char)letter).ToString()),
        .. Enumerable.Range(0, 10).Select(digit => digit.ToString()),
        .. Enumerable.Range(1, 12).Select(number => $"F{number}"),
        .. Enumerable.Range(0, 10).Select(digit => $"Keypad {digit}"),
        .. Special,
    ];

    public static bool IsValid(string key) => Valid.Contains(key);
}
