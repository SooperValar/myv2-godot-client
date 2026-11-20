using GdUnit4;
using TheLastGate.Session;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Session;

[TestSuite]
public class SessionStatsTextTests
{
    [TestCase(24000, "24,000")]
    [TestCase(0, "0")]
    [TestCase(-1200, "-1,200")]
    public void Amount_GroupsThousands(long amount, string expected)
    {
        AssertThat(SessionStatsText.Amount(Currency.Experience, amount)).IsEqual(expected);
    }

    [TestCase(6600, "66G  0S")]
    [TestCase(-250, "-2G 50S")]
    public void Amount_ShowsGoldAsMoneyWithOneSignInFront(long amount, string expected)
    {
        AssertThat(SessionStatsText.Amount(Currency.Gold, amount)).IsEqual(expected);
    }

    [TestCase(Currency.Tokens, 3, "+3")]
    [TestCase(Currency.Tokens, 0, "0")]
    [TestCase(Currency.Tokens, -3, "-3")]
    [TestCase(Currency.Gold, 6600, "+66G  0S")]
    public void Change_MarksGainsWithAPlus(Currency currency, long amount, string expected)
    {
        AssertThat(SessionStatsText.Change(currency, amount)).IsEqual(expected);
    }

    [TestCase]
    public void Elapsed_ShowsHoursMinutesAndSeconds()
    {
        AssertThat(SessionStatsText.Elapsed(new TimeSpan(1, 30, 5))).IsEqual("1:30:05");
        AssertThat(SessionStatsText.Elapsed(TimeSpan.FromHours(26))).IsEqual("26:00:00");
    }
}
