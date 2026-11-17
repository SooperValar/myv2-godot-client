using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TheLastGate.Assets;
using TheLastGate.State;

namespace TheLastGate.Items;

public readonly record struct TooltipText(string Text, TextColour Colour);

public abstract record TooltipPart;

public sealed record TooltipLine(TooltipText Text, bool Wraps = false) : TooltipPart;

public sealed record TooltipSeparator : TooltipPart;

public sealed record TooltipTable(IReadOnlyList<TooltipText> Header, IReadOnlyList<IReadOnlyList<TooltipText>> Rows)
    : TooltipPart;

public sealed record TooltipBadges(string Label, IReadOnlyList<TooltipText> Badges) : TooltipPart;

public static class TooltipContent
{
    private const int WorstCondition = 4;
    private const int SilverPerGold = 100;
    private const int TenthsPerPercent = 10;

    private static readonly (ItemModsApplied Mod, TooltipText Badge)[] AppliedBadges =
    [
        (ItemModsApplied.Whetstoned, new TooltipText("Whetstoned", TextColour.Orange)),
        (ItemModsApplied.Soulstoned, new TooltipText("Soulstoned", TextColour.Blue)),
        (ItemModsApplied.Catalyzed, new TooltipText("Catalyzed", TextColour.Blue)),
        (ItemModsApplied.Enchanted, new TooltipText("Enchanted", TextColour.Purple)),
        (ItemModsApplied.Focused, new TooltipText("Focused", TextColour.Purple)),
        (ItemModsApplied.Augmented, new TooltipText("Augmented", TextColour.Teal)),
        (ItemModsApplied.Easier, new TooltipText("Easier to use", TextColour.Lime)),
        (ItemModsApplied.Corrupted, new TooltipText("Corrupted", TextColour.Pink)),
        (ItemModsApplied.Legacy, new TooltipText("Legacy", TextColour.White)),
    ];

    private static readonly (ItemModsPossible Mod, TooltipText Badge)[] PossibleBadges =
    [
        (ItemModsPossible.CanWhetstone, new TooltipText("Whetstone", TextColour.Orange)),
        (ItemModsPossible.CanSoulstone, new TooltipText("Soulstone", TextColour.Blue)),
        (ItemModsPossible.CanOsiris, new TooltipText("Osiris", TextColour.Blue)),
        (ItemModsPossible.CanEnchant, new TooltipText("Enchant", TextColour.Purple)),
    ];

    private static readonly (ItemModsPossible Mod, TooltipText Badge)[] ImpossibleBadges =
    [
        (ItemModsPossible.CannotSoulstone, new TooltipText("Soulstone", TextColour.Red)),
        (ItemModsPossible.CannotEnchant, new TooltipText("Enchant", TextColour.Red)),
    ];

    private static readonly string[] ConditionLabels =
        ["Perfect condition", "Showing signs of age", "Fairly old", "Old", "Very old and battered"];

    public static IReadOnlyList<TooltipPart> For(ItemTooltip tooltip)
    {
        var lines = tooltip.Lines;
        var details = tooltip.Details;
        var parts = new List<TooltipPart> { new TooltipLine(new TooltipText(details.Name, Colour(lines[0]))) };
        if (details.DescriptionLine is { } description && lines[description].Text.Length > 0)
        {
            parts.Add(Wrapped(lines[description]));
        }

        AddSection(parts, details.EffectLines.Select(index => Wrapped(lines[index])));
        AddSection(parts, Stats(details));
        AddSection(parts, Badges(details));
        AddSection(parts, Footer(details));
        AddSection(parts, details.OtherLines.Select(index => Wrapped(lines[index])));
        return parts;
    }

    private static void AddSection(List<TooltipPart> parts, IEnumerable<TooltipPart> section)
    {
        var sectionParts = section.ToList();
        if (sectionParts.Count == 0)
        {
            return;
        }

        parts.Add(new TooltipSeparator());
        parts.AddRange(sectionParts);
    }

    private static TooltipLine Wrapped(LogLine line) => new(new TooltipText(line.Text, Colour(line)), true);

    private static TextColour Colour(LogLine line) =>
        line.Font is >= (int)TextColour.Red and <= (int)TextColour.White ? (TextColour)line.Font : TextColour.Yellow;

    private static IEnumerable<TooltipPart> Stats(ItemDetails details)
    {
        var tableStats = details.Stats.Where(InTable).ToList();
        if (tableStats.Count > 0)
        {
            yield return Table(details, tableStats);
        }

        foreach (var stat in details.Stats.Where(stat => !InTable(stat)))
        {
            yield return new TooltipLine(new TooltipText($"{stat.Name}: {Special(stat)}", TextColour.Yellow));
        }
    }

    // Complexity and Min. Rank read better as lines under the table than as a column of mostly empty cells.
    private static bool InTable(ItemStat stat) => stat.Kind is not (ItemStatKind.Single or ItemStatKind.Rank);

