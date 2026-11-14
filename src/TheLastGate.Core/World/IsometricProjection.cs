using TheLastGate.State;

namespace TheLastGate.World;

public static class IsometricProjection
{
    public const int TileSize = 32;
    public const int CameraOffsetX = -176;

    private const int HalfTile = TileSize / 2;
    private const int QuarterTile = TileSize / 4;
    private const int ScreenOriginX = 224 + 14;
    private const int ScreenOriginY = 460;
    private const int DesignViewSize = 34;

    // The layout was made for a 34-tile view, so the 54-tile map moves left by the extra half to keep the player in place.
    private const int MapCentringShift = (WorldMap.Size - DesignViewSize) / 2 * TileSize;

    public static (int X, int Y) TilePosition(int column, int row) =>
        (HalfTile * (column + row) + TileSize + ScreenOriginX - MapCentringShift,
            QuarterTile * (column - row) + ScreenOriginY);

    public static (int X, int Y) SpriteOffset(int width, int height) =>
        (-(width / TileSize) * HalfTile, -(height / TileSize) * TileSize);
}
