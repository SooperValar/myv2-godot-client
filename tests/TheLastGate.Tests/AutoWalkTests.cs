using TheLastGate.Net;
using TheLastGate.State;
using TheLastGate.World;

namespace TheLastGate.Tests;

public class AutoWalkTests
{
    private const int OriginX = 500;
    private const int OriginY = 400;
    private const int StepTicks = 4;

    private readonly List<byte[]> sent = [];
    private readonly GameState state = new();

    public AutoWalkTests()
    {
        state.AutoWalk.Send = sent.Add;
        Tick(TestCommand.Of(ServerCommand.SetOrigin).UInt16(OriginX).UInt16(OriginY));
        sent.Clear();
    }

    [Theory]
    [InlineData(289, 272, WalkDirection.North)]
    [InlineData(289, 622, WalkDirection.West)]
    [InlineData(990, 622, WalkDirection.South)]
    [InlineData(990, 272, WalkDirection.East)]
    public void EachCornerButton_WalksItsWay(int screenX, int screenY, WalkDirection direction) =>
        Assert.Equal(direction, AutoWalk.ButtonAt(screenX, screenY));

    [Theory]
    [InlineData(289 + 11, 272)]
    [InlineData(289 + 8, 272 + 8)]
    public void AClickJustInsideTheButton_Counts(int screenX, int screenY) =>
        Assert.Equal(WalkDirection.North, AutoWalk.ButtonAt(screenX, screenY));

    [Theory]
    [InlineData(289 + 12, 272)]
    [InlineData(289 + 9, 272 + 8)]
    [InlineData(640, 360)]
    public void AClickAwayFromTheButtons_IsNotOne(int screenX, int screenY) =>
        Assert.Null(AutoWalk.ButtonAt(screenX, screenY));

    [Theory]
    [InlineData(WalkDirection.North, 20, 27)]
    [InlineData(WalkDirection.West, 27, 20)]
    [InlineData(WalkDirection.South, 34, 27)]
    [InlineData(WalkDirection.East, 27, 34)]
    public void Walking_MovesToTheTileSevenAwayOnTheNextTick(WalkDirection direction, int column, int row)
    {
        state.AutoWalk.Start(direction);

        Tick();

        Assert.Equal([Move(column, row)], sent);
    }

    [Fact]
    public void Walking_StepsAgainEveryFourTicks()
    {
        state.AutoWalk.Start(WalkDirection.North);

        for (var tick = 0; tick < StepTicks; tick++)
        {
            Tick();
        }

        Assert.Single(sent);
        Tick();
        Assert.Equal(2, sent.Count);
    }

    [Fact]
    public void Stopping_SendsNoMoreSteps()
    {
        state.AutoWalk.Start(WalkDirection.North);
        Tick();

        state.AutoWalk.Stop();
        for (var tick = 0; tick < StepTicks * 2; tick++)
        {
            Tick();
        }

        Assert.Single(sent);
        Assert.False(state.AutoWalk.Walking);
    }

    [Fact]
    public void Walking_StopsByItselfAfterAThousandSteps()
    {
        const int stepsBeforeStopping = 1002;
        state.AutoWalk.Start(WalkDirection.North);

        for (var tick = 0; tick < (stepsBeforeStopping + 10) * StepTicks; tick++)
        {
            Tick();
        }

        Assert.Equal(stepsBeforeStopping, sent.Count);
        Assert.False(state.AutoWalk.Walking);
    }

    private static byte[] Move(int column, int row) =>
        MapCommandPackets.ForTile(ClientCommand.Move, OriginX + column, OriginY + row);

    private void Tick(params TestCommand[] commands) => new TickProcessor(state).Process(TestCommand.Payload(commands));
}
