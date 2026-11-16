using TheLastGate.Net;

namespace TheLastGate.Tests;

public class InventoryPacketsTests
{
    [Fact]
    public void For_WritesTheActionValueAndSelectedCharacter()
    {
        byte[] expected = [9, 5, 0, 0, 0, 3, 0, 0, 0, 210, 4, 0, 0, 0, 0, 0];

        Assert.Equal(expected, InventoryPackets.For(InventoryAction.UseGear, 3, 1234));
    }
}
