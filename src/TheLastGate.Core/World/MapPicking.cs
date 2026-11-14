using TheLastGate.State;

namespace TheLastGate.World;

public static class MapPicking
{
    private const int TileSize = IsometricProjection.TileSize;
    private const int MouseShiftX = 176 - 16;
    private const int MouseShiftY = 8;
    private const int ViewSize = 48;
    private const int EdgeMargin = 3;
    private const int FirstTile = (WorldMap.Size - ViewSize) / 2 + EdgeMargin;
    private const int LastTile = WorldMap.Size - FirstTile;
    private const int LowestPosition = FirstTile * TileSize + 12;
    private const int HighestPosition = LastTile * TileSize + 20;

    public static (int Column, int Row)? TileAt(int screenX, int screenY, int walkOffsetX = 0, int walkOffsetY = 0)
    {
        var x = screenX + walkOffsetX + MouseShiftX;
        var y = screenY + walkOffsetY + MouseShiftY;
        var columnPosition = 2 * y + x - 2 * IsometricProjection.ScreenOriginY - IsometricProjection.ScreenOriginX
            + IsometricProjection.MapCentringShift;
        var rowPosition = x - 2 * y + 2 * IsometricProjection.ScreenOriginY - IsometricProjection.ScreenOriginX
            + IsometricProjection.MapCentringShift;
        if (columnPosition is < LowestPosition or > HighestPosition
            || rowPosition is < LowestPosition or > HighestPosition)
        {
            return null;
        }

        var column = columnPosition / TileSize;
        var row = rowPosition / TileSize;
        return column is >= FirstTile and <= LastTile && row is >= FirstTile and <= LastTile ? (column, row) : null;
    }
}
