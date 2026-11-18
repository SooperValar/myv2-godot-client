using System;
using TheLastGate.Notes;

namespace TheLastGate.Windows;

public sealed class NoteImportDraft
{
    public NoteImportDraft(NoteSet notes, int parentId, string clipboard) => throw new NotImplementedException();

    public int ParentId => throw new NotImplementedException();

    public string Text => throw new NotImplementedException();

    public string RootName { get; set; } = "";

    public string Error => throw new NotImplementedException();

    public string Preview => throw new NotImplementedException();

    public bool IsReady => throw new NotImplementedException();

    public bool HasConflict => throw new NotImplementedException();

    public bool DestinationGone => throw new NotImplementedException();

    public string? DestinationName => throw new NotImplementedException();

    public string ConflictMessage => throw new NotImplementedException();

    public bool CanAddAsNew => throw new NotImplementedException();

    public void ChangeText(string text) => throw new NotImplementedException();

    public NoteImportResult? Add() => throw new NotImplementedException();

    public NoteImportResult? Merge() => throw new NotImplementedException();

    public NoteImportResult? AddAsNew() => throw new NotImplementedException();
}
