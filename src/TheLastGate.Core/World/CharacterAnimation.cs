using System;
using TheLastGate.State;

namespace TheLastGate.World;

public static class CharacterAnimation
{
    private const int IdleUp = 0;
    private const int LastIdle = 7;
    private const int IdleFrames = 8;
    private const int SpecialIdleSprite = 22480;
    private const int FirstStraightWalk = 16;
    private const int StraightWalkSteps = 8;
    private const int FirstDiagonalWalk = 48;
    private const int DiagonalWalkSteps = 12;
    private const int WalkFrames = 8;
    private const int FirstTurn = 96;
    private const int TurnSteps = 4;
    private const int FirstAction = 160;
    private const int LastAction = 191;
    private const int ActionSteps = 8;
    private const int TurnAndActionFrameShift = 32;
    private const int ActionRowShift = 5;

    private static readonly int[] ActionRows = [0, 1, 1, 6, 6, 2, 3, 4, 5, 7, 4];

    private static readonly WalkBlock[] StraightWalks =
    [
        new(16, 64, -1, 2, 1, 4),
        new(24, 72, 1, 2, -1, 4),
        new(32, 80, -1, 2, -1, 4),
        new(40, 88, 1, 2, 1, 4),
    ];

    private static readonly WalkBlock[] DiagonalWalks =
    [
        new(48, 96, -1, 1, 0, 1),
        new(60, 104, 0, 1, -1, 2),
        new(72, 112, 0, 1, 1, 2),
        new(84, 120, 1, 1, 0, 1),
    ];

    internal static void Advance(ref MapTile tile, int serverTick, Action<string> reportUnusualData)
    {
        if (tile.CharacterSprite == 0)
        {
            tile.AnimatedCharacterSprite = 0;
            return;
        }

        var advancing = !((MapFlags)tile.Flags).HasFlag(MapFlags.Stunned);
        var status = tile.CharacterStatus;
        int frame;
        switch (status)
        {
            case <= LastIdle:
                frame = Idle(ref tile, advancing, serverTick);
                break;
            case >= FirstStraightWalk and < FirstDiagonalWalk:
                frame = Walk(
                    ref tile,
                    StraightWalks[(status - FirstStraightWalk) / StraightWalkSteps],
                    StraightWalkSteps,
                    advancing,
                    serverTick,
                    reportUnusualData);
                break;
            case >= FirstDiagonalWalk and < FirstTurn:
                frame = Walk(
                    ref tile,
                    DiagonalWalks[(status - FirstDiagonalWalk) / DiagonalWalkSteps],
                    DiagonalWalkSteps,
                    advancing,
                    serverTick,
                    reportUnusualData);
                break;
            case >= FirstTurn and < FirstAction:
                frame = Turn(ref tile, advancing, serverTick, reportUnusualData);
                break;
            case >= FirstAction and <= LastAction:
                frame = Act(ref tile, advancing, serverTick, reportUnusualData);
                break;
            default:
                reportUnusualData(
                    $"Character {tile.CharacterNumber} has status {status}, which the current client doesn't animate.");
                frame = 0;
                break;
        }

        tile.AnimatedCharacterSprite = tile.CharacterSprite + frame;
    }

    private static int Idle(ref MapTile tile, bool advancing, int serverTick)
    {
        tile.CharacterOffsetX = 0;
        tile.CharacterOffsetY = 0;
        var moves = AnimationSpeed.MoveSteps(tile.CharacterSpeed, tile.CharacterMoveSpeed, serverTick) != 0;
        if (tile.CharacterStatus == IdleUp || (moves && advancing))
        {
            tile.IdleFrame = (tile.IdleFrame + 1) % IdleFrames;
        }

        return tile.CharacterStatus * IdleFrames
            + (tile.CharacterSprite == SpecialIdleSprite ? tile.IdleFrame : 0);
    }

    private static int Walk(
        ref MapTile tile,
        WalkBlock block,
        int stepsInWalk,
        bool advancing,
        int serverTick,
        Action<string> reportUnusualData)
    {
        var stepsDone = tile.CharacterStatus - block.FirstStatus;
        var distance = AnimationSpeed.WalkOffset(
            stepsDone,
            stepsInWalk,
            tile.CharacterSpeed,
            tile.CharacterMoveSpeed,
            advancing,
            serverTick);
        tile.CharacterOffsetX = block.XSign * (distance / block.XDivisor);
        tile.CharacterOffsetY = block.YSign * (distance / block.YDivisor);
        var frame = stepsDone * WalkFrames / stepsInWalk + block.FirstFrame;
        MoveOn(
            ref tile,
            block.FirstStatus,
            stepsInWalk,
            AnimationSpeed.MoveSteps(tile.CharacterSpeed, tile.CharacterMoveSpeed, serverTick),
            advancing,
            reportUnusualData);
        return frame;
    }

    private static int Turn(ref MapTile tile, bool advancing, int serverTick, Action<string> reportUnusualData)
    {
        tile.CharacterOffsetX = 0;
        tile.CharacterOffsetY = 0;
        var frame = tile.CharacterStatus + TurnAndActionFrameShift;
        MoveOn(
            ref tile,
            tile.CharacterStatus - (tile.CharacterStatus - FirstTurn) % TurnSteps,
            TurnSteps,
            AnimationSpeed.MoveSteps(tile.CharacterSpeed, tile.CharacterMoveSpeed, serverTick),
            advancing,
            reportUnusualData);
        return frame;
    }

    private static int Act(ref MapTile tile, bool advancing, int serverTick, Action<string> reportUnusualData)
    {
        tile.CharacterOffsetX = 0;
        tile.CharacterOffsetY = 0;
        var action = tile.CharacterStatusOffset;
        var row = 0;
        if (action < ActionRows.Length)
        {
            row = ActionRows[action];
        }
        else
        {
            reportUnusualData(
                $"Character {tile.CharacterNumber} does action {action}, past the end of the current client's action "
                + "table; showing its first row of frames.");
        }

        var frame = tile.CharacterStatus + TurnAndActionFrameShift + (row << ActionRowShift);
        var steps = AnimationSpeed.ActionSteps(
            action,
            tile.CharacterSpeed,
            tile.CharacterAttackSpeed,
            tile.CharacterCastSpeed,
            tile.CharacterTextColour,
            serverTick);
        MoveOn(
            ref tile,
            tile.CharacterStatus - (tile.CharacterStatus - FirstAction) % ActionSteps,
            ActionSteps,
            steps,
            advancing,
            reportUnusualData);
        return frame;
    }

    // Wraps once, as the current client does, so a jump of more than a whole block lands outside it.
    private static void MoveOn(
        ref MapTile tile,
        int firstStatus,
        int statusCount,
        int steps,
        bool advancing,
        Action<string> reportUnusualData)
    {
        if (steps == 0 || !advancing)
        {
            return;
        }

        var lastStatus = firstStatus + statusCount - 1;
        var next = tile.CharacterStatus + steps;
        if (next > lastStatus)
        {
            next = firstStatus + (next - lastStatus - 1);
            if (next > lastStatus)
            {
                reportUnusualData(
                    $"Character {tile.CharacterNumber}'s animation jumped past its last frame into status {next}, "
                    + "as the current client's does.");
            }
        }

        tile.CharacterStatus = next;
    }

    private readonly record struct WalkBlock(
        int FirstStatus,
        int FirstFrame,
        int XSign,
        int XDivisor,
        int YSign,
        int YDivisor);
}
