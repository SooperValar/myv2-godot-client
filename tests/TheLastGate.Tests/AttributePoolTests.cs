using TheLastGate.Planner;
using TheLastGate.State;

namespace TheLastGate.Tests;

public class AttributePoolTests
{
    [Fact]
    public void Cheapest_TakesTheCheapestPointsFirst()
    {
        var costs = Costs(30);
        for (var step = 0; step < AttributePool.MostPoints; step++)
        {
            costs[3][step] = step < 3 ? 10 : 50;
        }

        var pick = AttributePool.Cheapest(costs, 5);

        Assert.NotNull(pick);
        Assert.Equal(90, pick.TotalCost);
        Assert.Equal([2, 0, 0, 3, 0], pick.Points);
    }

    [Fact]
    public void Cheapest_SkipsAttributesThatCantBeRaised()
    {
        var costs = Costs(10);
        Array.Fill(costs[0], AttributePool.NoCost);

        var pick = AttributePool.Cheapest(costs, 5);

        Assert.NotNull(pick);
        Assert.Equal(0, pick.Points[0]);
        Assert.Equal(50, pick.TotalCost);
    }

    [Fact]
    public void Cheapest_IsNothingWhenThereArentEnoughPoints()
    {
        var costs = Costs(AttributePool.NoCost);
        costs[1][0] = 10;
        costs[1][1] = 10;
        costs[2][0] = 10;

        Assert.Null(AttributePool.Cheapest(costs, 5));
    }

    private static int[][] Costs(int cost) =>
        Enumerable.Range(0, PlayerState.AttributeCount)
            .Select(_ => Enumerable.Repeat(cost, AttributePool.MostPoints).ToArray())
            .ToArray();
}
