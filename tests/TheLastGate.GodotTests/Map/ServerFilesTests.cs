using GdUnit4;
using TheLastGate.Map;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Map;

[TestSuite]
public sealed class ServerFilesTests
{
    private string folder = "";

    [BeforeTest]
    public void MakeAFolder() => folder = Directory.CreateTempSubdirectory("server-files-").FullName;

    [AfterTest]
    public void DeleteTheFolder() => Directory.Delete(folder, true);

    [TestCase]
    public void Name_JoinsThePrefixHostAndPort()
    {
        AssertThat(ServerFiles.Name("TLGMap_", "play.example.com", 5555)).IsEqual("TLGMap_play.example.com_5555.dat");
    }

    [TestCase]
    public void Name_ReplacesCharactersAFileNameCanNotHold()
    {
        AssertThat(ServerFiles.Name("TLGMap_", "bad:host/name\\x", 1)).IsEqual("TLGMap_bad_host_name_x_1.dat");
    }

    [TestCase]
    public void SdlClientPath_IsInTheCurrentClientsSettingsFolder()
    {
        AssertThat(ServerFiles.SdlClientPath("TLGMap_", "tlg.abyss.codes", 5555))
            .IsEqual(SdlClientFolder.FilePath("TLGMap_tlg.abyss.codes_5555.dat"));
    }

    [TestCase]
    public void PathToLoad_KeepsToTheOwnFileOnceThereIsOne()
    {
        var own = Write("own.dat");
        var sdlClient = Write("sdl.dat");

        AssertThat(ServerFiles.PathToLoad(own, sdlClient)).IsEqual(own);
    }

    [TestCase]
    public void PathToLoad_ImportsTheCurrentClientsFileWhenThereIsNoOwnFile()
    {
        var own = Path.Combine(folder, "own.dat");
        var sdlClient = Write("sdl.dat");

        AssertThat(ServerFiles.PathToLoad(own, sdlClient)).IsEqual(sdlClient);
    }

    [TestCase]
    public void PathToLoad_FallsBackToTheOwnFileWhenNeitherExists()
    {
        var own = Path.Combine(folder, "own.dat");

        AssertThat(ServerFiles.PathToLoad(own, Path.Combine(folder, "sdl.dat"))).IsEqual(own);
    }

    private string Write(string name)
    {
        var path = Path.Combine(folder, name);
        File.WriteAllText(path, name);
        return path;
    }
}
