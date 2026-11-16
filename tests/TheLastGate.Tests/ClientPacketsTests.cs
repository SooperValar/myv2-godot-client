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

    [Fact]
    public void ShortAndInt_WritesASixteenBitThenAThirtyTwoBitValue()
    {
        byte[] expected = [8, 2, 0, 7, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

        Assert.Equal(expected, ClientPackets.ShortAndInt(ClientCommand.Mode, 2, 7));
    }

    [Fact]
    public void OneValue_WritesTheCommandAndOneLittleEndianValue()
    {
        byte[] expected = [31, 5, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

        Assert.Equal(expected, ClientPackets.OneValue(ClientCommand.Exit, 261));
    }
}
