using System;
using TheLastGate.State;

namespace TheLastGate.World;

public static class MapSpriteRules
{
    public static int FloorSprite(in MapTile tile) => throw new NotImplementedException();

    public static int ObjectSprite(in MapTile tile, int column, int row, PlayerState player, bool hideWalls) =>
        throw new NotImplementedException();

    public static int CharacterSprite(in MapTile tile, int column, int row) => throw new NotImplementedException();
}
