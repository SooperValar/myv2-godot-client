using GdUnit4;
using TheLastGate.Windows;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Windows;

[TestSuite]
public class WindowPauseTests
{
    private WindowPause pause = new();

    [BeforeTest]
    public void StartWithoutAPause() => pause = new WindowPause();

    [TestCase]
    public void Active_IsFalseAtFirst()
    {
        AssertThat(pause.Active).IsFalse();
    }

    [TestCase]
    public void Start_KeepsThePauseForTwentyFourTicks()
    {
        pause.Start();

        for (var tick = 1; tick < WindowPause.Ticks; tick++)
        {
            pause.EndOfTick();
        }

        AssertThat(pause.Active).IsTrue();
        pause.EndOfTick();
        AssertThat(pause.Active).IsFalse();
    }

    [TestCase]
    public void Start_BeginsTheTwentyFourTicksAgain()
    {
        pause.Start();
        pause.EndOfTick();

        pause.Start();
        for (var tick = 1; tick < WindowPause.Ticks; tick++)
        {
            pause.EndOfTick();
        }

        AssertThat(pause.Active).IsTrue();
    }

    [TestCase]
    public void Cancel_EndsThePause()
    {
        pause.Start();

        pause.Cancel();

        AssertThat(pause.Active).IsFalse();
    }
}
