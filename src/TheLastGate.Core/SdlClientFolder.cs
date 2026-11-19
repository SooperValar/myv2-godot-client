using System;
using System.IO;

namespace TheLastGate;

public static class SdlClientFolder
{
    private const string OrganisationName = "TheLastGate";
    private const string ApplicationName = "TheLastGate";

    public static string FilePath(string fileName) =>
        Path.Combine(
            Environment.GetFolderPath(SettingsRoot(OperatingSystem.IsWindows())),
            OrganisationName,
            ApplicationName,
            fileName);

    // Must match SDL_GetPrefPath: the roaming AppData on Windows, XDG_DATA_HOME (~/.local/share) on Linux, where .NET's
    // ApplicationData would be ~/.config instead.
    public static Environment.SpecialFolder SettingsRoot(bool isWindows) =>
        isWindows ? Environment.SpecialFolder.ApplicationData : Environment.SpecialFolder.LocalApplicationData;
}
