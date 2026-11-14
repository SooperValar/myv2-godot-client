namespace TheLastGate.State;

public sealed class WorldMap
{
    public const int Size = 54;
    public const int TileCount = Size * Size;

    public MapTile[] Tiles { get; } = new MapTile[TileCount];
}
