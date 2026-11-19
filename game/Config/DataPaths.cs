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

    public static string Root() =>
        OS.HasFeature("editor")
            ? Path.GetFullPath(Path.Combine(ProjectSettings.GlobalizePath("res://"), "..", DataFolderName))
            : Path.Combine(Path.GetDirectoryName(OS.GetExecutablePath())!, DataFolderName);

    // The data folder can be read-only (inside the Linux AppImage) and is replaced by every update, so sprites a
    // player adds live in their own folder.
    public static string PlayerData() => ProjectSettings.GlobalizePath(PlayerDataFolder);

    public static string PlayerOverrides() => ProjectSettings.GlobalizePath(PlayerOverridesFolder);
}
