using System;
using System.Collections.Generic;
using TheLastGate.Map;
using TheLastGate.Notes;

namespace TheLastGate.Windows;

public enum NoteLabelPurpose
{
    None,
    Pin,
    Text,
    Path,
}

public sealed class WorldMapEditor
{
    public const double SelectRadius = 8;
    public const double ClickSlack = 2;

    public WorldMapTool Tool => throw new NotImplementedException();

    public int CurrentCategoryId => throw new NotImplementedException();

    public int SelectedNoteId => throw new NotImplementedException();

    public IReadOnlyList<PathSegment> PendingSegments => throw new NotImplementedException();

    public int PendingCategoryId => throw new NotImplementedException();

    public Note? Edited => throw new NotImplementedException();

    public bool CanSaveEdit => throw new NotImplementedException();

    public NoteLabelPurpose LabelPurpose => throw new NotImplementedException();

    public bool Focused { get; set; }

    public event Action<string>? Problem
    {
        add => throw new NotImplementedException();
        remove => throw new NotImplementedException();
    }

    public void EnsureCurrentCategory(NoteSet notes) => throw new NotImplementedException();

    public void ChooseTool(WorldMapTool tool) => throw new NotImplementedException();

    public void Select(int noteId) => throw new NotImplementedException();

    public void BeginEdit(NoteSet notes, int noteId) => throw new NotImplementedException();

    public void RenameEdited(string label) => throw new NotImplementedException();

    public void SaveEdit(NoteSet notes) => throw new NotImplementedException();

    public void CancelEdit() => throw new NotImplementedException();

    public void Delete(NoteSet notes, int noteId) => throw new NotImplementedException();

    public void DeleteSegment(NoteSet notes, int noteId, int segmentIndex) => throw new NotImplementedException();

    public void Recolour(NoteSet notes, int noteId, int colourIndex) => throw new NotImplementedException();

    public void SendTo(NoteSet notes, int noteId, int categoryId) => throw new NotImplementedException();

    public void PressLeft(WorldMapPanel panel, double panelX, double panelY) =>
        throw new NotImplementedException();

    public void MoveMouse(WorldMapPanel panel, double panelX, double panelY) => throw new NotImplementedException();

    public void ReleaseLeft(NoteSet notes, WorldMapPanel panel, double panelX, double panelY) =>
        throw new NotImplementedException();

    public void PressRight(double panelX, double panelY) => throw new NotImplementedException();

    public NoteHit? ReleaseRight(NoteSet notes, WorldMapPanel panel, double panelX, double panelY) =>
        throw new NotImplementedException();

    public PathSegment? Preview(WorldMapPanel panel, double panelX, double panelY) =>
        throw new NotImplementedException();

    public int DrawingCategoryId(NoteSet notes) => throw new NotImplementedException();

    public bool CanConfirmLabel(string label) => throw new NotImplementedException();

    public void ConfirmLabel(NoteSet notes, string label) => throw new NotImplementedException();

    public void CancelLabel() => throw new NotImplementedException();

    public bool FinishPendingPath() => throw new NotImplementedException();

    public bool DiscardPendingPath() => throw new NotImplementedException();

    public void DropInterruptedWork() => throw new NotImplementedException();
}
