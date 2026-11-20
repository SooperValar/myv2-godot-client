using GdUnit4;
using TheLastGate.Assets;
using TheLastGate.Hud;
using TheLastGate.State;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Hud;

[TestSuite]
public class WindowResetTests
{
    [TestCase]
    public void Line_ResetsAWindowedGameToTheVirtualSizeAndSaysSo()
    {
        AssertThat(WindowReset.Line(true))
            .IsEqual(new LogLine("Game window reset to default resolution.", (int)TextColour.Green));
        AssertThat(WindowReset.Size).IsEqual((1280, 720));
    }

    [TestCase]
    public void Line_LeavesAFullScreenGameAlone() => AssertThat(WindowReset.Line(false)).IsNull();
}
