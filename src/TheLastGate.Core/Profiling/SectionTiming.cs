namespace TheLastGate.Profiling;

public sealed record SectionTiming(
    string Name,
    double CallsPerFrame,
    double AverageFrameMilliseconds,
    double PeakFrameMilliseconds,
    double? AverageTickMilliseconds,
    double? PeakTickMilliseconds);
