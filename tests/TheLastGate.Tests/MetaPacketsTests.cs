using TheLastGate.Net;

namespace TheLastGate.Tests;

public class MetaPacketsTests
{
    [Fact]
    public void Request_SendsTheMetaCommand()
    {
        byte[] expected = [40, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

        Assert.Equal(expected, MetaPackets.Request());
    }
}
