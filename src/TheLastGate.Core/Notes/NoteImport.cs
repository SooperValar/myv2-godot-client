using System;

namespace TheLastGate.Notes;

public enum NoteImportStatus
{
    Ok,
    NameTaken,
    TooManyCategories,
    TooDeep,
    CategoryFull,
    Failed,
}

public sealed record NoteImportResult(NoteImportStatus Status, int RootId, string CategoryName);

public static class NoteImport
{
    public static int Conflict(NoteSet set, NoteSet imported, int parentId) => throw new NotImplementedException();

    public static NoteImportResult Add(NoteSet set, NoteSet imported, int parentId, string? rootName) =>
        throw new NotImplementedException();

    public static NoteImportResult Merge(NoteSet set, NoteSet imported, int targetId) =>
        throw new NotImplementedException();

    public static string Problem(NoteImportResult result) => throw new NotImplementedException();

    public static string ConflictMessage(string? destinationName, string name) => throw new NotImplementedException();
}
