using System;
using System.Collections.Generic;

namespace TheLastGate.Windows;

public enum GeneralOption
{
    EscapeClosesWindowsFirst,
    GiveMore,
    UseQueue,
    EnterToTalk,
    ExpandedQuickStats,
    SkillPlanner,
    AdaptivePacing,
    ItemTooltips,
    ItemTooltipsAlways,
    GlideMovement,
    PickTileDrawnUnderMouse,
}

public sealed record OptionRow(GeneralOption Option, string Label, bool Default, bool GodotOnly)
{
    public GeneralOption? Under { get; init; }
}

// The General tab's checkboxes: the current client's in its own words, then this client's own, which start off.
public static class GeneralOptions
{
    public const string GodotHeading = "Godot Client";

    public static IReadOnlyList<OptionRow> Rows { get; } =
    [
        new(GeneralOption.EscapeClosesWindowsFirst, "Escape closes windows before canceling movement.", true, false),
        new(
            GeneralOption.GiveMore,
            "Holding Ctrl+Shift when giving an item will attempt to reload that item.",
            false,
            false),
        new(GeneralOption.UseQueue, "Holding Ctrl+Shift when using an item queues that use. Limit 5.", false, false),
        new(
            GeneralOption.EnterToTalk,
            "Press Enter to activate chat mode before typing. Press Enter again to send, or ESC to cancel.",
            false,
            false),
        new(GeneralOption.ExpandedQuickStats, "Show Hit/Parry Score with Weapon/Armor value.", true, false),
        new(
            GeneralOption.SkillPlanner,
            "Enable the skill planner: the Planner button below the skill list and its hotkey.",
            false,
            false),
        new(
            GeneralOption.AdaptivePacing,
            "Adaptive network smoothing: lower delay on steady connections, more buffering when the connection is uneven.",
            false,
            false),
        new(GeneralOption.ItemTooltips, "Show item tooltips for inventory and equipped gear.", true, false),
        new(
            GeneralOption.ItemTooltipsAlways,
            "Always show tooltips on hover. When off, hold the Item Tooltip hotkey to show them.",
            true,
            false) { Under = GeneralOption.ItemTooltips },
        new(
            GeneralOption.GlideMovement,
            "Interpolate character/map movement for smoothness",
            false,
            true),
        new(
            GeneralOption.PickTileDrawnUnderMouse,
            "When walking, click the tile drawn under the mouse rather than where the map was.",
            false,
            true),
    ];

    public static bool IsShown(OptionRow row, Func<GeneralOption, bool> isOn) =>
        row.Under is not { } parent || isOn(parent);

    public static bool ShowsItemTooltips(bool itemTooltips, bool always, bool keyHeld) =>
        itemTooltips && (always || keyHeld);

    public static bool EscapeSendsReset(bool escapeClosesWindowsFirst, bool closedAWindow) =>
        !escapeClosesWindowsFirst || !closedAWindow;
}
