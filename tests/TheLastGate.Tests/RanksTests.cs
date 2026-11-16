using TheLastGate.Hud;

namespace TheLastGate.Tests;

public class RanksTests
{
    [Theory]
    [InlineData(0, "Private", 10)]
    [InlineData(249, "Private", 10)]
    [InlineData(250, "Private First Class", 11)]
    [InlineData(1750, "Lance Corporal", 12)]
    [InlineData(750749, "Sergeant Major", 18)]
    [InlineData(750750, "Second Lieutenant", 19)]
    [InlineData(44274999, "Field Marshal", 29)]
    [InlineData(44275000, "Knight", 30)]
    [InlineData(57557500, "Baron", 6)]
    [InlineData(74002500, "Earl", 7)]
    [InlineData(118754999, "Marquess", 8)]
    [InlineData(118755000, "Warlord", 9)]
    [InlineData(int.MaxValue, "Warlord", 9)]
    public void For_FindsTheRankForTheTotalPoints(int points, string name, int badgeSprite)
    {
        Assert.Equal(new Rank(name, badgeSprite), Ranks.For(points));
    }

    [Fact]
    public void For_TreatsNegativePointsAsTheLowestRank()
    {
        Assert.Equal(new Rank("Private", 10), Ranks.For(-1));
    }
}
