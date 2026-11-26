using System.Collections.Generic;

namespace TheLastGate.Hotkeys;

public static class HotkeyCommands
{
    private static readonly Dictionary<string, string> Said = new()
    {
        ["fightback_hotkey"] = "/fightback",
        ["who_hotkey"] = "/who",
        ["my_buff_hotkey"] = "/buffs",

        // Words the player says; the Ghost Companion obeys them.
        ["gc_offense_hotkey"] = "Offense",
        ["gc_defense_hotkey"] = "Defense",
        ["gc_passive_hotkey"] = "Passive",
        ["gc_wait_hotkey"] = "Wait",
        ["gc_follow_hotkey"] = "Follow",
        ["gc_move_hotkey"] = "Move",
        ["gc_buffs_hotkey"] = "Buffs",
        ["gc_transfer_hotkey"] = "Transfer",
    };

    public static string? Says(string id) => Said.GetValueOrDefault(id);
}
