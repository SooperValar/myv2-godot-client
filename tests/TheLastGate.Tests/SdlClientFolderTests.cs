namespace TheLastGate.Tests;

public class SdlClientFolderTests
{
    [Fact]
    public void SettingsRoot_IsTheRoamingAppDataOnWindows()
    {
        Assert.Equal(Environment.SpecialFolder.ApplicationData, SdlClientFolder.SettingsRoot(true));
    }

    [Fact]
    public void SettingsRoot_IsTheXdgDataFolderOnLinux()
    {
        Assert.Equal(Environment.SpecialFolder.LocalApplicationData, SdlClientFolder.SettingsRoot(false));
    }

    [Fact]
    public void FilePath_IsInTheSdlClientsPrefPathForThisSystem()
    {
        var expected = Path.Combine(
            Environment.GetFolderPath(SdlClientFolder.SettingsRoot(OperatingSystem.IsWindows())),
            "TheLastGate",
            "TheLastGate",
            "unique.dat");

        Assert.Equal(expected, SdlClientFolder.FilePath("unique.dat"));
    }
}