    private static TooltipTable Table(ItemDetails details, IReadOnlyList<ItemStat> stats)
    {
        var showActive = !details.SoulstoneTable && details.Stats.Any(ShowsActive);
        var showRequired = !details.SoulstoneTable && details.Stats.Any(ShowsRequired);
        var valueLabel = details.SoulstoneTable ? "Adds" : showActive ? "Passive" : "Value";
        var header = new List<string> { "Stat", valueLabel };
        if (showActive)
        {
            header.Add("Active");
        }

        if (showRequired)
        {
            header.Add("Req");
        }

        var rows = stats.Select(stat => Row(stat, showActive, showRequired)).ToList();
        return new TooltipTable(header.Select(label => new TooltipText(label, TextColour.Orange)).ToList(), rows);
    }

    private static IReadOnlyList<TooltipText> Row(ItemStat stat, bool showActive, bool showRequired)
    {
        var passive = Special(stat) ?? Value(stat, stat.Passive);
        var row = new List<TooltipText>
        {
            new(stat.Name, TextColour.Yellow),
            new(passive, stat.Passive < 0 ? TextColour.Red : TextColour.White),
        };
        if (showActive)
        {
            row.Add(
                HasActive(stat)
                    ? new TooltipText(Value(stat, stat.Active), stat.Active < 0 ? TextColour.Red : TextColour.Teal)
                    : new TooltipText("", TextColour.Teal));
        }

        if (showRequired)
        {
            row.Add(new TooltipText(ShowsRequired(stat) ? Whole(stat.Required!.Value) : "", TextColour.Dim));
        }

        return row;
    }

    private static bool HasActive(ItemStat stat) => stat.Kind is ItemStatKind.Plain or ItemStatKind.Percent;

    private static bool ShowsActive(ItemStat stat) => HasActive(stat) && stat.Active != stat.Passive;

    private static bool ShowsRequired(ItemStat stat) => stat.Required is not (null or 0);

    private static string Value(ItemStat stat, int value)
    {
        if (stat.Kind != ItemStatKind.Percent)
        {
            return Signed(value);
        }

        return value % TenthsPerPercent == 0
            ? Signed(value / TenthsPerPercent) + "%"
            : (value / (double)TenthsPerPercent).ToString("+0.0;-0.0", CultureInfo.InvariantCulture) + "%";
    }

    private static string? Special(ItemStat stat) => stat.Kind switch
    {
        ItemStatKind.Regen => $"{Signed(stat.Passive)} ({Signed(stat.Active)}/s)",
        ItemStatKind.Duration => $"{Whole(stat.Passive)} seconds",
        ItemStatKind.Single => Whole(stat.Passive),
        ItemStatKind.Rank => $"{Whole(stat.Passive)} ({stat.RankName})",
        _ => null,
    };

    private static IEnumerable<TooltipPart> Badges(ItemDetails details)
    {
        var has = AppliedBadges.Where(entry => details.ModsApplied.HasFlag(entry.Mod)).Select(entry => entry.Badge);
        var can = PossibleBadges.Where(entry => details.ModsPossible.HasFlag(entry.Mod)).Select(entry => entry.Badge);
        var cannot = ImpossibleBadges.Where(entry => details.ModsPossible.HasFlag(entry.Mod))
            .Select(entry => entry.Badge);
        TooltipBadges[] rows =
        [
            new("Has:", has.ToList()),
            new("Can:", can.ToList()),
            new("Cannot:", cannot.ToList()),
        ];
        return rows.Where(row => row.Badges.Count > 0);
    }

    private static IEnumerable<TooltipPart> Footer(ItemDetails details)
    {
        if (details.Level is { } level)
        {
            yield return Line($"Level {Whole(level)} - {Whole(details.KillsToLevel)} kills to next", TextColour.Green);
        }

        if (details.Blessings is { } blessings)
        {
            var plural = blessings == 1 ? "" : "s";
            var text = $"{Whole(blessings)} blessing{plural} left - next in {Whole(details.KillsToBlessing)} kills";
            yield return Line(text, TextColour.Green);
        }

        if (details.Condition is { } condition)
        {
            var colour = condition >= WorstCondition ? TextColour.Red : TextColour.Yellow;
            yield return Line(ConditionLabels[condition], colour);
        }

        if (details.SellSilver is { } silver)
        {
            var price = $"Sells for about {Whole(silver / SilverPerGold)}G {Whole(silver % SilverPerGold)}S";
            yield return Line(price, TextColour.Lime);
        }
    }

    private static TooltipLine Line(string text, TextColour colour) => new(new TooltipText(text, colour));

    private static string Signed(int value) => value.ToString("+0;-0", CultureInfo.InvariantCulture);

    private static string Whole(int value) => value.ToString(CultureInfo.InvariantCulture);
}
