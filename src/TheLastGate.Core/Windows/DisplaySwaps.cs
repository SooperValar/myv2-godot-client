using System.Collections.Generic;
using System.Linq;

namespace TheLastGate.Windows;

public enum DisplaySwap
{
    ObviousHoleTraps,
}

public sealed record DisplaySwapRow(
    DisplaySwap Swap,
    string Label,
    string SettingKey,
    IReadOnlyDictionary<int, int> Sprites);

public static class DisplaySwaps
{
    private const int HiddenHoleTrap = 16859;
    private const int ObviousHoleTrap = 34194;

    public static IReadOnlyList<DisplaySwapRow> Rows { get; } =
    [
        new(
            DisplaySwap.ObviousHoleTraps,
            "More obvious hole traps.",
            "obvious_hole_traps",
            new Dictionary<int, int> { [HiddenHoleTrap] = ObviousHoleTrap }),
    ];

    public static IReadOnlyDictionary<int, int> SpritesFor(IEnumerable<DisplaySwap> switchedOn)
    {
        var sprites = new Dictionary<int, int>();
        foreach (var swap in switchedOn)
        {
            foreach (var (shown, drawn) in Rows.Single(row => row.Swap == swap).Sprites)
            {
                sprites[shown] = drawn;
            }
        }

        return sprites;
    }
}
