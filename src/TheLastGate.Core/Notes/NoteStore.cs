using System;

namespace TheLastGate.Notes;

public static class NoteStore
{
    public static byte[] Save(NoteSet set) => throw new NotImplementedException();

    public static NoteSet? Load(ReadOnlySpan<byte> file) => throw new NotImplementedException();

    public static bool SaveTo(string path, NoteSet set) => throw new NotImplementedException();

    public static (NoteSet Set, bool SetAside) Open(string ownPath, string sdlClientPath) =>
        throw new NotImplementedException();
}
