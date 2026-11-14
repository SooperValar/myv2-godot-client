using TheLastGate.Characters;

namespace TheLastGate.Tests;

public class CharacterHistoryTests
{
    [Fact]
    public void Use_PutsTheNewestFileFirst()
    {
        var history = new CharacterHistory();

        history.Use("Ishtar", "C:/saves/ishtar.moa");
        history.Use("Bran", "C:/saves/bran.moa");

        Assert.Equal(["C:/saves/bran.moa", "C:/saves/ishtar.moa"], history.Entries.Select(entry => entry.FilePath));
    }

    [Fact]
    public void Use_MovesAKnownFileToTheFrontWithItsNewName()
    {
        var history = new CharacterHistory();
        history.Use("Ishtar", "C:/saves/ishtar.moa");
        history.Use("Bran", "C:/saves/bran.moa");

        history.Use("Ishtar the Bold", "C:/Saves/ISHTAR.moa");

        Assert.Equal(
            [
                new PreviousCharacter("Ishtar the Bold", "C:/Saves/ISHTAR.moa"),
                new PreviousCharacter("Bran", "C:/saves/bran.moa"),
            ],
            history.Entries);
    }

    [Fact]
    public void Use_KeepsOnlyTheMostRecentEntries()
    {
        var history = new CharacterHistory();

        for (var fileNumber = 0; fileNumber < CharacterHistory.MaxEntries + 5; fileNumber++)
        {
            history.Use($"Character {fileNumber}", $"C:/saves/{fileNumber}.moa");
        }

        Assert.Equal(CharacterHistory.MaxEntries, history.Entries.Count);
        Assert.Equal("C:/saves/24.moa", history.Entries[0].FilePath);
        Assert.Equal("C:/saves/5.moa", history.Entries[^1].FilePath);
    }

    [Fact]
    public void Constructor_KeepsTheSavedOrderWithoutDuplicates()
    {
        PreviousCharacter[] saved =
        [
            new("Bran", "C:/saves/bran.moa"),
            new("Ishtar", "C:/saves/ishtar.moa"),
            new("Bran again", "C:/saves/BRAN.moa"),
        ];

        var history = new CharacterHistory(saved);

        Assert.Equal([saved[0], saved[1]], history.Entries);
    }

    [Fact]
    public void RemoveMissingFiles_DropsFilesThatNoLongerExist()
    {
        using var folder = new TempFolder();
        var kept = folder.Write("kept.moa", [1]);
        var history = new CharacterHistory();
        history.Use("Gone", Path.Combine(folder.FolderPath, "gone.moa"));
        history.Use("Kept", kept);

        history.RemoveMissingFiles();

        Assert.Equal([new PreviousCharacter("Kept", kept)], history.Entries);
    }
}
