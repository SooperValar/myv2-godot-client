using TheLastGate.Net;

namespace TheLastGate.Tests;

public class StatPacketsTests
{
    [Fact]
    public void Raise_SendsUpTo900PointsACommandBeforeTheSkills()
    {
        byte[] first = [10, 3, 0, 132, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
        byte[] rest = [10, 3, 0, 100, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

        Assert.Equal([first, rest], StatPackets.Raise(3, 1000));
    }

    [Fact]
    public void Raise_SendsUpTo135PointsACommandForASkill()
    {
        byte[] full = [10, 9, 0, 135, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
        byte[] rest = [10, 9, 0, 30, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

        Assert.Equal([full, full, rest], StatPackets.Raise(StatPackets.FirstSkillSlot + 1, 300));
    }

    [Fact]
    public void Raise_SendsNothingForNoPoints()
    {
        Assert.Empty(StatPackets.Raise(3, 0));
    }
}
