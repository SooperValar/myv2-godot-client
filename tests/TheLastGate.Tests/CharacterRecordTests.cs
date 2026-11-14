using TheLastGate.Characters;

namespace TheLastGate.Tests;

public class CharacterRecordTests
{
    [Fact]
    public void CreateNew_IsANewAccountWithDefaultSettings()
    {
        var record = CharacterRecord.CreateNew();

        Assert.Equal(new LoginKey(0, 0, 0, CharacterRecord.NewAccountName, 0), record.Key);
        Assert.Equal("", record.CharacterName);
        Assert.True(record.Changed);
        Assert.True(record.Settings().HideWalls);
        Assert.Equal(CharacterSettings.Unassigned, record.Settings().HotbarActions[0]);
    }

    [Fact]
    public void Write_ThenRead_KeepsEverything()
    {
        using var folder = new TempFolder();
        var path = Path.Combine(folder.FolderPath, "ishtar.moa");
        var record = CharacterRecord.CreateNew();
        record.Key = new LoginKey(1234, 5, 6, "Ishtar", 110009);
        record.CharacterName = "Ishtar";
        record.Description = "A quiet Harakim.";
        record.Changed = false;

        record.Write(path);
        var read = CharacterRecord.Read(path);

        Assert.Equal(CharacterRecord.Size, new FileInfo(path).Length);
        Assert.Equal(record.Key, read.Key);
        Assert.Equal("Ishtar", read.CharacterName);
        Assert.Equal("A quiet Harakim.", read.Description);
        Assert.False(read.Changed);
    }

    [Fact]
    public void Read_GivesAKeyOnlyFileDefaultSettings()
    {
        using var folder = new TempFolder();
        var keyBytes = new byte[LoginKey.Size];
        new LoginKey(77, 1, 2, "Old", 4).Write(keyBytes);
        var path = folder.Write("old.moa", keyBytes);

        var record = CharacterRecord.Read(path);

        Assert.Equal(77u, record.Key.UserNumber);
        Assert.Equal("", record.CharacterName);
        Assert.True(record.Settings().ShowNames);
    }

    [Fact]
    public void Read_RejectsAFileShorterThanTheKey()
    {
        using var folder = new TempFolder();
        var path = folder.Write("broken.moa", new byte[LoginKey.Size - 1]);

        Assert.Throws<InvalidDataException>(() => CharacterRecord.Read(path));
    }

    [Fact]
    public void NameAndDescription_AreTruncatedToFitTheirFields()
    {
        var record = CharacterRecord.CreateNew();

        record.CharacterName = new string('n', 100);
        record.Description = new string('d', 200);

        Assert.Equal(79, record.CharacterName.Length);
        Assert.Equal(159, record.Description.Length);
    }

    [Fact]
    public void StartNewAccount_ForgetsTheAccountButKeepsTheDescriptionAndHotbar()
    {
        var record = CharacterRecord.CreateNew();
        record.Key = new LoginKey(1234, 5, 6, "Ishtar", 110009);
        record.CharacterName = "Ishtar";
        record.Description = "A quiet Harakim.";
        var settings = record.Settings();
        settings.HotbarActions[0] = 50;
        record.Store(settings);
        record.Changed = false;

        record.StartNewAccount();

        Assert.Equal(0u, record.Key.UserNumber);
        Assert.Equal("", record.CharacterName);
        Assert.Equal("A quiet Harakim.", record.Description);
        Assert.Equal(50, record.Settings().HotbarActions[0]);
        Assert.True(record.Changed);
    }
}
