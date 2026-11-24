using System.IO;
using Godot;

namespace TheLastGate.Config;

// An exported game reads the data folder next to its executable. In the editor the executable is
// Godot itself, so the repository's data folder beside the project is used instead.
public static class DataPaths
{
    private const string DataFolderName = "data";
    private const string PlayerDataFolder = "user://";
    private const string PlayerOverridesFolder = "user://overrides";
    private const string MyV2ClientVariable = "MYV2_CLIENT";
    private const string MyV2ClientFolderName = "Client";

    public static string Root() =>
        OS.HasFeature("editor")
            ? Path.GetFullPath(Path.Combine(ProjectSettings.GlobalizePath("res://"), "..", DataFolderName))
            : Path.Combine(Path.GetDirectoryName(OS.GetExecutablePath())!, DataFolderName);

    // The data folder can be read-only (inside the Linux AppImage) and is replaced by every update, so sprites a
    // player adds live in their own folder.
    public static string PlayerData() => ProjectSettings.GlobalizePath(PlayerDataFolder);

    public static string PlayerOverrides() => ProjectSettings.GlobalizePath(PlayerOverridesFolder);

    // The MyV2 client's folder holds the game's art and sounds. MYV2_CLIENT names it; otherwise the editor uses the
    // Client folder beside the repository (MyV2\Client next to MyV2\Godot-Client), and an exported game the folder
    // its executable is in.
    public static string MyV2Client()
    {
        var chosen = System.Environment.GetEnvironmentVariable(MyV2ClientVariable);
        if (!string.IsNullOrWhiteSpace(chosen))
        {
            return chosen;
        }

        return OS.HasFeature("editor")
            ? Path.GetFullPath(Path.Combine(ProjectSettings.GlobalizePath("res://"), "..", "..", MyV2ClientFolderName))
            : Path.GetDirectoryName(OS.GetExecutablePath())!;
    }
}
