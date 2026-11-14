using System;
using TheLastGate.State;

namespace TheLastGate.World;

public static class MapMarkers
{
    public const int WalkTarget = 31;
    public const int AttackTarget = 34;
    public const int GiveTarget = 45;

    public static bool IsWalkTarget(in MapTile tile, PlayerTarget target) => throw new NotImplementedException();

    public static bool IsAttackTarget(in MapTile tile, PlayerTarget target) => throw new NotImplementedException();

    public static bool IsGiveTarget(in MapTile tile, PlayerTarget target) => throw new NotImplementedException();

    public static int ActionMarker(in MapTile tile, PlayerTarget target) => throw new NotImplementedException();
}
