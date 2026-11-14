using TheLastGate.Characters;

namespace TheLastGate.Tests;

public class NulTerminatedTextTests
{
    [Fact]
    public void Read_StopsAtTheFirstNul()
    {
        byte[] field = [(byte)'A', (byte)'b', (byte)'c', 0, (byte)'x', (byte)'y'];

        Assert.Equal("Abc", NulTerminatedText.Read(field));
    }

    [Fact]
    public void Read_UsesTheWholeFieldWithoutANul()
    {
        byte[] field = [(byte)'A', 0xE9];

        Assert.Equal("Aé", NulTerminatedText.Read(field));
    }

    [Fact]
    public void Write_ClearsTheFieldAndKeepsRoomForTheNul()
    {
        var field = new byte[] { 9, 9, 9, 9, 9 };

        NulTerminatedText.Write(field, "abcdefg");

        Assert.Equal(new byte[] { (byte)'a', (byte)'b', (byte)'c', (byte)'d', 0 }, field);
    }
}
