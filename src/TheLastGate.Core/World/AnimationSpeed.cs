using System;

namespace TheLastGate.World;

public static class AnimationSpeed
{
    public const int SpeedMax = 300;
    public const int CycleLength = 200;

    private const int FastestSpeed = SpeedMax - 1;
    private const int Attack = 0;
    private const int SecondAttack = 5;
    private const int ThirdAttack = 6;
    private const int Cast = 9;
    private const int PlayerTextColour = 5;

    public static int StepsThisTick(int speed, int serverTick, bool instantCast = false)
    {
        var stepsPerCycle = SpeedMax - speed;
        return stepsPerCycle * (serverTick + 1) / CycleLength
            - stepsPerCycle * serverTick / CycleLength
            + (instantCast ? InstantCastBonus(speed) : 0);
    }

    public static int MoveSteps(int speed, int moveSpeed, int serverTick) =>
        StepsThisTick(KeepInRange(speed - moveSpeed), serverTick);

    public static int ActionSteps(
        int action,
        int speed,
        int attackSpeed,
        int castSpeed,
        int textColour,
        int serverTick) =>
        action switch
        {
            Attack or SecondAttack or ThirdAttack => StepsThisTick(KeepInRange(speed - attackSpeed), serverTick),
            Cast => StepsThisTick(
                KeepInRange(2 * speed - 2 * castSpeed),
                serverTick,
                textColour == PlayerTextColour),
            _ => StepsThisTick(KeepInRange(speed), serverTick),
        };

    // The share of a tile walked so far: the ticks since the step began over all the ticks the step will take,
    // counted from the speed's pattern of ticks that move a frame.
    public static int WalkOffset(
        int framesDone,
        int framesInStep,
        int speed,
        int moveSpeed,
        bool advancing,
        int serverTick)
    {
        if (!advancing)
        {
            return IsometricProjection.TileSize * framesDone / framesInStep;
        }

        var effectiveSpeed = KeepInRange(speed - moveSpeed);
        var tick = serverTick;
        var ticksDone = 0;
        for (var framesLeft = framesDone; framesLeft > 0;)
        {
            tick = Previous(tick);
            ticksDone++;
            if (StepsThisTick(effectiveSpeed, tick) != 0)
            {
                framesLeft--;
            }
        }

        while (true)
        {
            tick = Previous(tick);
            if (StepsThisTick(effectiveSpeed, tick) != 0)
            {
                break;
            }

            ticksDone++;
        }

        tick = serverTick;
        var ticksInStep = ticksDone;
        var framesToGo = framesInStep - framesDone;
        while (true)
        {
            if (StepsThisTick(effectiveSpeed, tick) != 0)
            {
                framesToGo--;
            }

            if (framesToGo < 1)
            {
                break;
            }

            tick = Next(tick);
            ticksInStep++;
        }

        return IsometricProjection.TileSize * ticksDone / (ticksInStep + 1);
    }

    private static int InstantCastBonus(int speed) => speed switch
    {
        <= 0 => 8,
        <= 33 => 4,
        <= 66 => 2,
        <= 99 => 1,
        _ => 0,
    };

    private static int KeepInRange(int speed) => Math.Clamp(speed, 0, FastestSpeed);

    private static int Previous(int tick) => tick == 0 ? CycleLength - 1 : tick - 1;

    private static int Next(int tick) => tick == CycleLength - 1 ? 0 : tick + 1;
}
