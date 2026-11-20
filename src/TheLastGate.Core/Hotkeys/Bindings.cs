using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
public sealed record Binding(string Id, string Name, BindingCategory Category)
{
    public bool Hold { get; init; }
}

// The current client's bindings and ids, in its order; their default keys are in the project's Input Map.
public static class Bindings
{
    public const int SpellCount = 20;

    private const string SpellPrefix = "spell_";

    public static IReadOnlyList<Binding> All { get; } =
    [
        .. Enumerable.Range(0, SpellCount)
            .Select(slot => new Binding(SpellId(slot), $"Spell {slot + 1}", BindingCategory.Spell)),
        new("speed_fast_hotkey", "Fast Mode", BindingCategory.Speed),
        new("speed_normal_hotkey", "Normal Mode", BindingCategory.Speed),
        new("speed_slow_hotkey", "Slow Mode", BindingCategory.Speed),
        new("toggle_percent_hotkey", "Toggle Health %", BindingCategory.World),
        new("toggle_stat_base_hotkey", "Toggle Stat Bases", BindingCategory.World),
        new("toggle_hide_sprite_hotkey", "Toggle Hide Sprites", BindingCategory.World),
        new("toggle_names_hotkey", "Toggle Names", BindingCategory.World),
        new("toggle_health_bars_hotkey", "Toggle Health Bars", BindingCategory.World),
        new("toggle_options_hotkey", "Options Window", BindingCategory.Window),
        new("toggle_gamma_hotkey", "Toggle World Gamma", BindingCategory.Window),
        new("reset_window_size_hotkey", "Reset Window Size", BindingCategory.Window),
        new("toggle_planner_hotkey", "Skill Planner", BindingCategory.Window),
        new("toggle_worldmap_hotkey", "World Map", BindingCategory.Window),
        new("toggle_session_stats_hotkey", "Session Stats", BindingCategory.Window),
        new("item_tooltip_hotkey", "Item Tooltip (hold)", BindingCategory.Window) { Hold = true },
        new("fightback_hotkey", "Toggle Fightback", BindingCategory.General),
        new("swap_gear_hotkey", "Swap Gearset", BindingCategory.General),
        new("swap_position_hotkey", "Swap Position", BindingCategory.General),
        new("who_hotkey", "Who's Online", BindingCategory.General),
        new("display_debug_information_hotkey", "Display Debug Info", BindingCategory.General),
        new("exit_hotkey", "Exit", BindingCategory.General),
        new("my_buff_hotkey", "Buffs", BindingCategory.General),
        new("gc_offense_hotkey", "Offense Mode", BindingCategory.GuildCommands),
        new("gc_defense_hotkey", "Defense Mode", BindingCategory.GuildCommands),
        new("gc_passive_hotkey", "Passive Mode", BindingCategory.GuildCommands),
        new("gc_wait_hotkey", "Wait", BindingCategory.GuildCommands),
        new("gc_follow_hotkey", "Follow", BindingCategory.GuildCommands),
        new("gc_move_hotkey", "Move", BindingCategory.GuildCommands),
        new("gc_buffs_hotkey", "Buffs", BindingCategory.GuildCommands),
        new("gc_transfer_hotkey", "Transfer", BindingCategory.GuildCommands),
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
}
