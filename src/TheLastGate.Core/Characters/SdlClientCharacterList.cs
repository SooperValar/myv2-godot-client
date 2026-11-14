using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;

namespace TheLastGate.Characters;

// { uint32 version, uint32 count }, then { char name[40], char path[512] } per character, little-endian.
public static class SdlClientCharacterList
{
    private const int HeaderSize = 8;
    private const int CountOffset = 4;
    private const int NameLength = 40;
    private const int PathLength = 512;
    private const int EntrySize = NameLength + PathLength;

    public static string FilePath() =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TheLastGate",
            "TheLastGate",
            "TLGCharacters.dat");

    public static List<PreviousCharacter> Parse(ReadOnlySpan<byte> fileBytes)
    {
        var characters = new List<PreviousCharacter>();
        if (fileBytes.Length < HeaderSize)
        {
            return characters;
        }

        var count = BinaryPrimitives.ReadUInt32LittleEndian(fileBytes[CountOffset..]);
        for (var entryStart = HeaderSize;
            count > 0 && entryStart + EntrySize <= fileBytes.Length;
            entryStart += EntrySize, count--)
        {
            var entry = fileBytes.Slice(entryStart, EntrySize);
            var filePath = NulTerminatedText.Read(entry[NameLength..]);
            if (filePath.Length > 0)
            {
                characters.Add(new PreviousCharacter(NulTerminatedText.Read(entry[..NameLength]), filePath));
            }
        }

        return characters;
    }
}
