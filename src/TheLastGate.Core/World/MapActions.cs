using System;
using TheLastGate.State;

namespace TheLastGate.World;

public static class MapActions
{
    public static MapAction Decide(
        GameState state,
        (float Column, float Row)? point,
        MapKeys keys,
        MapButton button,
        int selectedCharacter) =>
        throw new NotImplementedException();
}
