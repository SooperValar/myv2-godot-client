using TheLastGate.Net;
using TheLastGate.Session;
using TheLastGate.State;

namespace TheLastGate.Tests;

public class SessionCurrenciesTests
{
    private readonly TickProcessor processor;
    private readonly SessionCurrencies session = new();
    private readonly GameState state = new();

    public SessionCurrenciesTests() => processor = new TickProcessor(state);

    [Fact]
    public void Observe_WaitsForTheNameBeforeTakingTheStart()
    {
        Tick(TimeSpan.Zero, Gold(500));

        Assert.False(session.Started);
    }

    [Fact]
    public void Observe_StartsFromTheLoginBurstWithoutCountingItAsEarned()
    {
        Tick(TimeSpan.Zero, Name(), Gold(500), Points(1000, 90000));

        Assert.Equal(500, Row(Currency.Gold).Start);
        Assert.Equal(0, Row(Currency.Gold).Earned);
        Assert.Equal(1000, Row(Currency.Experience).Start);
        Assert.Equal(0, Row(Currency.Experience).Earned);
    }

    [Fact]
    public void Observe_TreatsACurrencyNeverSentAsZero()
    {
        Tick(TimeSpan.Zero, Name());
        Tick(TimeSpan.FromMinutes(1), Tokens(3));

        Assert.Equal(0, Row(Currency.Tokens).Start);
        Assert.Equal(3, Row(Currency.Tokens).Earned);
    }

    [Fact]
    public void Observe_AddsRisesToEarnedAndFallsToSpent()
    {
        Tick(TimeSpan.Zero, Name(), Gold(500));
        Tick(TimeSpan.FromMinutes(1), Gold(800));
        Tick(TimeSpan.FromMinutes(2), Gold(600));

        var gold = Row(Currency.Gold);
        Assert.Equal(300, gold.Earned);
        Assert.Equal(200, gold.Spent);
        Assert.Equal(100, gold.Net);
    }

    [Fact]
    public void Observe_CountsRaisingAStatAsSpentExperience()
    {
        Tick(TimeSpan.Zero, Name(), Points(1000, 90000));
        Tick(TimeSpan.FromMinutes(1), Points(1400, 90400));
        Tick(TimeSpan.FromMinutes(2), Points(200, 90400));

        var experience = Row(Currency.Experience);
        Assert.Equal(400, experience.Earned);
        Assert.Equal(1200, experience.Spent);
        Assert.Equal(-800, experience.Net);
    }

    [Fact]
    public void Observe_SplitsGainAndRaiseInTheSameUpdate()
    {
        Tick(TimeSpan.Zero, Name(), Points(1000, 90000));
        Tick(TimeSpan.FromMinutes(1), Points(1050, 90100));

        Assert.Equal(100, Row(Currency.Experience).Earned);
        Assert.Equal(50, Row(Currency.Experience).Spent);
    }

    [Fact]
    public void PerHour_SpreadsOverTheWholeSession()
    {
        Tick(TimeSpan.FromHours(1), Name(), Gold(0));
        Tick(TimeSpan.FromHours(1.5), Gold(300));
        Tick(TimeSpan.FromHours(3));

        Assert.Equal(150, Row(Currency.Gold).EarnedPerHour(session.Elapsed), 6);
    }

    [Fact]
    public void Reset_StartsAgainFromTheCurrentAmounts()
    {
        Tick(TimeSpan.Zero, Name(), Gold(500));
        Tick(TimeSpan.FromMinutes(1), Gold(800));

        Tick(TimeSpan.FromMinutes(2));
        session.Reset(state.Player);

        Assert.Equal(800, Row(Currency.Gold).Start);
        Assert.Equal(0, Row(Currency.Gold).Earned);
        Tick(TimeSpan.FromMinutes(3));
        Assert.Equal(TimeSpan.FromMinutes(1), session.Elapsed);
    }

    private void Tick(TimeSpan now, params TestCommand[] commands)
    {
        processor.Process(TestCommand.Payload(commands));
        session.Observe(state.Player, now);
    }

    private CurrencyTally Row(Currency currency) => session.Tallies.Single(tally => tally.Currency == currency);

    private static TestCommand Name() => TestCommand.Of(ServerCommand.SetCharacterName1).Text("Ishtar", 15);

    private static TestCommand Gold(uint gold) =>
        TestCommand.Of(ServerCommand.SetCharacterGold).UInt32(gold).UInt32(0).UInt32(0);

    private static TestCommand Points(uint unspent, uint total) =>
        TestCommand.Of(ServerCommand.SetCharacterPoints).UInt32(unspent).UInt32(total).UInt32(0);

    private static TestCommand Tokens(uint tokens) =>
        TestCommand.Of(ServerCommand.SetCharacterTokens).UInt32(tokens).UInt16(0).UInt16(0);
}
