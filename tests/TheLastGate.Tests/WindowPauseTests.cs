using TheLastGate.Windows;

namespace TheLastGate.Tests;

public class WindowPauseTests
{
    private readonly WindowPause pause = new();

    [Fact]
    public void Active_IsFalseAtFirst()
    {
        Assert.False(pause.Active);
    }

    [Fact]
    public void Start_KeepsThePauseForTwentyFourTicks()
    {
        pause.Start();

        for (var tick = 1; tick < WindowPause.Ticks; tick++)
        {
            pause.EndOfTick();
        }

        Assert.True(pause.Active);
        pause.EndOfTick();
        Assert.False(pause.Active);
    }

    [Fact]
    public void Start_BeginsTheTwentyFourTicksAgain()
    {
        pause.Start();
        pause.EndOfTick();

        pause.Start();
        for (var tick = 1; tick < WindowPause.Ticks; tick++)
        {
            pause.EndOfTick();
        }

        Assert.True(pause.Active);
    }

    [Fact]
    public void Cancel_EndsThePause()
    {
        pause.Start();

        pause.Cancel();

        Assert.False(pause.Active);
    }
}
