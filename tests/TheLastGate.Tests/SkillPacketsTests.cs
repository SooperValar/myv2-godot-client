using TheLastGate.Net;

namespace TheLastGate.Tests;

public class SkillPacketsTests
{
    [Fact]
    public void Use_SendsTheSkillAndTheTarget()
    {
        byte[] expected = [26, 30, 0, 0, 0, 210, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0];

        Assert.Equal(expected, SkillPackets.Use(30, 1234));
    }
}
