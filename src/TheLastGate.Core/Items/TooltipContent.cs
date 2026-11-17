using System;
using System.Collections.Generic;
using TheLastGate.Assets;

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
    public static IReadOnlyList<TooltipPart> For(ItemTooltip tooltip) => throw new NotImplementedException();
}
