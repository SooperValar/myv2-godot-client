using System;
using System.Collections.Generic;
using TheLastGate.Hotkeys;

namespace TheLastGate.Hotbar;

public static class GcCommands
{
    public const int FirstAction = 300;

    private const int GhostCompanion = 27;
    private const string AttackId = "gc_attack_hotkey";

    private static readonly (string Name, string HotkeyId)[] Commands =
    [
        ("GC Att", AttackId),
        ("GC Off", "gc_offense_hotkey"),
        ("GC Def", "gc_defense_hotkey"),
        ("GC Pas", "gc_passive_hotkey"),
        ("GC Wait", "gc_wait_hotkey"),
        ("GC Fol", "gc_follow_hotkey"),
        ("GC Move", "gc_move_hotkey"),
        ("GC Buff", "gc_buffs_hotkey"),
        ("GC Tran", "gc_transfer_hotkey"),
    ];

    public static bool IsCommand(int action) => action >= FirstAction && action < FirstAction + Commands.Length;

    public static IReadOnlyList<(int Action, string Name)> MenuFor(int[][] skills) =>
        skills[GhostCompanion][0] == 0
            ? []
            : Array.ConvertAll(Commands, command => (FirstAction + Array.IndexOf(Commands, command), command.Name));

    public static string Name(int action) => Commands[action - FirstAction].Name;

    public static string? Line(int action, int selectedCharacter)
    {
        if (!IsCommand(action))
        {
            return null;
        }

        var id = Commands[action - FirstAction].HotkeyId;
        return id == AttackId ? GcAttack.Line(selectedCharacter) : HotkeyCommands.Says(id);
    }
}
