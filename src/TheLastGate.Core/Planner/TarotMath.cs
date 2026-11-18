using System;
using System.Collections.Generic;

namespace TheLastGate.Planner;

// A card's number is its bit in a tarot set; only these cards change anything the planner shows.
public enum TarotCard
{
    Magician = 1,
    Lovers = 6,
    Hermit = 9,
    FoolReversed = 22,
    LoversReversed = 28,
    StrengthReversed = 30,
    HangedReversed = 34,
    TemperanceReversed = 36,
    StarReversed = 39,
    MoonReversed = 40,
}

public static class TarotMath
{
    public const int CardCount = 44;
    public const int TemperanceSicknessStacks = 4;
    public const int FoolPointsPerBreakpoint = 5;

    public static IReadOnlyList<TarotCard> ModelledCards => throw new NotImplementedException();

    public static bool Has(ulong cards, int cardIndex) => throw new NotImplementedException();

    public static bool Has(ulong cards, TarotCard card) => throw new NotImplementedException();

    public static int FloorDivide(int dividend, int divisor) => throw new NotImplementedException();

    public static int FoolReversedTotal(int shownTotal, int sumChange) => throw new NotImplementedException();

    public static int ScaledChange(int serverValue, int rawChange, int numerator, int denominator) =>
        throw new NotImplementedException();

    public static CombatValues CombatShown(
        ulong cards,
        PlannerEffects effects,
        CombatValues server,
        CombatValues rawChange) =>
        throw new NotImplementedException();
}
