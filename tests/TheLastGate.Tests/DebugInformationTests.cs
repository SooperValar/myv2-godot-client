using TheLastGate.Assets;
using TheLastGate.Hud;
using TheLastGate.State;

namespace TheLastGate.Tests;

public class DebugInformationTests
{
    private static readonly DebugFacts Facts = new(
        0x010203,
        "4.7.2-stable (official)",
        "NVIDIA GeForce RTX 4090",
        "NVIDIA",
        "3.3.0 NVIDIA 610.88",
        5,
        3,
        16.66);

    [Fact]
    public void Lines_ShowWhatTheGodotClientShowedInGreenAfterABlankLine()
    {
        Assert.Equal(
            [
                " ",
                "Networking Version 0.15.00",
                "Server Version 1.02.03",
                "Godot Version 4.7.2-stable (official)",
                "Renderer: NVIDIA GeForce RTX 4090 (NVIDIA)",
                "Graphics API Version: 3.3.0 NVIDIA 610.88",
                "Tick queue 5, target 3, period 16.7 ms",
            ],
            DebugInformation.Lines(Facts).Select(line => line.Text));
        Assert.All(DebugInformation.Lines(Facts), line => Assert.Equal((int)TextColour.Green, line.Font));
    }

    [Theory]
    [InlineData(0x000F00u, "0.15.00")]
    [InlineData(0x0A0B0Cu, "10.11.12")]
    [InlineData(0u, "0.00.00")]
    public void VersionText_SplitsTheVersionIntoItsThreeBytes(uint version, string text)
    {
        Assert.Equal(text, DebugInformation.VersionText(version));
    }

    [Fact]
    public void WindowReset_ResetsAWindowedGameToTheVirtualSizeAndSaysSo()
    {
        Assert.Equal(
            new LogLine("Game window reset to default resolution.", (int)TextColour.Green),
            WindowReset.Line(true));
        Assert.Equal((1280, 720), WindowReset.Size);
    }

    [Fact]
    public void WindowReset_LeavesAFullScreenGameAlone()
    {
        Assert.Null(WindowReset.Line(false));
    }
}
