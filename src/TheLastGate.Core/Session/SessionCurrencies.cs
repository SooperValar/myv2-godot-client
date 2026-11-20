using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TheLastGate.State;

namespace TheLastGate.Session;

public enum Currency
{
    Experience,
    Gold,
    BlackStronghold,
    Osiris,
    Tokens,
}

public sealed record CurrencyTally(Currency Currency, long Start, long Earned, long Spent)
{
    public long Net => Earned - Spent;

    public double EarnedPerHour(TimeSpan elapsed) => PerHour(Earned, elapsed);

    public double NetPerHour(TimeSpan elapsed) => PerHour(Net, elapsed);

    private static double PerHour(long amount, TimeSpan elapsed) =>
        elapsed > TimeSpan.Zero ? amount / elapsed.TotalHours : 0;
}

public sealed class SessionCurrencies
{
    private static readonly Currency[] CurrenciesBesidesExperience =
        [Currency.Gold, Currency.BlackStronghold, Currency.Osiris, Currency.Tokens];

    private readonly Dictionary<Currency, Tally> tallies =
        Enum.GetValues<Currency>().ToDictionary(currency => currency, _ => new Tally());

    private int lastPointsTotal;
    private TimeSpan lastSeenAt;
    private TimeSpan startedAt;

    public bool Started { get; private set; }

    public IEnumerable<CurrencyTally> Tallies =>
        tallies.Select(pair => new CurrencyTally(pair.Key, pair.Value.Start, pair.Value.Earned, pair.Value.Spent));

    public TimeSpan Elapsed => lastSeenAt - startedAt;

    // The server only sends a value once it differs from zero, so a currency that never arrives is really zero. The
    // name comes in the same burst as the currencies at login, so once it is here the whole first burst is too.
    public void Observe(PlayerState player, TimeSpan now)
    {
        lastSeenAt = now;
        if (!Started)
        {
            if (player.Name.Length > 0)
            {
                Reset(player);
            }

            return;
        }

        ObserveExperience(player);
        foreach (var currency in CurrenciesBesidesExperience)
        {
            tallies[currency].Move(Amount(player, currency));
        }
    }

    public void Reset(PlayerState player)
    {
        Started = true;
        startedAt = lastSeenAt;
        lastPointsTotal = player.PointsTotal;
        foreach (var (currency, tally) in tallies)
        {
            tally.Restart(Amount(player, currency));
        }
    }

    // Raising a stat only lowers the unspent points; gaining experience raises both, so what the unspent points fell
    // short of the total's rise was spent.
    private void ObserveExperience(PlayerState player)
    {
        var tally = tallies[Currency.Experience];
        var gained = Math.Max(0, player.PointsTotal - lastPointsTotal);
        var unspentChange = player.Points - tally.Current;
        tally.Add(gained, gained - unspentChange, player.Points);
        lastPointsTotal = player.PointsTotal;
    }

    private static long Amount(PlayerState player, Currency currency) => currency switch
    {
        Currency.Experience => player.Points,
        Currency.Gold => player.Gold,
        Currency.BlackStronghold => player.BlackStrongholdPoints,
        Currency.Osiris => player.OsirisPoints,
        Currency.Tokens => player.Tokens,
        _ => throw new UnreachableException(),
    };

    private sealed class Tally
    {
        public long Start { get; private set; }
        public long Current { get; private set; }
        public long Earned { get; private set; }
        public long Spent { get; private set; }

        public void Restart(long amount)
        {
            Start = amount;
            Current = amount;
            Earned = 0;
            Spent = 0;
        }

        public void Move(long amount)
        {
            var change = amount - Current;
            Add(Math.Max(0, change), Math.Max(0, -change), amount);
        }

        public void Add(long earned, long spent, long amount)
        {
            Earned += earned;
            Spent += spent;
            Current = amount;
        }
    }
}
