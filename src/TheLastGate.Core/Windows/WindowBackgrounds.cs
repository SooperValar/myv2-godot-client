using System;
using System.Collections.Generic;
using System.Globalization;

namespace TheLastGate.Windows;

public enum WindowBackground
{
    None,
    Shop,
    Depot,
    Smith,
    SkillTree,
    Waypoints,
    Text,
}

public sealed record WindowBackgroundRow(WindowBackground Window, string Label, string SettingKey);

public static class WindowBackgrounds
{
    public const int Solid = 100;
    public const int Clear = 0;

    public static IReadOnlyList<WindowBackgroundRow> Rows { get; } =
    [
        new(WindowBackground.Shop, "Shop / Grave", "shop"),
        new(WindowBackground.Depot, "Depot", "depot"),
        new(WindowBackground.Smith, "Smith", "smith"),
        new(WindowBackground.SkillTree, "Skill Tree", "skill_tree"),
        new(WindowBackground.Waypoints, "Waypoints", "waypoints"),
        new(WindowBackground.Text, "Text Windows", "text"),
    ];

    public static int Clamp(int opacity) => Math.Clamp(opacity, Clear, Solid);

    public static string Text(int opacity) => opacity.ToString(CultureInfo.InvariantCulture) + "%";
}
