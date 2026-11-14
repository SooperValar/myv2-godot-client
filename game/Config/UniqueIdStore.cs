using System;
using Godot;
using TheLastGate.Net;

namespace TheLastGate.Config;

public static class UniqueIdStore
{
    private const string FilePath = "user://unique.dat";

    public static UniqueId LoadOrCreate() =>
        UniqueIdFile.LoadOrCreate(
            ProjectSettings.GlobalizePath(FilePath),
            UniqueIdFile.SdlClientFilePath(),
            Random.Shared);
}
