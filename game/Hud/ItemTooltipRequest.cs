using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TheLastGate.Assets;
using TheLastGate.Items;
using TheLastGate.State;

namespace TheLastGate.Hud;

// What a slot's tooltip shows right now: the MyV2 server's lines for the item, and with Shift the worn pieces it would
// replace, beside it. The tooltip rebuilds itself whenever this changes.
public sealed record ItemTooltipRequest(IReadOnlyList<LogLine>? Lines, IReadOnlyList<IReadOnlyList<LogLine>> Equipped)
{
    // The server's tooltip fonts: 0-3 its four text colours, 8 the worn slots the item compares with (not shown),
    // 9 a divider, and 10-14 the item tiers Common, Magic, Rare, Epic and Legendary.
    public const int CompareFont = 8;

    private const int DividerFont = 9;
    private const int FirstTierFont = 10;
    private const string FetchingText = "Fetching item details...";
    private const string EquippedTitle = "Equipped:";
    private const string CompareStart = "W ";

    private static readonly TextColour[] TierColours =
        [TextColour.White, TextColour.Blue, TextColour.Yellow, TextColour.Purple, TextColour.Orange];

    // The worn slots the server says the item would replace, from its "W a b" line.
    public static IReadOnlyList<int> ComparedSlots(IReadOnlyList<LogLine>? lines)
    {
        var compare = lines?.FirstOrDefault(line => line.Font == CompareFont && line.Text.StartsWith(CompareStart));
        if (compare is not { } found)
        {
            return [];
        }

        return found.Text[CompareStart.Length..]
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(part => int.TryParse(part, NumberStyles.Integer, CultureInfo.InvariantCulture, out var slot) ? slot : -1)
            .Where(slot => slot >= 0)
            .ToArray();
    }

    public IReadOnlyList<TooltipPart> Parts() =>
        Lines == null ? [new TooltipLine(new TooltipText(FetchingText, TextColour.Dim))] : PartsOf(Lines).ToArray();

    // Shown in a column to the right of the item's own lines.
    public IReadOnlyList<TooltipPart> EquippedParts()
    {
        var parts = new List<TooltipPart>();
        foreach (var equipped in Equipped)
        {
            if (parts.Count > 0)
            {
                parts.Add(new TooltipSeparator());
            }

            parts.Add(new TooltipLine(new TooltipText(EquippedTitle, TextColour.Blue)));
            parts.AddRange(PartsOf(equipped));
        }

        return parts;
    }

    public bool Equals(ItemTooltipRequest? other) =>
        other != null && ReferenceEquals(Lines, other.Lines) && Equipped.SequenceEqual(other.Equipped);

    public override int GetHashCode() => Lines?.GetHashCode() ?? 0;

    private static IEnumerable<TooltipPart> PartsOf(IReadOnlyList<LogLine> lines)
    {
        foreach (var line in lines)
        {
            switch (line.Font)
            {
                case CompareFont:
                    continue;
                case DividerFont:
                    yield return new TooltipSeparator();
                    continue;
                default:
                    yield return new TooltipLine(new TooltipText(line.Text, ColourOf(line.Font)), true);
                    continue;
            }
        }
    }

    private static TextColour ColourOf(int font) =>
        font >= FirstTierFont && font < FirstTierFont + TierColours.Length
            ? TierColours[font - FirstTierFont]
            : font is >= (int)TextColour.Red and <= (int)TextColour.Blue ? (TextColour)font : TextColour.Yellow;
}
