using System;
using System.Collections.Generic;

namespace TheLastGate.Notes;

public sealed class NoteSet
{
    public IReadOnlyList<NoteCategory> Categories => throw new NotImplementedException();

    public bool Changed { get; set; }

    public int AddCategory(string name, int colourIndex, int parentId = 0) => throw new NotImplementedException();

    public NoteCategory? FindCategory(int categoryId) => throw new NotImplementedException();

    public bool HasChildren(int categoryId) => throw new NotImplementedException();

    public int Depth(int categoryId) => throw new NotImplementedException();

    public int SubtreeHeight(int categoryId) => throw new NotImplementedException();

    public bool IsWithin(int categoryId, int ancestorId) => throw new NotImplementedException();

    public bool IsShown(NoteCategory category) => throw new NotImplementedException();

    public bool LabelsShown(NoteCategory category) => throw new NotImplementedException();

    public IReadOnlyList<NoteCategory> Tree(int rootId = 0) => throw new NotImplementedException();

    public bool RemoveCategory(int categoryId) => throw new NotImplementedException();

    public bool RenameCategory(int categoryId, string name) => throw new NotImplementedException();

    public bool SetCategoryColour(int categoryId, int colourIndex) => throw new NotImplementedException();

    public bool CanMoveCategory(int categoryId, int newParentId) => throw new NotImplementedException();

    public bool MoveCategory(int categoryId, int newParentId) => throw new NotImplementedException();

    public bool CanShiftCategory(int categoryId, int direction) => throw new NotImplementedException();

    public bool ShiftCategory(int categoryId, int direction) => throw new NotImplementedException();
}
