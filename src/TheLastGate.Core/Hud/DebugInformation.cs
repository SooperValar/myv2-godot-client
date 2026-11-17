using System;
using System.Collections.Generic;
using TheLastGate.State;

namespace TheLastGate.Hud;

public sealed record DebugFacts(
    uint ServerVersion,
    string GodotVersion,
    string AdapterName,
    string AdapterVendor,
    string ApiVersion,
    int QueuedTicks,
    int TickTarget,
    double TickPeriodMilliseconds);

public static class DebugInformation
{
    public static IReadOnlyList<LogLine> Lines(DebugFacts facts) => throw new NotImplementedException();

    public static string VersionText(uint version) => throw new NotImplementedException();
}
