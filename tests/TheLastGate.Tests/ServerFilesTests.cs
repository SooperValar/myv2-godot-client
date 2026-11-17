using TheLastGate.Map;

namespace TheLastGate.Tests;

public sealed class ServerFilesTests : IDisposable
{
    private readonly string folder = Directory.CreateTempSubdirectory("server-files-").FullName;

    public void Dispose() => Directory.Delete(folder, true);

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

    [Fact]
    public void SdlClientPath_IsInTheCurrentClientsSettingsFolder()
    {
        var expected = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TheLastGate",
            "TheLastGate",
            "TLGMap_tlg.abyss.codes_5555.dat");

        Assert.Equal(expected, ServerFiles.SdlClientPath("TLGMap_", "tlg.abyss.codes", 5555));
    }

    [Fact]
    public void PathToLoad_KeepsToTheOwnFileOnceThereIsOne()
    {
        var own = Write("own.dat");
        var sdlClient = Write("sdl.dat");

        Assert.Equal(own, ServerFiles.PathToLoad(own, sdlClient));
    }

    [Fact]
    public void PathToLoad_ImportsTheCurrentClientsFileWhenThereIsNoOwnFile()
    {
        var own = Path.Combine(folder, "own.dat");
        var sdlClient = Write("sdl.dat");

        Assert.Equal(sdlClient, ServerFiles.PathToLoad(own, sdlClient));
    }

    [Fact]
    public void PathToLoad_FallsBackToTheOwnFileWhenNeitherExists()
    {
        var own = Path.Combine(folder, "own.dat");

        Assert.Equal(own, ServerFiles.PathToLoad(own, Path.Combine(folder, "sdl.dat")));
    }

    private string Write(string name)
    {
        var path = Path.Combine(folder, name);
        File.WriteAllText(path, name);
        return path;
    }
}
