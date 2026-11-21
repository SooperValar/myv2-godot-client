using System.Collections.Generic;
using System.Linq;

namespace TheLastGate.Notes;

public sealed class NoteCategory
{
    public int Id { get; internal set; }

    public int ParentId { get; internal set; }

    public string Name { get; internal set; } = "";

    public int ColourIndex { get; internal set; }

    public bool Visible { get; set; } = true;

    public bool ShowLabels { get; set; } = true;

    public IReadOnlyList<Note> Notes => NoteList;

    public int SegmentTotal => NoteList.Sum(note => note.Segments.Count);

    internal List<Note> NoteList { get; } = [];

    // A category read from a file or a share code, not yet part of a set; attaching it gives it and its notes ids.
    public static NoteCategory Detached(
        string name,
        int colourIndex,
        bool visible,
        bool showLabels,
        IEnumerable<Note> notes)
    {
        var category = new NoteCategory
        {
            Name = name,
            ColourIndex = colourIndex,
            Visible = visible,
            ShowLabels = showLabels,
        };
        category.NoteList.AddRange(notes);
        return category;
    }

    public int Count(NoteKind kind) => NoteList.Count(note => note.Kind == kind);
}
