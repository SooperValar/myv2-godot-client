using TheLastGate.Net;

namespace TheLastGate.Tests;

public class UniqueIdFileTests
{
    private static readonly UniqueId OwnId = new(0x11111111, 0x22222222);
    private static readonly UniqueId SdlClientId = new(0x33333333, 0x44444444);

    [Fact]
    public void LoadOrCreate_UsesTheClientsOwnFileFirst()
    {
        using var folder = new TempFolder();
        var ownPath = folder.Write("unique.dat", OwnId.ToBytes());
        var sdlClientPath = folder.Write("sdl/unique.dat", SdlClientId.ToBytes());

        Assert.Equal(OwnId, UniqueIdFile.LoadOrCreate(ownPath, sdlClientPath, new Random(1)));
    }

    [Fact]
    public void LoadOrCreate_CopiesTheSdlClientsIdWhenThereIsNoOwnFile()
    {
        using var folder = new TempFolder();
        var ownPath = Path.Combine(folder.FolderPath, "unique.dat");
        var sdlClientPath = folder.Write("sdl/unique.dat", SdlClientId.ToBytes());

        var uniqueId = UniqueIdFile.LoadOrCreate(ownPath, sdlClientPath, new Random(1));

        Assert.Equal(SdlClientId, uniqueId);
        Assert.Equal(SdlClientId.ToBytes(), File.ReadAllBytes(ownPath));
        Assert.Equal(SdlClientId.ToBytes(), File.ReadAllBytes(sdlClientPath));
    }

    [Fact]
    public void LoadOrCreate_MakesAndSavesANewIdWhenNeitherFileExists()
    {
        using var folder = new TempFolder();
        var ownPath = Path.Combine(folder.FolderPath, "unique.dat");
        var sdlClientPath = Path.Combine(folder.FolderPath, "sdl", "unique.dat");

        var uniqueId = UniqueIdFile.LoadOrCreate(ownPath, sdlClientPath, new Random(1));

        Assert.NotEqual(0u, uniqueId.Low);
        Assert.NotEqual(0u, uniqueId.High);
        Assert.Equal(uniqueId.ToBytes(), File.ReadAllBytes(ownPath));
        Assert.False(File.Exists(sdlClientPath));
    }

    [Fact]
    public void LoadOrCreate_TreatsATooShortFileAsMissing()
    {
        using var folder = new TempFolder();
        var ownPath = folder.Write("unique.dat", [1, 2, 3]);
        var sdlClientPath = folder.Write("sdl/unique.dat", SdlClientId.ToBytes());

        Assert.Equal(SdlClientId, UniqueIdFile.LoadOrCreate(ownPath, sdlClientPath, new Random(1)));
        Assert.Equal(SdlClientId.ToBytes(), File.ReadAllBytes(ownPath));
    }

    [Fact]
    public void Save_ReplacesTheId()
    {
        using var folder = new TempFolder();
        var ownPath = folder.Write("unique.dat", OwnId.ToBytes());

        UniqueIdFile.Save(ownPath, SdlClientId);

        Assert.Equal(SdlClientId.ToBytes(), File.ReadAllBytes(ownPath));
    }

    [Fact]
    public void SdlClientFilePath_IsInTheSdlClientsSettingsFolder()
    {
        var expected = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TheLastGate",
            "TheLastGate",
            "unique.dat");

        Assert.Equal(expected, UniqueIdFile.SdlClientFilePath());
    }
}
