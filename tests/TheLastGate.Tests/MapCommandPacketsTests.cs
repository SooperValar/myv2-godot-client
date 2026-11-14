using TheLastGate.Net;

namespace TheLastGate.Tests;

public class MapCommandPacketsTests
{
    [Fact]
    public void Move_PutsTheTilesWorldPositionAfterTheCommand()
    {
        byte[] expected = [(byte)ClientCommand.Move, 0xD2, 0x04, 0x70, 0x11, 0x01, 0x00, 0, 0, 0, 0, 0, 0, 0, 0, 0];

        Assert.Equal(expected, MapCommandPackets.Move(1234, 70000));
    }

    [Fact]
    public void Turn_UsesTheSameLayoutWithTheTurnCommand()
    {
        byte[] expected = [(byte)ClientCommand.Turn, 0x0A, 0x00, 0x14, 0x00, 0x00, 0x00, 0, 0, 0, 0, 0, 0, 0, 0, 0];

        Assert.Equal(expected, MapCommandPackets.Turn(10, 20));
    }
}
