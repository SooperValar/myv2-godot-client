using TheLastGate.Net;

namespace TheLastGate.Tests;

public class StatPacketsTests
{
    [Fact]
    public void Raise_SendsUpTo99RaisesACommand()
    {
        byte[] full = [10, 9, 0, 99, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
        byte[] rest = [10, 9, 0, 52, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

        Assert.Equal([full, full, rest], StatPackets.Raise(StatPackets.FirstSkillSlot + 1, 250));
    }

    [Fact]
    public void Raise_SendsNothingForNoPoints()
    {
        Assert.Empty(StatPackets.Raise(3, 0));
    }
}
