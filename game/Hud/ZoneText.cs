using System;

namespace TheLastGate.Hud;

// The MyV2 server names every zone the player stands in, outermost first ("Aston, Market"). When that's too long for
// the bar above the minimap, the innermost zone is the one worth showing.
public static class ZoneText
{
    public const int MostLetters = 20;

    private const string Separator = ", ";

    public static string Fitted(string zones)
    {
        if (zones.Length <= MostLetters)
        {
            return zones;
        }

        var innermost = zones[(zones.LastIndexOf(Separator, StringComparison.Ordinal) + 1)..].Trim();
        return innermost.Length <= MostLetters ? innermost : innermost[..MostLetters];
    }
}
