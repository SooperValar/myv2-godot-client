using System.Collections.Generic;
using System.Linq;

namespace TheLastGate.Notes;

public enum NoteKind
{
    Pin = 1,
    Text = 2,
    Path = 3,
}

// A path is a set of separate segments, so deleting one can leave a gap.
public readonly record struct PathSegment(int StartX, int StartY, int EndX, int EndY);

// A pin or text stands on its tile; a path's tile is the start of its first segment.
public sealed record Note(
    NoteKind Kind,
    string Label,
    int TileX,
    int TileY,
    IReadOnlyList<PathSegment> Segments,
    int ColourIndex)
{
    public const int CategoryColour = -1;

    public int Id { get; init; }

    public bool IsIdenticalTo(Note other) =>
        Kind == other.Kind
        && Label == other.Label
        && TileX == other.TileX
        && TileY == other.TileY
        && ColourIndex == other.ColourIndex
        && Segments.SequenceEqual(other.Segments);
}
