using TheLastGate.Net;

namespace TheLastGate.Tests;

public class DepotPacketsTests
{
    private const int DepotNumber = 1210;

    [Fact]
    public void Take_SendsTheDepotAndTheSlot()
    {
        byte[] expected = [25, 186, 4, 138, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

        Assert.Equal(expected, DepotPackets.Take(DepotNumber, 138));
    }

    [Fact]
    public void Describe_SendsTheSlotPastEveryPage()
    {
        byte[] expected = [25, 186, 4, 255, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

        Assert.Equal(expected, DepotPackets.Describe(DepotNumber, 511));
    }
}
