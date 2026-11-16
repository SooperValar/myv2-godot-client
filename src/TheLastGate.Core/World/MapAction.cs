using TheLastGate.Cursors;

namespace TheLastGate.World;

public readonly record struct MapAction(
    int HighlightedTile,
    MapLayer HighlightedLayer,
    MouseCursor Cursor,
    byte[]? Command,
    int? NewSelection)
{
    public const int NoTile = -1;

    public static MapAction Nothing { get; } = new(NoTile, MapLayer.Floor, MouseCursor.None, null, null);
}
