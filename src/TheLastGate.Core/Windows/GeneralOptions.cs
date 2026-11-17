using System;
using System.Collections.Generic;

namespace TheLastGate.Windows;

public enum GeneralOption
{
    EscapeClosesWindowsFirst,
    ExpandedQuickStats,
    GlideMovement,
    PickTileDrawnUnderMouse,
}

public sealed record OptionRow(GeneralOption Option, string Label, bool Default, bool GodotOnly);

public static class GeneralOptions
{
    public const string GodotHeading = "Godot Client";

    public static IReadOnlyList<OptionRow> Rows => throw new NotImplementedException();

    public static bool EscapeSendsReset(bool escapeClosesWindowsFirst, bool closedAWindow) =>
        throw new NotImplementedException();
}
