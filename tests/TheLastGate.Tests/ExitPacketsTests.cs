using TheLastGate.Net;

namespace TheLastGate.Tests;

public class ExitPacketsTests
{
    [Fact]
    public void Exit_SendsTheExitCommand()
    {
        byte[] expected = [31, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

        Assert.Equal(expected, ExitPackets.Exit());
    }

    [Theory]
    [InlineData(ExitReasons.EnteredTavern, "You entered a Tavern.")]
    [InlineData(ExitReasons.UserRequest, "Aborting on user request.")]
    public void TheNamedReasons_AreTheOnesTheServerDescribes(int code, string reason)
    {
        Assert.Equal(reason, ExitReasons.Describe((uint)code));
    }
}
