using TheLastGate.State;

namespace TheLastGate.Tests;

public class StatusSpeedTests
{
    [Theory]
    [InlineData(0, 30)]
    [InlineData(9, 21)]
    [InlineData(10, 20)]
    [InlineData(20, 10)]
    [InlineData(29, 1)]
    public void StepsThisTick_GivesEachSpeedItsShareOfTheCycle(int speed, int framesPerCycle)
    {
        var total = 0;
        for (var serverTick = 0; serverTick < StatusSpeed.CycleLength; serverTick++)
        {
            total += StatusSpeed.StepsThisTick(speed, serverTick);
        }

        Assert.Equal(framesPerCycle, total);
    }

    [Theory]
    [InlineData(0, 0, 2)]
    [InlineData(0, 1, 1)]
    [InlineData(11, 9, 0)]
    [InlineData(11, 10, 1)]
    [InlineData(29, 9, 1)]
    [InlineData(29, 8, 0)]
    public void StepsThisTick_FollowsTheServersTable(int speed, int serverTick, int steps)
    {
        Assert.Equal(steps, StatusSpeed.StepsThisTick(speed, serverTick));
    }

    [Fact]
    public void StepsThisTick_KeepsSpeedsAndTicksInRange()
    {
        Assert.Equal(StatusSpeed.StepsThisTick(29, 9), StatusSpeed.StepsThisTick(40, 29));
        Assert.Equal(StatusSpeed.StepsThisTick(0, 3), StatusSpeed.StepsThisTick(-5, 3));
    }
}
