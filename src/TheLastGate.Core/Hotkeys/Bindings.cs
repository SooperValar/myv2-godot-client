using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TheLastGate.World;

namespace TheLastGate.Hotkeys;

public enum BindingCategory
{
    Spell,
    Speed,
    World,
    Window,
    General,
    GuildCommands,
}

// Hold bindings are read while their key is down instead of firing when it is pressed.
public sealed record Binding(string Id, string Name, BindingCategory Category, KeyChord? Default)
{
    public bool Hold { get; init; }
}

// The current client's bindings, ids and defaults, in its order: a key bound twice fires the first.
public static class Bindings
{
    public const int SpellCount = 20;

    private const string SpellPrefix = "spell_";
    private const string SpellKeys = "12345QWERTASDFGZXCVB";

    public static IReadOnlyList<Binding> All { get; } =
    [
        .. SpellKeys.Select((key, slot) => new Binding(
            SpellId(slot),
            $"Spell {slot + 1}",
            BindingCategory.Spell,
            new KeyChord(key.ToString(), MapKeys.Ctrl))),
        new("speed_fast_hotkey", "Fast Mode", BindingCategory.Speed, Plain("F1")),
        new("speed_normal_hotkey", "Normal Mode", BindingCategory.Speed, Plain("F2")),
        new("speed_slow_hotkey", "Slow Mode", BindingCategory.Speed, Plain("F3")),
        new("toggle_percent_hotkey", "Toggle Health %", BindingCategory.World, Plain("F4")),
        new("toggle_stat_base_hotkey", "Toggle Stat Bases", BindingCategory.World, Plain("F5")),
        new("toggle_hide_sprite_hotkey", "Toggle Hide Sprites", BindingCategory.World, Plain("F6")),
        new("toggle_names_hotkey", "Toggle Names", BindingCategory.World, Plain("F7")),
        new("toggle_health_bars_hotkey", "Toggle Health Bars", BindingCategory.World, Plain("F8")),
        new("toggle_options_hotkey", "Options Window", BindingCategory.Window, Plain("F9")),
        new("toggle_gamma_hotkey", "Toggle Gamma", BindingCategory.Window, Plain("F10")),
        new("reset_window_size_hotkey", "Reset Window Size", BindingCategory.Window, With(MapKeys.Shift, "F10")),
        new("toggle_planner_hotkey", "Skill Planner", BindingCategory.Window, With(MapKeys.Ctrl, "P")),
        new("toggle_worldmap_hotkey", "World Map", BindingCategory.Window, null),
        new("item_tooltip_hotkey", "Item Tooltip (hold)", BindingCategory.Window, Plain(KeyNames.Alt)) { Hold = true },
        new("fightback_hotkey", "Toggle Fightback", BindingCategory.General, null),
        new("swap_gear_hotkey", "Swap Gearset", BindingCategory.General, Plain("Insert")),
        new("swap_position_hotkey", "Swap Position", BindingCategory.General, null),
        new("who_hotkey", "Who's Online", BindingCategory.General, null),
        new("display_debug_information_hotkey", "Display Debug Info", BindingCategory.General, Plain("F11")),
        new("exit_hotkey", "Exit", BindingCategory.General, Plain("F12")),
        new("my_buff_hotkey", "Buffs", BindingCategory.General, null),
        new("gc_offense_hotkey", "Offense Mode", BindingCategory.GuildCommands, null),
        new("gc_defense_hotkey", "Defense Mode", BindingCategory.GuildCommands, null),
        new("gc_passive_hotkey", "Passive Mode", BindingCategory.GuildCommands, null),
        new("gc_wait_hotkey", "Wait", BindingCategory.GuildCommands, null),
        new("gc_follow_hotkey", "Follow", BindingCategory.GuildCommands, null),
        new("gc_move_hotkey", "Move", BindingCategory.GuildCommands, null),
        new("gc_buffs_hotkey", "Buffs", BindingCategory.GuildCommands, null),
        new("gc_transfer_hotkey", "Transfer", BindingCategory.GuildCommands, null),
    ];

    public static string CategoryName(BindingCategory category) => category switch
    {
        BindingCategory.Spell => "Spell Hotkeys",
        BindingCategory.Speed => "Speed Hotkeys",
        BindingCategory.World => "World Hotkeys",
        BindingCategory.Window => "Window Hotkeys",
        BindingCategory.General => "General Hotkeys",
        BindingCategory.GuildCommands => "GC Commands",
        _ => throw new UnreachableException(),
    };

    public static string SpellId(int slot) => $"{SpellPrefix}{slot + 1}";

    public static int? SpellSlot(string id) =>
        id.StartsWith(SpellPrefix, StringComparison.Ordinal)
        && int.TryParse(id[SpellPrefix.Length..], out var number)
        && number is >= 1 and <= SpellCount
            ? number - 1
            : null;

    private static KeyChord Plain(string key) => new(key, MapKeys.None);

    private static KeyChord With(MapKeys modifiers, string key) => new(key, modifiers);
}
