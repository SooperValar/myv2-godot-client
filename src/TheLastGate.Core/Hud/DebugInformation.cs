using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TheLastGate.Assets;
using TheLastGate.Net;
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
    public static IReadOnlyList<LogLine> Lines(DebugFacts facts)
    {
        string[] lines =
        [
            " ",
            $"Networking Version {VersionText(Protocol.NetworkingVersion)}",
            $"Server Version {VersionText(facts.ServerVersion)}",
            $"Godot Version {facts.GodotVersion}",
            $"Renderer: {facts.AdapterName} ({facts.AdapterVendor})",
            $"Graphics API Version: {facts.ApiVersion}",
            string.Create(
                CultureInfo.InvariantCulture,
                $"Tick queue {facts.QueuedTicks}, target {facts.TickTarget}, period {facts.TickPeriodMilliseconds:F1} ms"),
        ];
        return lines.Select(line => new LogLine(line, (int)TextColour.Green)).ToList();
    }

    // Versions pack major, minor and patch into the low three bytes.
    public static string VersionText(uint version) =>
        $"{version >> 16}.{(version >> 8) & 255:00}.{version & 255:00}";
}
