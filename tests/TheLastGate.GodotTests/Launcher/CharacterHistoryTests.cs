using System.Linq;
using GdUnit4;
using TheLastGate.Launcher;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Launcher;

[TestSuite]
public class CharacterHistoryTests
{
    [TestCase]
    public void Use_PutsTheNewestFileFirst()
    {
        var history = new CharacterHistory();

        history.Use("Ishtar", "C:/saves/ishtar.moa");
        history.Use("Bran", "C:/saves/bran.moa");

        AssertThat(history.Entries.Select(entry => entry.FilePath))
            .ContainsExactly("C:/saves/bran.moa", "C:/saves/ishtar.moa");
    }

    [TestCase]
    public void Use_MovesAKnownFileToTheFrontWithItsNewName()
    {
        var history = new CharacterHistory();
        history.Use("Ishtar", "C:/saves/ishtar.moa");
        history.Use("Bran", "C:/saves/bran.moa");

        history.Use("Ishtar the Bold", "C:/Saves/ISHTAR.moa");

        AssertThat(history.Entries)
            .ContainsExactly(
                new PreviousCharacter("Ishtar the Bold", "C:/Saves/ISHTAR.moa"),
                new PreviousCharacter("Bran", "C:/saves/bran.moa"));
    }

    [TestCase]
    public void Use_KeepsOnlyTheMostRecentEntries()
    {
        var history = new CharacterHistory();

        for (var fileNumber = 0; fileNumber < CharacterHistory.MaxEntries + 5; fileNumber++)
        {
            history.Use($"Character {fileNumber}", $"C:/saves/{fileNumber}.moa");
        }

        AssertThat(history.Entries.Count).IsEqual(CharacterHistory.MaxEntries);
        AssertThat(history.Entries[0].FilePath).IsEqual("C:/saves/24.moa");
        AssertThat(history.Entries[^1].FilePath).IsEqual("C:/saves/5.moa");
    }

    [TestCase]
    public void Constructor_KeepsTheSavedOrderWithoutDuplicates()
    {
        PreviousCharacter[] saved =
        [
            new("Bran", "C:/saves/bran.moa"),
            new("Ishtar", "C:/saves/ishtar.moa"),
            new("Bran again", "C:/saves/BRAN.moa"),
        ];

        var history = new CharacterHistory(saved);

        AssertThat(history.Entries).ContainsExactly(saved[0], saved[1]);
    }

    [TestCase]
    public void RemoveMissingFiles_DropsFilesThatNoLongerExist()
    {
        using var folder = new TempFolder();
        var kept = folder.Write("kept.moa", [1]);
        var history = new CharacterHistory();
        history.Use("Gone", Path.Combine(folder.FolderPath, "gone.moa"));
        history.Use("Kept", kept);

        history.RemoveMissingFiles();

        AssertThat(history.Entries).ContainsExactly(new PreviousCharacter("Kept", kept));
    }

    [TestCase]
    public void Remove_DropsOnlyThatFile()
    {
        var history = new CharacterHistory();
        history.Use("Bran", "C:/saves/bran.moa");
        history.Use("Bran", "C:/saves/bran-old.moa");
        history.Use("Ishtar", "C:/saves/ishtar.moa");

        history.Remove("C:/saves/BRAN-OLD.moa");

        AssertThat(history.Entries).ContainsExactly(
            new PreviousCharacter("Ishtar", "C:/saves/ishtar.moa"),
            new PreviousCharacter("Bran", "C:/saves/bran.moa"));
    }

    [TestCase]
    public void Clear_EmptiesTheList()
    {
        var history = new CharacterHistory();
        history.Use("Bran", "C:/saves/bran.moa");
        history.Use("Ishtar", "C:/saves/ishtar.moa");

        history.Clear();

        AssertThat(history.Entries).IsEmpty();
    }

    [TestCase]
    public void Label_AddsTheFileNameOnlyWhenTheNameRepeats()
    {
        var history = new CharacterHistory();
        history.Use("Bran", "C:/saves/bran.moa");
        history.Use("Bran", "C:/saves/bran-old.moa");
        history.Use("Ishtar", "C:/saves/ishtar.moa");
        history.Use("", "C:/saves/nameless.moa");

        AssertThat(history.Entries.Select(history.Label).ToArray())
            .ContainsExactly("nameless", "Ishtar", "Bran (bran-old)", "Bran (bran)");
    }
}
