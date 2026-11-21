using TheLastGate.Net;
using TheLastGate.State;

namespace TheLastGate.Tests;

public class CharacterAnimationTests
{
    private const byte AbsoluteSetMap = 0x80;
    private const byte FlagsAndCharacter = 0b1110_0010;
    private const byte CharacterLookOnly = 0b0010_0000;
    private const int TileIndex = 1000;
    private const int Sprite = 2000;
    private const int SpecialIdleSprite = 22480;
    private const int OneStepEveryTick = 100;
    private const int Attack = 0;
    private const int Pickup = 1;
    private const int Cast = 9;
    private const int UnknownAction = 10;
    private const int ActionPastTheTable = 11;
    private const int PlayerTextColour = 5;

    [Fact]
    public void Idle_ShowsTheFrameForItsDirection()
    {
        var tile = Run(Character(1), 50);

        Assert.Equal(Sprite + 8, tile.AnimatedCharacterSprite);
        Assert.Equal((0, 0), (tile.CharacterOffsetX, tile.CharacterOffsetY));
    }

    [Fact]
    public void Idle_CyclesTheIdleFramesOfTheSpecialIdleSprite()
    {
        Assert.Equal(SpecialIdleSprite + 8 + 1, Run(Character(1, SpecialIdleSprite), 50).AnimatedCharacterSprite);
        Assert.Equal(SpecialIdleSprite + 8, Run(Character(1, SpecialIdleSprite), 50, 7).AnimatedCharacterSprite);
    }

    [Fact]
    public void Idle_FacingUpCyclesEvenWhileStunnedLikeTheCurrentClient()
    {
        var facingUp = Run(Character(0, SpecialIdleSprite, flags: MapFlags.Stunned), 50);
        var facingDown = Run(Character(1, SpecialIdleSprite, flags: MapFlags.Stunned), 50);

        Assert.Equal(SpecialIdleSprite + 1, facingUp.AnimatedCharacterSprite);
        Assert.Equal(SpecialIdleSprite + 8, facingDown.AnimatedCharacterSprite);
    }

    [Theory]
    [InlineData(19, 67, -6, 3)]
    [InlineData(27, 75, 6, -3)]
    [InlineData(35, 83, -6, -3)]
    [InlineData(43, 91, 6, 3)]
    public void Walk_ShowsTheFrameAndHowFarAlongTheTileItIs(int status, int frame, int offsetX, int offsetY)
    {
        var tile = Run(Character(status), 50);

        Assert.Equal(Sprite + frame, tile.AnimatedCharacterSprite);
        Assert.Equal((offsetX, offsetY), (tile.CharacterOffsetX, tile.CharacterOffsetY));
    }

    [Fact]
    public void Walk_MovesOnAFrameEachTickItsSpeedAllows()
    {
        var tile = Run(Character(19), 50, 1);

        Assert.Equal(Sprite + 68, tile.AnimatedCharacterSprite);
        Assert.Equal((-8, 4), (tile.CharacterOffsetX, tile.CharacterOffsetY));
    }

    [Fact]
    public void Walk_StartsTheStepAgainAfterItsLastFrame()
    {
        Assert.Equal(Sprite + 64, Run(Character(23), 50, 1).AnimatedCharacterSprite);
    }

    [Theory]
    [InlineData(54, 100, -16, 0)]
    [InlineData(66, 108, 0, -8)]
    [InlineData(78, 116, 0, 8)]
    [InlineData(90, 124, 16, 0)]
    public void DiagonalWalk_SpreadsEightFramesOverTwelveStepsAndMovesOneAxis(
        int status,
        int frame,
        int offsetX,
        int offsetY)
    {
        var tile = Run(Character(status), 50);

        Assert.Equal(Sprite + frame, tile.AnimatedCharacterSprite);
        Assert.Equal((offsetX, offsetY), (tile.CharacterOffsetX, tile.CharacterOffsetY));
    }

    [Theory]
    [InlineData(96, 128)]
    [InlineData(101, 133)]
    [InlineData(159, 191)]
    public void Turn_ShowsItsFrameWithoutMoving(int status, int frame)
    {
        var tile = Run(Character(status), 50);

        Assert.Equal(Sprite + frame, tile.AnimatedCharacterSprite);
        Assert.Equal((0, 0), (tile.CharacterOffsetX, tile.CharacterOffsetY));
    }

