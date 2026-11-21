using System.Linq;
using TheLastGate.Notes;

namespace TheLastGate.Windows;

// The Import prompt while it is open: the pasted code, what it holds, and whether its top category clashes with one
// already there. A clash offers merging into that category or adding under a name of the player's choosing.
public sealed class NoteImportDraft
{
    private readonly NoteSet notes;
    private int conflictId;
    private NoteSet? imported;
    private NoteCodeStatus status;

    // A code already on the clipboard is pasted in, so sharing one is copy, Import, Add.
    public NoteImportDraft(NoteSet notes, int parentId, string clipboard)
    {
        this.notes = notes;
        ParentId = parentId;
        var trimmed = clipboard.TrimStart(' ', '\t', '\r', '\n');
        ChangeText(NoteShareCode.HasPrefix(trimmed) ? trimmed : "");
    }

    public int ParentId { get; }

    public string Text { get; private set; } = "";

    public string RootName { get; set; } = "";

    public string Error { get; private set; } = "";

    public string Preview => Text.Length == 0
        ? ""
        : status == NoteCodeStatus.Ok
            ? NoteShareCode.Describe(imported!)
            : NoteShareCode.Problem(status);

    public bool IsReady => Text.Length > 0 && status == NoteCodeStatus.Ok && imported!.Categories.Count > 0;

    public bool HasConflict => IsReady && conflictId != 0;

    public bool DestinationGone => ParentId != 0 && notes.FindCategory(ParentId) == null;

    public string? DestinationName => notes.FindCategory(ParentId)?.Name;

    public string ConflictMessage =>
        HasConflict ? NoteImport.ConflictMessage(DestinationName, imported!.Categories[0].Name) : "";

    public bool CanAddAsNew =>
        HasConflict
        && RootName.Length > 0
        && !notes.Categories.Any(category =>
            category.ParentId == ParentId && NoteText.SameName(category.Name, RootName));

    public void ChangeText(string text)
    {
        Text = text;
        status = NoteShareCode.Import(text, out imported);
        Error = "";
        conflictId = 0;
        RootName = "";
        if (IsReady)
        {
            conflictId = NoteImport.Conflict(notes, imported!, ParentId);
            RootName = imported!.Categories[0].Name;
        }
    }

    public NoteImportResult? Add() =>
        IsReady && !HasConflict ? Report(NoteImport.Add(notes, imported!, ParentId, null)) : null;

    public NoteImportResult? Merge() => HasConflict ? Report(NoteImport.Merge(notes, imported!, conflictId)) : null;

    public NoteImportResult? AddAsNew() =>
        CanAddAsNew ? Report(NoteImport.Add(notes, imported!, ParentId, RootName)) : null;

    private NoteImportResult Report(NoteImportResult result)
    {
        Error = result.Status == NoteImportStatus.Ok ? "" : NoteImport.Problem(result);
        return result;
    }
}
