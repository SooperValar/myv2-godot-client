using System.Collections.Generic;

namespace TheLastGate.Profiling;

public sealed record FrameSummary(double FrameMilliseconds, IReadOnlyList<(string Name, double Milliseconds)> Sections);
