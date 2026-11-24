using TheLastGate.State;

namespace TheLastGate.Tests;

public class StatusSpeedTests
{
    [Theory]
    [InlineData(0, 20)]
    [InlineData(10, 10)]
    [InlineData(19, 1)]
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
    [InlineData(1, 9, 0)]
    [InlineData(1, 10, 1)]
    [InlineData(19, 9, 1)]
    [InlineData(19, 8, 0)]
    public void StepsThisTick_FollowsTheServersTable(int speed, int serverTick, int steps)
    {
        Assert.Equal(steps, StatusSpeed.StepsThisTick(speed, serverTick));
    }

    [Fact]
    public void StepsThisTick_KeepsSpeedsAndTicksInRange()
    {
        Assert.Equal(StatusSpeed.StepsThisTick(19, 9), StatusSpeed.StepsThisTick(40, 29));
        Assert.Equal(StatusSpeed.StepsThisTick(0, 3), StatusSpeed.StepsThisTick(-5, 3));
    }
}
