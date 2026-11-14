using System.Buffers.Binary;
using System.Text;
using TheLastGate.Characters;

namespace TheLastGate.Tests;

public class SdlClientCharacterListTests
{
    private const int HeaderSize = 8;
    private const int NameLength = 40;
    private const int PathLength = 512;

    [Fact]
    public void Parse_ReadsNamesAndPathsInOrder()
    {
        var fileBytes = ListFile(2, ("Worldofslaying", @"C:\saves\slaying.moa"), ("Élan", @"C:\saves\élan.moa"));

        var characters = SdlClientCharacterList.Parse(fileBytes);

        Assert.Equal(
            [
                new PreviousCharacter("Worldofslaying", @"C:\saves\slaying.moa"),
                new PreviousCharacter("Élan", @"C:\saves\élan.moa"),
            ],
            characters);
    }

    [Fact]
    public void Parse_ReadsOnlyAsManyEntriesAsTheCountSays()
    {
        var fileBytes = ListFile(1, ("First", @"C:\saves\first.moa"), ("Second", @"C:\saves\second.moa"));

        var characters = SdlClientCharacterList.Parse(fileBytes);

        Assert.Equal([new PreviousCharacter("First", @"C:\saves\first.moa")], characters);
    }

    [Fact]
    public void Parse_StopsAtTheEndOfAShortFile()
    {
        var fileBytes = ListFile(3, ("First", @"C:\saves\first.moa"), ("Second", @"C:\saves\second.moa"));

        var characters = SdlClientCharacterList.Parse(fileBytes);

        Assert.Equal(2, characters.Count);
    }

    [Fact]
    public void Parse_SkipsEntriesWithoutAPath()
    {
        var fileBytes = ListFile(2, ("Nobody", ""), ("Somebody", @"C:\saves\somebody.moa"));

        var characters = SdlClientCharacterList.Parse(fileBytes);

        Assert.Equal([new PreviousCharacter("Somebody", @"C:\saves\somebody.moa")], characters);
    }

    [Fact]
    public void Parse_ReturnsNothingForAFileShorterThanItsHeader()
    {
        Assert.Empty(SdlClientCharacterList.Parse(new byte[HeaderSize - 1]));
    }

    private static byte[] ListFile(uint count, params (string Name, string FilePath)[] entries)
    {
        var fileBytes = new byte[HeaderSize + entries.Length * (NameLength + PathLength)];
        BinaryPrimitives.WriteUInt32LittleEndian(fileBytes, 1);
        BinaryPrimitives.WriteUInt32LittleEndian(fileBytes.AsSpan(4), count);
        for (var entryIndex = 0; entryIndex < entries.Length; entryIndex++)
        {
            var entryStart = HeaderSize + entryIndex * (NameLength + PathLength);
            Encoding.Latin1.GetBytes(entries[entryIndex].Name).CopyTo(fileBytes, entryStart);
            Encoding.Latin1.GetBytes(entries[entryIndex].FilePath).CopyTo(fileBytes, entryStart + NameLength);
        }

        return fileBytes;
    }
}