    [Theory]
    [InlineData(Attack, 195)]
    [InlineData(Pickup, 227)]
    [InlineData(Cast, 419)]
    [InlineData(UnknownAction, 323)]
    public void Action_ShowsTheRowOfFramesForWhatTheCharacterDoes(int action, int frame)
    {
        Assert.Equal(Sprite + frame, Run(Character(163, action: action), 50).AnimatedCharacterSprite);
    }

    [Fact]
    public void Action_MovesOnAtTheActionsSpeed()
    {
        var tile = Run(Character(163, action: Attack, speed: 250, attackSpeed: 150), 50, 1);

        Assert.Equal(Sprite + 196, tile.AnimatedCharacterSprite);
    }

    [Fact]
    public void Action_CanSpillIntoTheNextDirectionLikeTheCurrentClient()
    {
        var (tile, reports) = RunWithReports(
            Character(167, action: Cast, speed: 50, castSpeed: 50, textColour: PlayerTextColour),
            1,
            1);

        Assert.Equal(Sprite + 169 + 32 + (7 << 5), tile.AnimatedCharacterSprite);
        Assert.Single(reports);
    }

    [Fact]
    public void Action_PastTheEndOfTheCurrentClientsTableUsesTheFirstRowAndIsReported()
    {
        var (tile, reports) = RunWithReports(Character(163, action: ActionPastTheTable), 50);

        Assert.Equal(Sprite + 195, tile.AnimatedCharacterSprite);
        Assert.Single(reports);
    }

    [Fact]
    public void Animation_ReportsNothingForOrdinaryStatuses()
    {
        var (_, reports) = RunWithReports(Character(19), 50, 10);

        Assert.Empty(reports);
    }

    [Fact]
    public void Stunned_HoldsTheCurrentFrame()
    {
        Assert.Equal(Sprite + 67, Run(Character(19, flags: MapFlags.Stunned), 50, 2).AnimatedCharacterSprite);
    }

    [Fact]
    public void UnknownStatus_ShowsTheFirstFrameAndKeepsTheLastOffset()
    {
        var state = new GameState();
        var reports = new List<string>();
        state.UnusualDataSeen += reports.Add;
        var processor = new TickProcessor(state);
        processor.Process(TestCommand.Payload(Character(19), Tick(50)));

        processor.Process(
            TestCommand.Payload(
                TestCommand.Of(AbsoluteSetMap).Byte(CharacterLookOnly).UInt16(TileIndex).UInt16(Sprite).Bytes(10, 0)));

        var tile = state.Map.Tiles[TileIndex];
        Assert.Equal(Sprite, tile.AnimatedCharacterSprite);
        Assert.Equal((-6, 3), (tile.CharacterOffsetX, tile.CharacterOffsetY));
        Assert.Single(reports);
    }

    [Fact]
    public void NoCharacter_ShowsNoCharacterSprite()
    {
        Assert.Equal(0, Run(Character(1, 0), 50).AnimatedCharacterSprite);
    }

    private static TestCommand Character(
        int status,
        int sprite = Sprite,
        int action = Attack,
        MapFlags flags = MapFlags.None,
        int speed = OneStepEveryTick,
        int attackSpeed = 0,
        int castSpeed = 0,
        int textColour = 0) =>
        TestCommand.Of(AbsoluteSetMap)
            .Byte(FlagsAndCharacter)
            .UInt16(TileIndex)
            .UInt32((uint)flags)
            .UInt16((ushort)sprite)
            .Bytes((byte)status, (byte)action)
            .UInt16(77)
            .UInt16(88)
            .UInt16((ushort)speed)
            .Byte(100)
            .UInt16((ushort)castSpeed)
            .UInt16((ushort)attackSpeed)
            .UInt16(0)
            .Byte((byte)textColour);

    private static TestCommand Tick(int serverTick) => TestCommand.Of(ServerCommand.Tick).Byte((byte)serverTick);

    private static MapTile Run(TestCommand character, int serverTick, int moreTicks = 0) =>
        RunWithReports(character, serverTick, moreTicks).Tile;

    private static (MapTile Tile, List<string> Reports) RunWithReports(
        TestCommand character,
        int serverTick,
        int moreTicks = 0)
    {
        var state = new GameState();
        var reports = new List<string>();
        state.UnusualDataSeen += reports.Add;
        var processor = new TickProcessor(state);
        processor.Process(TestCommand.Payload(character, Tick(serverTick)));
        for (var tick = 0; tick < moreTicks; tick++)
        {
            processor.Process([]);
        }

        return (state.Map.Tiles[TileIndex], reports);
    }
}
