using System;
using System.Collections.Generic;

namespace TheLastGate.Notes;

public sealed class NoteCategory
{
    public int Id { get; internal set; }

    public int ParentId { get; internal set; }

    public string Name { get; internal set; } = "";

    public int ColourIndex { get; internal set; }

    public bool Visible { get; set; } = true;

    public bool ShowLabels { get; set; } = true;

    public IReadOnlyList<Note> Notes => throw new NotImplementedException();

    public int SegmentTotal => throw new NotImplementedException();

    public static NoteCategory Detached(
        string name,
        int colourIndex,
        bool visible,
        bool showLabels,
        IEnumerable<Note> notes) =>
        throw new NotImplementedException();

    public int Count(NoteKind kind) => throw new NotImplementedException();
}
