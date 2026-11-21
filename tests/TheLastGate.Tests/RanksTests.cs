using TheLastGate.State;

namespace TheLastGate.Tests;

public class RanksTests
{
    [Theory]
    [InlineData(0, "Private")]
    [InlineData(249, "Private")]
    [InlineData(250, "Private First Class")]
    [InlineData(1750, "Lance Corporal")]
    [InlineData(750749, "Sergeant Major")]
    [InlineData(750750, "Second Lieutenant")]
    [InlineData(44274999, "Field Marshal")]
    [InlineData(44275000, "Knight")]
    [InlineData(118754999, "Marquess")]
    [InlineData(118755000, "Warlord")]
    [InlineData(int.MaxValue, "Warlord")]
    [InlineData(-1, "Private")]
    public void Name_IsTheRankForTheTotalPoints(int points, string name)
    {
        Assert.Equal(name, Ranks.Name(points));
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, 0)]
    [InlineData(250, 1)]
    [InlineData(118754999, 23)]
    [InlineData(118755000, 24)]
    [InlineData(int.MaxValue, 24)]
    public void Number_CountsTheRanksPassed(int points, int number)
    {
        Assert.Equal(number, Ranks.Number(points));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(125, 0.5)]
    [InlineData(250, 0)]
    [InlineData(1000, 0.5)]
    [InlineData(94185000, 0)]
    [InlineData(106470000, 0.5)]
    [InlineData(118755000, 1)]
    [InlineData(int.MaxValue, 1)]
    public void Progress_IsHowFarThePointsAreFromThisRankToTheNext(int points, double progress)
    {
        Assert.Equal(progress, Ranks.Progress(points), 6);
    }

    [Fact]
    public void Progress_TreatsNegativePointsAsNone()
    {
        Assert.Equal(0, Ranks.Progress(-1));
    }
}
