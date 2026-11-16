using System.Text;
using TheLastGate.Net;

namespace TheLastGate.Tests;

public class ChatPacketsTests
{
    private static readonly ClientCommand[] Chunks =
    [
        ClientCommand.Input1, ClientCommand.Input2, ClientCommand.Input3, ClientCommand.Input4,
        ClientCommand.Input5, ClientCommand.Input6, ClientCommand.Input7, ClientCommand.Input8,
    ];

    [Fact]
    public void Say_SendsEightChunksOfFifteenLetters()
    {
        var packets = ChatPackets.Say("/tavern");

        Assert.Equal(Chunks.Length, packets.Count);
        for (var chunk = 0; chunk < Chunks.Length; chunk++)
        {
            Assert.Equal(Protocol.PacketSize, packets[chunk].Length);
            Assert.Equal((byte)Chunks[chunk], packets[chunk][0]);
        }

        Assert.Equal("/tavern", Encoding.ASCII.GetString(packets[0], 1, 7));
        Assert.All(packets[0][8..], value => Assert.Equal(0, value));
        Assert.All(packets.Skip(1), packet => Assert.All(packet[1..], value => Assert.Equal(0, value)));
    }

    [Fact]
    public void Say_CarriesLongTextIntoTheNextChunk()
    {
        var packets = ChatPackets.Say("abcdefghijklmnopqrst");

        Assert.Equal("abcdefghijklmno", Encoding.ASCII.GetString(packets[0], 1, 15));
        Assert.Equal("pqrst", Encoding.ASCII.GetString(packets[1], 1, 5));
        Assert.Equal(0, packets[1][6]);
    }

    [Fact]
    public void Say_StopsAtOneHundredAndTwentyLetters()
    {
        var packets = ChatPackets.Say(new string('x', 130));

        Assert.All(packets, packet => Assert.All(packet[1..], value => Assert.Equal((byte)'x', value)));
    }
}
