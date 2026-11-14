using System;

namespace TheLastGate.World;

public static class AnimationSpeed
{
    public const int SpeedMax = 300;
    public const int CycleLength = 200;

    public static int StepsThisTick(int speed, int serverTick, bool instantCast = false) =>
        throw new NotImplementedException();

    public static int MoveSteps(int speed, int moveSpeed, int serverTick) => throw new NotImplementedException();

    public static int ActionSteps(
        int action,
        int speed,
        int attackSpeed,
        int castSpeed,
        int textColour,
        int serverTick) =>
        throw new NotImplementedException();

    public static int WalkOffset(
        int framesDone,
        int framesInStep,
        int speed,
        int moveSpeed,
        bool advancing,
        int serverTick) =>
        throw new NotImplementedException();
}
