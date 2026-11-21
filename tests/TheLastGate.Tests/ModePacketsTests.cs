using TheLastGate.Net;
using TheLastGate.State;

namespace TheLastGate.Tests;

public class ModePacketsTests
{
    [Theory]
    [InlineData(SpeedMode.Slow, 0)]
    [InlineData(SpeedMode.Normal, 1)]
    [InlineData(SpeedMode.Fast, 2)]
    public void For_SendsTheSpeedAsAShort(SpeedMode mode, byte value)
    {
        byte[] expected = [8, value, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

        Assert.Equal(expected, ModePackets.For(mode));
    }
}
