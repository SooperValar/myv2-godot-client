using System;

namespace TheLastGate.Hud;

// Reserved hit points, endurance or mana show as a part at the right end of a bar, rounded up by a pixel as the
// current client's portrait bars round it.
public static class ReservedBar
{
    public static int Width(int barLength, int reservedPercent) =>
        reservedPercent > 0 ? Math.Min(barLength, barLength * reservedPercent / 100 + 1) : 0;
}
