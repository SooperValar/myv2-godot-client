using System.Globalization;
using System.Linq;

namespace TheLastGate.Profiling;

public static class HitchLog
{
    public const double ThresholdMilliseconds = 50;
    private const int SectionsShown = 5;

    public static string? Describe(FrameSummary frame)
    {
        if (frame.FrameMilliseconds < ThresholdMilliseconds)
        {
            return null;
        }

        var sections = frame.Sections
            .Take(SectionsShown)
            .Select(section => string.Create(
                CultureInfo.InvariantCulture,
                $"{section.Name} {section.Milliseconds:F1}"));
        return string.Create(CultureInfo.InvariantCulture, $"Hitch {frame.FrameMilliseconds:F1} ms: ")
            + string.Join(", ", sections);
    }
}
