namespace TheLastGate.State;

public sealed class WorldMap
{
    public const int Size = 44;
    public const int WorldTiles = 1024;
    public const int TileCount = Size * Size;
    public const int PlayerTile = Size / 2 * Size + Size / 2;

    public MapTile[] Tiles { get; } = new MapTile[TileCount];

    internal int LastUpdatedTile { get; set; }
}
