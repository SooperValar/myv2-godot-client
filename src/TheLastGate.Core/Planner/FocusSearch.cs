using System;
using System.Collections.Generic;

namespace TheLastGate.Planner;

// Something the focus may put points into, by its stat raise slot: 0-4 an attribute, 5 Hitpoints, 7 Mana, 8 and up a
// skill.
public sealed class FocusCandidate
{
    public int RaiseIndex { get; set; }

    public int MostPoints { get; set; }

    // The cost of the 1st..nth extra point; NoCost when the stat is capped.
    public int[] StepCosts { get; } = new int[FocusSearch.MostPoints];
}

public sealed record FocusRaise(int RaiseIndex, int Points, int Cost);

// The raises are in raise slot order.
public sealed record FocusRoute(int TotalCost, IReadOnlyList<FocusRaise> Raises);

public static class FocusSearch
{
    public const int MostCandidates = 15;
    public const int MostPoints = 16;
    public const int MostStates = 65536;

    public static FocusRoute? Cheapest(
        IReadOnlyList<FocusCandidate> candidates,
        uint allowedCandidates,
        Func<int[], bool> accept) =>
        throw new NotImplementedException();
}
