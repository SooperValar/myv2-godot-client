using System;

namespace TheLastGate.Hud;

public static class PortraitBars
{
    private const int Length = 62;
    private const int LongestCurrent = Length * 2;

    public static BarWidths For(int current, int maximum, int reservedPercent)
    {
        var reserved = ReservedBar.Width(Length, reservedPercent);
        if (maximum <= 0)
        {
            return new BarWidths(0, reserved, 0);
        }

        return new BarWidths(Length, reserved, Math.Clamp(current * Length / maximum, 0, LongestCurrent));
    }
}
