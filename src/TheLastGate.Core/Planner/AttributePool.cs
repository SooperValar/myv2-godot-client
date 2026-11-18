using System;
using System.Collections.Generic;

namespace TheLastGate.Planner;

public sealed record AttributePoolPick(int TotalCost, IReadOnlyList<int> Points);

public static class AttributePool
{
    public const int MostPoints = 20;
    public const int NoCost = -1;

    public static AttributePoolPick? Cheapest(int[][] stepCosts, int pointCount) =>
        throw new NotImplementedException();
}
