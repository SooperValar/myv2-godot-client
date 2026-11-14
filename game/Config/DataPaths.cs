using System.IO;
using Godot;

namespace TheLastGate.Config;

// An exported game reads the data folder next to its executable. In the editor the executable is
// Godot itself, so the repository's data folder beside the project is used instead.
public static class DataPaths
{
    private const string DataFolderName = "data";

    public static string Root() =>
        OS.HasFeature("editor")
            ? Path.GetFullPath(Path.Combine(ProjectSettings.GlobalizePath("res://"), "..", DataFolderName))
            : Path.Combine(Path.GetDirectoryName(OS.GetExecutablePath())!, DataFolderName);
}
