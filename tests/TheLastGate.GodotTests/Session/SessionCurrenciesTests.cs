using GdUnit4;
using TheLastGate.Net;
using TheLastGate.Session;
using TheLastGate.State;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Session;

[TestSuite]
public class SessionCurrenciesTests
{
    private TickProcessor processor = null!;
    private SessionCurrencies session = new();
    private GameState state = new();

    [BeforeTest]
    public void StartASession()
    {
        state = new GameState();
        session = new SessionCurrencies();
        processor = new TickProcessor(state);
    }

    [TestCase]
    public void Observe_WaitsForTheNameBeforeTakingTheStart()
    {
        Tick(TimeSpan.Zero, Gold(500));

        AssertThat(session.Started).IsFalse();
    }

    [TestCase]
    public void Observe_StartsFromTheLoginBurstWithoutCountingItAsEarned()
    {
        Tick(TimeSpan.Zero, Name(), Gold(500), Points(1000, 90000));

        AssertThat(Row(Currency.Gold).Start).IsEqual(500);
        AssertThat(Row(Currency.Gold).Earned).IsEqual(0);
        AssertThat(Row(Currency.Experience).Start).IsEqual(1000);
        AssertThat(Row(Currency.Experience).Earned).IsEqual(0);
    }

    [TestCase]
    public void Observe_AddsRisesToEarnedAndFallsToSpent()
    {
        Tick(TimeSpan.Zero, Name(), Gold(500));
        Tick(TimeSpan.FromMinutes(1), Gold(800));
        Tick(TimeSpan.FromMinutes(2), Gold(600));

        var gold = Row(Currency.Gold);
        AssertThat(gold.Earned).IsEqual(300);
        AssertThat(gold.Spent).IsEqual(200);
        AssertThat(gold.Net).IsEqual(100);
    }

    [TestCase]
    public void Observe_CountsRaisingAStatAsSpentExperience()
    {
        Tick(TimeSpan.Zero, Name(), Points(1000, 90000));
        Tick(TimeSpan.FromMinutes(1), Points(1400, 90400));
        Tick(TimeSpan.FromMinutes(2), Points(200, 90400));

        var experience = Row(Currency.Experience);
        AssertThat(experience.Earned).IsEqual(400);
        AssertThat(experience.Spent).IsEqual(1200);
        AssertThat(experience.Net).IsEqual(-800);
    }

    [TestCase]
    public void Observe_SplitsGainAndRaiseInTheSameUpdate()
    {
        Tick(TimeSpan.Zero, Name(), Points(1000, 90000));
        Tick(TimeSpan.FromMinutes(1), Points(1050, 90100));

        AssertThat(Row(Currency.Experience).Earned).IsEqual(100);
        AssertThat(Row(Currency.Experience).Spent).IsEqual(50);
    }

    [TestCase]
    public void PerHour_SpreadsOverTheWholeSession()
    {
        Tick(TimeSpan.FromHours(1), Name(), Gold(0));
        Tick(TimeSpan.FromHours(1.5), Gold(300));
        Tick(TimeSpan.FromHours(3));

        AssertThat(Math.Round(Row(Currency.Gold).EarnedPerHour(session.Elapsed), 6)).IsEqual(150.0);
    }

    [TestCase]
    public void Reset_StartsAgainFromTheCurrentAmounts()
    {
        Tick(TimeSpan.Zero, Name(), Gold(500));
        Tick(TimeSpan.FromMinutes(1), Gold(800));

        Tick(TimeSpan.FromMinutes(2));
        session.Reset(state.Player);

        AssertThat(Row(Currency.Gold).Start).IsEqual(800);
        AssertThat(Row(Currency.Gold).Earned).IsEqual(0);
        Tick(TimeSpan.FromMinutes(3));
        AssertThat(session.Elapsed).IsEqual(TimeSpan.FromMinutes(1));
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
}
