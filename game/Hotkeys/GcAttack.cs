using System;

namespace TheLastGate.Hotkeys;

// MyV2's #gca sends the Ghost Companion after the given character, or after the player's own enemy without one.
// merc.exe added the Alt-selected character to a bare #gca, and so does this client.
public static class GcAttack
{
    private const string Command = "#gca";

    private static readonly string[] Names = ["gca", "gcattack"];

    public static string Line(int selectedCharacter) =>
        selectedCharacter > 0 ? $"{Command} {selectedCharacter}" : Command;

    public static string Complete(string typed, int selectedCharacter) =>
        IsBare(typed.Trim()) ? Line(selectedCharacter) : typed;

    private static bool IsBare(string line) =>
        line.Length > 1
        && line[0] is '#' or '/'
        && Array.Exists(Names, name => line[1..].Equals(name, StringComparison.OrdinalIgnoreCase));
}
