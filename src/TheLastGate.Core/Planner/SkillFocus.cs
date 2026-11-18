using System;
using System.Collections.Generic;

namespace TheLastGate.Planner;

public sealed class SkillFocusInput
{
    // Null marks a slot the skill doesn't use.
    public AttributeSlot?[] Attributes { get; } = new AttributeSlot?[3];

    // The values the preview sums, planned raises included.
    public int[] AttributeValues { get; } = new int[3];

    // The cost of each slot's 1st..6th extra point; a repeated attribute uses its first slot's costs.
    public int[][] AttributeStepCosts { get; } =
        [new int[SkillFocus.MostSteps], new int[SkillFocus.MostSteps], new int[SkillFocus.MostSteps]];

    public int Divisor { get; set; }

    // NoCost when the skill itself is capped.
    public int DirectCost { get; set; }

    // Under the Reverse Fool any attribute point counts, so the route buys the cheapest points across all five.
    public bool FoolReversed { get; set; }

    public int[][] PoolStepCosts { get; } =
    [
        new int[AttributePool.MostPoints], new int[AttributePool.MostPoints], new int[AttributePool.MostPoints],
        new int[AttributePool.MostPoints], new int[AttributePool.MostPoints],
    ];
}

public sealed record SkillFocusRaise(AttributeSlot Attribute, int Points, int Cost);

// The raises are in attribute order; the direct route has none.
public sealed record SkillFocusRoute(int TotalCost, IReadOnlyList<SkillFocusRaise> Raises);

public sealed record SkillFocusSuggestion(SkillFocusRoute? Direct, SkillFocusRoute? Attributes)
{
    public bool UseDirect => throw new NotImplementedException();

    public bool AnyAvailable => throw new NotImplementedException();
}

public static class SkillFocus
{
    public const int MostSteps = SkillPreview.HardModeDivisor;
    public const int NoCost = AttributePool.NoCost;

    public static SkillFocusSuggestion Suggest(SkillFocusInput input) => throw new NotImplementedException();
}
