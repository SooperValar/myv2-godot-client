using System;
using System.Collections.Generic;

namespace TheLastGate.Notes;

public enum NoteKind
{
    Pin = 1,
    Text = 2,
    Path = 3,
}

public readonly record struct PathSegment(int StartX, int StartY, int EndX, int EndY);

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

    public bool IsIdenticalTo(Note other) => throw new NotImplementedException();
}
