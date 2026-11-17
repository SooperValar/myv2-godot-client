using System;
using System.Globalization;

namespace TheLastGate.World;

// The current client's gamma ran from 1.00 to 1.20 in steps of 0.05; the sliders go there in hundredths.
public static class Gamma
{
    public const float Normal = 1f;
    public const float Lowest = 1f;
    public const float Highest = 1.2f;
    public const float Step = 0.01f;

    public static float Clamp(float gamma) => float.IsNaN(gamma) ? Normal : Math.Clamp(gamma, Lowest, Highest);

    public static string Text(float gamma) => gamma.ToString("0.00", CultureInfo.InvariantCulture);
}
