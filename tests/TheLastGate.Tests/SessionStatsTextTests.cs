using TheLastGate.Session;

namespace TheLastGate.Tests;

public class SessionStatsTextTests
{
    [Theory]
    [InlineData(24000, "24,000")]
    [InlineData(0, "0")]
    [InlineData(-1200, "-1,200")]
    public void Amount_GroupsThousands(long amount, string expected)
    {
        Assert.Equal(expected, SessionStatsText.Amount(Currency.Experience, amount));
    }

    [Theory]
    [InlineData(6600, "66G  0S")]
    [InlineData(-250, "-2G 50S")]
    public void Amount_ShowsGoldAsMoneyWithOneSignInFront(long amount, string expected)
    {
        Assert.Equal(expected, SessionStatsText.Amount(Currency.Gold, amount));
    }

    [Theory]
    [InlineData(Currency.Tokens, 3, "+3")]
    [InlineData(Currency.Tokens, 0, "0")]
    [InlineData(Currency.Tokens, -3, "-3")]
    [InlineData(Currency.Gold, 6600, "+66G  0S")]
    public void Change_MarksGainsWithAPlus(Currency currency, long amount, string expected)
    {
        Assert.Equal(expected, SessionStatsText.Change(currency, amount));
    }

    [Fact]
    public void Elapsed_ShowsHoursMinutesAndSeconds()
    {
        Assert.Equal("1:30:05", SessionStatsText.Elapsed(new TimeSpan(1, 30, 5)));
        Assert.Equal("26:00:00", SessionStatsText.Elapsed(TimeSpan.FromHours(26)));
    }
}
