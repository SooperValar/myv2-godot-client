using TheLastGate.Characters;

namespace TheLastGate.Tests;

public class CharacterRecordTests
{
    private const int SomeSettingByte = 344;

    [Fact]
    public void CreateNew_IsANewAccountWithDefaultSettings()
    {
        var record = CharacterRecord.CreateNew();

        Assert.Equal(new LoginKey(0, 0, 0, CharacterRecord.NewAccountName, 0), record.Key);
        Assert.Equal("", record.CharacterName);
        Assert.True(record.Changed);
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
        Assert.True(read.SettingsRead);
    }

    [Fact]
    public void Read_KeepsTheKeyOfAKeyOnlyFileButNoSettings()
    {
        using var folder = new TempFolder();
        var keyBytes = new byte[LoginKey.Size];
        new LoginKey(77, 1, 2, "Old", 4).Write(keyBytes);
        var path = folder.Write("old.moa", keyBytes);

        var record = CharacterRecord.Read(path);

        Assert.Equal(77u, record.Key.UserNumber);
        Assert.Equal("", record.CharacterName);
        Assert.False(record.SettingsRead);
    }

    [Fact]
    public void Read_KeepsTheSharedFieldsOfAMyV2File()
    {
        using var folder = new TempFolder();
        const int myV2SettingsSize = 480;
        const int descriptionOffset = 160;
        var fileBytes = new byte[LoginKey.Size + myV2SettingsSize];
        new LoginKey(77, 1, 2, "Iluvatar", 4).Write(fileBytes);
        "Iluvatar"u8.CopyTo(fileBytes.AsSpan(LoginKey.Size));
        "A tall figure in white."u8.CopyTo(fileBytes.AsSpan(LoginKey.Size + descriptionOffset));
        fileBytes[^1] = 0x55;
        var path = folder.Write("iluvatar.moa", fileBytes);

        var record = CharacterRecord.Read(path);

        Assert.Equal(77u, record.Key.UserNumber);
        Assert.Equal("Iluvatar", record.CharacterName);
        Assert.Equal("A tall figure in white.", record.Description);
        Assert.False(record.SettingsRead);
        Assert.Equal(0, record.SettingsBlock[^1]);
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
    public void StartNewAccount_ForgetsTheAccountButKeepsTheDescriptionAndSettings()
    {
        var record = CharacterRecord.CreateNew();
        record.Key = new LoginKey(1234, 5, 6, "Ishtar", 110009);
        record.CharacterName = "Ishtar";
        record.Description = "A quiet Harakim.";
        record.SettingsBlock[SomeSettingByte] = 50;
        record.Changed = false;

        record.StartNewAccount();

        Assert.Equal(0u, record.Key.UserNumber);
        Assert.Equal("", record.CharacterName);
        Assert.Equal("A quiet Harakim.", record.Description);
        Assert.Equal(50, record.SettingsBlock[SomeSettingByte]);
        Assert.True(record.Changed);
    }
}
