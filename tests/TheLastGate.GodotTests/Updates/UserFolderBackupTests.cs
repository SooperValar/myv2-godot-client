using GdUnit4;
using TheLastGate.Updates;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Updates;

[TestSuite]
public class UserFolderBackupTests
{
    private static readonly Version CurrentVersion = new(1, 2, 0);
    private static readonly DateTime BackupTime = new(2026, 11, 24, 15, 30, 5);

    private string root = "";
    private string userFolder = "";

    [BeforeTest]
    public void MakeAUserFolder()
    {
        root = Path.Combine(Path.GetTempPath(), $"backup-tests-{Guid.NewGuid():N}");
        userFolder = Path.Combine(root, "TheLastGateGodot");
        Directory.CreateDirectory(Path.Combine(userFolder, "maps"));
        File.WriteAllText(Path.Combine(userFolder, "settings.json"), "{\"volume\":5}");
        File.WriteAllBytes(Path.Combine(userFolder, "maps", "server.map"), [1, 2, 3]);
    }

    [AfterTest]
    public void DeleteTheUserFolder() => Directory.Delete(root, true);

    [TestCase]
    public void BackupsFolder_IsASiblingOfTheUserFolder() =>
        AssertThat(UserFolderBackup.BackupsFolder(userFolder)).IsEqual(Path.Combine(root, "TheLastGateGodot-backups"));

    [TestCase]
    public void Create_CopiesEveryFileAndFolder()
    {
        var backup = UserFolderBackup.Create(userFolder, CurrentVersion, BackupTime);

        AssertThat(File.ReadAllText(Path.Combine(backup, "settings.json"))).IsEqual("{\"volume\":5}");
        AssertThat(File.ReadAllBytes(Path.Combine(backup, "maps", "server.map"))).ContainsExactly(1, 2, 3);
    }

    [TestCase]
    public void Create_NamesBackupByVersionAndTime()
    {
        var backup = UserFolderBackup.Create(userFolder, CurrentVersion, BackupTime);

        AssertThat(backup).IsEqual(Path.Combine(root, "TheLastGateGodot-backups", "1.2.0-20261124-153005"));
    }

    [TestCase]
    public void Create_KeepsOnlyTheNewestThree()
    {
        for (var backupIndex = 0; backupIndex < 5; backupIndex++)
        {
            UserFolderBackup.Create(userFolder, CurrentVersion, BackupTime.AddMinutes(backupIndex));
        }

        AssertThat(KeptBackups())
            .ContainsExactly("1.2.0-20261124-153205", "1.2.0-20261124-153305", "1.2.0-20261124-153405");
    }

    [TestCase]
    public void Create_KeepsTheNewestByTimeNotByVersion()
    {
        UserFolderBackup.Create(userFolder, new Version(1, 10, 0), BackupTime);
        UserFolderBackup.Create(userFolder, new Version(1, 10, 0), BackupTime.AddMinutes(1));
        UserFolderBackup.Create(userFolder, new Version(1, 9, 0), BackupTime.AddMinutes(2));
        UserFolderBackup.Create(userFolder, new Version(1, 9, 0), BackupTime.AddMinutes(3));

        AssertThat(KeptBackups())
            .ContainsExactly("1.10.0-20261124-153105", "1.9.0-20261124-153205", "1.9.0-20261124-153305");
    }

    [TestCase]
    public void Create_SameSecondBackupGetsSuffix()
    {
        var first = UserFolderBackup.Create(userFolder, CurrentVersion, BackupTime);
        File.WriteAllText(Path.Combine(userFolder, "settings.json"), "{\"volume\":9}");

        var second = UserFolderBackup.Create(userFolder, CurrentVersion, BackupTime);

        AssertThat(second).IsEqual(first + "-2");
        AssertThat(File.ReadAllText(Path.Combine(first, "settings.json"))).IsEqual("{\"volume\":5}");
    }

    [TestCase]
    public void Create_LeavesTheUserFolderUntouched()
    {
        var before = Snapshot(userFolder);

        UserFolderBackup.Create(userFolder, CurrentVersion, BackupTime);

        AssertThat(Snapshot(userFolder)).ContainsExactly(before);
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
