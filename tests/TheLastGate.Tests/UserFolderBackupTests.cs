using TheLastGate.Updates;

namespace TheLastGate.Tests;

public sealed class UserFolderBackupTests : IDisposable
{
    private static readonly Version CurrentVersion = new(1, 2, 0);
    private static readonly DateTime BackupTime = new(2026, 11, 24, 15, 30, 5);

    private readonly string root = Path.Combine(Path.GetTempPath(), $"backup-tests-{Guid.NewGuid():N}");
    private readonly string userFolder;

    public UserFolderBackupTests()
    {
        userFolder = Path.Combine(root, "TheLastGateGodot");
        Directory.CreateDirectory(Path.Combine(userFolder, "maps"));
        File.WriteAllText(Path.Combine(userFolder, "settings.json"), "{\"volume\":5}");
        File.WriteAllBytes(Path.Combine(userFolder, "maps", "server.map"), [1, 2, 3]);
    }

    public void Dispose() => Directory.Delete(root, true);

    [Fact]
    public void BackupsFolder_IsASiblingOfTheUserFolder()
    {
        Assert.Equal(Path.Combine(root, "TheLastGateGodot-backups"), UserFolderBackup.BackupsFolder(userFolder));
    }

    [Fact]
    public void Create_CopiesEveryFileAndFolder()
    {
        var backup = UserFolderBackup.Create(userFolder, CurrentVersion, BackupTime);

        Assert.Equal("{\"volume\":5}", File.ReadAllText(Path.Combine(backup, "settings.json")));
        Assert.Equal([1, 2, 3], File.ReadAllBytes(Path.Combine(backup, "maps", "server.map")));
    }

    [Fact]
    public void Create_NamesBackupByVersionAndTime()
    {
        var backup = UserFolderBackup.Create(userFolder, CurrentVersion, BackupTime);

        Assert.Equal(Path.Combine(root, "TheLastGateGodot-backups", "1.2.0-20261124-153005"), backup);
    }

    [Fact]
    public void Create_KeepsOnlyTheNewestThree()
    {
        for (var backupIndex = 0; backupIndex < 5; backupIndex++)
        {
            UserFolderBackup.Create(userFolder, CurrentVersion, BackupTime.AddMinutes(backupIndex));
        }

        Assert.Equal(["1.2.0-20261124-153205", "1.2.0-20261124-153305", "1.2.0-20261124-153405"], KeptBackups());
    }

    [Fact]
    public void Create_KeepsTheNewestByTimeNotByVersion()
    {
        UserFolderBackup.Create(userFolder, new Version(1, 10, 0), BackupTime);
        UserFolderBackup.Create(userFolder, new Version(1, 10, 0), BackupTime.AddMinutes(1));
        UserFolderBackup.Create(userFolder, new Version(1, 9, 0), BackupTime.AddMinutes(2));
        UserFolderBackup.Create(userFolder, new Version(1, 9, 0), BackupTime.AddMinutes(3));

        Assert.Equal(["1.10.0-20261124-153105", "1.9.0-20261124-153205", "1.9.0-20261124-153305"], KeptBackups());
    }

    [Fact]
    public void Create_SameSecondBackupGetsSuffix()
    {
        var first = UserFolderBackup.Create(userFolder, CurrentVersion, BackupTime);
        File.WriteAllText(Path.Combine(userFolder, "settings.json"), "{\"volume\":9}");

        var second = UserFolderBackup.Create(userFolder, CurrentVersion, BackupTime);

        Assert.Equal(first + "-2", second);
        Assert.Equal("{\"volume\":5}", File.ReadAllText(Path.Combine(first, "settings.json")));
    }

    [Fact]
    public void Create_LeavesTheUserFolderUntouched()
    {
        var before = Snapshot(userFolder);

        UserFolderBackup.Create(userFolder, CurrentVersion, BackupTime);

        Assert.Equal(before, Snapshot(userFolder));
    }

    private string[] KeptBackups() =>
        new DirectoryInfo(UserFolderBackup.BackupsFolder(userFolder)).GetDirectories()
            .Select(backup => backup.Name)
            .Order()
            .ToArray();

    private static string[] Snapshot(string folder) =>
        Directory.GetFiles(folder, "*", SearchOption.AllDirectories)
            .Order()
            .Select(file => $"{file}|{File.GetLastWriteTimeUtc(file):O}|{new FileInfo(file).Length}")
            .ToArray();
}
