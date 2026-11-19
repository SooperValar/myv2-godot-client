using System;
using System.IO;

namespace TheLastGate.Net;

public static class UniqueIdFile
{
    public static string SdlClientFilePath() => SdlClientFolder.FilePath("unique.dat");

    public static UniqueId LoadOrCreate(string filePath, string sdlClientFilePath, Random random)
    {
        if (Read(filePath) is { } ownId)
        {
            return ownId;
        }

        var uniqueId = Read(sdlClientFilePath) ?? UniqueId.CreateRandom(random);
        Save(filePath, uniqueId);
        return uniqueId;
    }

    public static void Save(string filePath, UniqueId uniqueId) => File.WriteAllBytes(filePath, uniqueId.ToBytes());

    private static UniqueId? Read(string filePath) =>
        File.Exists(filePath) ? UniqueId.Parse(File.ReadAllBytes(filePath)) : null;
}
