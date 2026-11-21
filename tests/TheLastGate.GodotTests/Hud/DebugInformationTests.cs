using GdUnit4;
using TheLastGate.Assets;
using TheLastGate.Hud;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Hud;

[TestSuite]
public class DebugInformationTests
{
    private static readonly DebugFacts Facts = new(
        "1.4.2+5e0c1d7",
        0x010203,
        "4.7.2-stable (official)",
        "NVIDIA GeForce RTX 4090",
        "NVIDIA",
        "3.3.0 NVIDIA 610.88",
        5,
        3,
        16.66);

    [TestCase]
    public void Lines_ShowTheClientVersionThenWhatTheGodotClientShowedInGreenAfterABlankLine()
    {
        var lines = DebugInformation.Lines(Facts);

        AssertThat(lines.Select(line => line.Text))
            .ContainsExactly(
                " ",
                "Client Version 1.4.2",
                "Networking Version 0.15.00",
                "Server Version 1.02.03",
                "Godot Version 4.7.2-stable (official)",
                "Renderer: NVIDIA GeForce RTX 4090 (NVIDIA)",
                "Graphics API Version: 3.3.0 NVIDIA 610.88",
                "Tick queue 5, target 3, period 16.7 ms");
        AssertThat(lines.Select(line => line.Font).Distinct()).ContainsExactly((int)TextColour.Green);
    }
}
