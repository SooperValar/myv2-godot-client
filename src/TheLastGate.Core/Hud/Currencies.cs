using System.Globalization;

namespace TheLastGate.Hud;

public static class Currencies
{
    private const int SilverPerGold = 100;
    private const string HiddenName = "???";

    public static string MoneyText(int gold) =>
        string.Format(CultureInfo.InvariantCulture, "{0}G {1,2}S", gold / SilverPerGold, gold % SilverPerGold);

    public static string RowName(string name, int points) => points == 0 ? HiddenName : name;
}
