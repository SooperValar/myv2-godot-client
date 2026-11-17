using TheLastGate.Map;

namespace TheLastGate.Tests;

public class ServerFilesTests
{
    [Fact]
    public void Name_JoinsThePrefixHostAndPort()
    {
        Assert.Equal("TLGMap_play.example.com_5555.dat", ServerFiles.Name("TLGMap_", "play.example.com", 5555));
    }

    [Fact]
    public void Name_ReplacesCharactersAFileNameCanNotHold()
    {
        Assert.Equal("TLGMap_bad_host_name_x_1.dat", ServerFiles.Name("TLGMap_", "bad:host/name\\x", 1));
    }
}
