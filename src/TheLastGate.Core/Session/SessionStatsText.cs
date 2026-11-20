using System;
using System.Globalization;
using TheLastGate.Hud;

namespace TheLastGate.Session;

public static class SessionStatsText
{
    public static string Amount(Currency currency, long amount)
    {
        var sign = amount < 0 ? "-" : "";
        var size = Math.Abs(amount);
        return currency == Currency.Gold
            ? sign + Currencies.MoneyText((int)size)
            : sign + size.ToString("N0", CultureInfo.InvariantCulture);
    }

    public static string Change(Currency currency, long amount) =>
        (amount > 0 ? "+" : "") + Amount(currency, amount);

    public static string Elapsed(TimeSpan elapsed) =>
        string.Format(
            CultureInfo.InvariantCulture,
            "{0}:{1:00}:{2:00}",
            (int)elapsed.TotalHours,
            elapsed.Minutes,
            elapsed.Seconds);
}
