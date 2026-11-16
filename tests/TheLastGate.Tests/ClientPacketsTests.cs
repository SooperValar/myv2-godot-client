using TheLastGate.Net;

namespace TheLastGate.Tests;

public class ClientPacketsTests
{
    [Fact]
    public void ThreeValues_WritesTheCommandAndThreeLittleEndianValues()
    {
        byte[] expected = [38, 7, 0, 0, 0, 12, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0];

        Assert.Equal(expected, ClientPackets.ThreeValues(ClientCommand.QuickShop, 7, 12, -1));
    }
}
