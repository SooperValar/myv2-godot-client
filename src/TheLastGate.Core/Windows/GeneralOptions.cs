using System;
using System.Collections.Generic;

namespace TheLastGate.Windows;

public enum GeneralOption
{
    EscapeClosesWindowsFirst,
    ExpandedQuickStats,
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
        new(GeneralOption.ExpandedQuickStats, "Show Hit/Parry Score with Weapon/Armor value.", true, false),
        new(
            GeneralOption.GlideMovement,
            "Glide characters and the map smoothly between server updates instead of stepping.",
            false,
            true),
        new(
            GeneralOption.PickTileDrawnUnderMouse,
            "When walking, click the tile drawn under the mouse rather than where the map was.",
            false,
            true),
    ];

    public static bool IsShown(OptionRow row, Func<GeneralOption, bool> isOn) =>
        throw new NotImplementedException();

    public static bool ShowsItemTooltips(bool itemTooltips, bool always, bool keyHeld) =>
        throw new NotImplementedException();

    public static bool EscapeSendsReset(bool escapeClosesWindowsFirst, bool closedAWindow) =>
        !escapeClosesWindowsFirst || !closedAWindow;
}
