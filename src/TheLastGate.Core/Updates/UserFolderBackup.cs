using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace TheLastGate.Updates;

public static class UserFolderBackup
{
    public const int KeptBackups = 3;

    private const string TimeFormat = "yyyyMMdd-HHmmss";

    public static string BackupsFolder(string userFolder)
    {
        var folder = new DirectoryInfo(userFolder);
        return Path.Combine(folder.Parent!.FullName, $"{folder.Name}-backups");
    }

    public static string Create(string userFolder, Version currentVersion, DateTime now)
    {
        var backupsFolder = BackupsFolder(userFolder);
        var backup = FreeFolder(
            Path.Combine(backupsFolder, $"{currentVersion}-{now.ToString(TimeFormat, CultureInfo.InvariantCulture)}"));
        CopyFolder(userFolder, backup);
        RemoveOldBackups(backupsFolder);
        return backup;
    }

    private static string FreeFolder(string wanted)
    {
        var candidate = wanted;
        for (var suffix = 2; Directory.Exists(candidate); suffix++)
        {
            candidate = $"{wanted}-{suffix}";
        }

        return candidate;
    }

    private static void CopyFolder(string source, string target)
    {
        Directory.CreateDirectory(target);
        foreach (var file in Directory.GetFiles(source))
        {
            File.Copy(file, Path.Combine(target, Path.GetFileName(file)));
        }

        foreach (var folder in Directory.GetDirectories(source))
        {
            CopyFolder(folder, Path.Combine(target, Path.GetFileName(folder)));
        }
    }

    // A backup is named "<version>-<time>[-n]"; versions contain no '-', so the part after the first '-' sorts by time.
    private static void RemoveOldBackups(string backupsFolder)
    {
        var oldBackups = new DirectoryInfo(backupsFolder).GetDirectories()
            .OrderByDescending(backup => backup.Name[(backup.Name.IndexOf('-') + 1)..], StringComparer.Ordinal)
            .Skip(KeptBackups);
        foreach (var oldBackup in oldBackups)
        {
            oldBackup.Delete(true);
        }
    }
}
