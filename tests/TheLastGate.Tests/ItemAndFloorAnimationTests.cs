using TheLastGate.Net;
using TheLastGate.State;
using TheLastGate.World;

namespace TheLastGate.Tests;

public class ItemAndFloorAnimationTests
{
    private const byte AbsoluteSetMap = 0x80;
    private const byte FloorOnly = 0b0000_0001;
    private const byte ItemSpriteAndStatus = 0b0001_1000;
    private const int TileIndex = 10 * WorldMap.Size + 10;
    private const int Sprite = 5000;
    private const int ShimmeringFloor = 16980;
    private const int EvenServerTick = 50;

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void Item_StaysStillWithoutAnimation(int status)
    {
        Assert.Equal(Sprite, Run(Item(status), EvenServerTick, 3).AnimatedItemSprite);
    }

    [Theory]
    [InlineData(2, 0)]
    [InlineData(3, 2)]
    [InlineData(5, 6)]
    [InlineData(6, 0)]
    [InlineData(7, 1)]
    [InlineData(12, 4)]
    [InlineData(18, 2)]
    public void Item_ShowsTheFrameForItsStatus(int status, int frame)
    {
        Assert.Equal(Sprite + frame, Run(Item(status), EvenServerTick).AnimatedItemSprite);
    }

    [Fact]
    public void Item_MovesOnEveryOtherServerTick()
    {
        Assert.Equal(Sprite + 2, Run(Item(2), EvenServerTick, 1).AnimatedItemSprite);
        Assert.Equal(Sprite + 2, Run(Item(2), EvenServerTick, 2).AnimatedItemSprite);
        Assert.Equal(Sprite + 4, Run(Item(2), EvenServerTick, 3).AnimatedItemSprite);
    }

    [Theory]
    [InlineData(5, 0)]
    [InlineData(7, 0)]
    [InlineData(15, 0)]
    [InlineData(20, 0)]
    public void Item_StartsItsLoopAgainAfterTheLastFrame(int lastStatus, int firstFrame)
    {
        Assert.Equal(Sprite + firstFrame, Run(Item(lastStatus), EvenServerTick, 1).AnimatedItemSprite);
    }

    [Fact]
    public void Item_WithStatus21PlaysSixtyFourFramesByTheClientsTickCount()
    {
        Assert.Equal(Sprite + 1, Run(Item(21), EvenServerTick).AnimatedItemSprite);
        Assert.Equal(Sprite + 1, Run(Item(21), EvenServerTick, 64).AnimatedItemSprite);
    }

    [Fact]
    public void Item_WithAnUnknownStatusStaysStillAndIsReported()
    {
        var state = new GameState();
        var reports = new List<string>();
        state.UnusualDataSeen += reports.Add;

        new TickProcessor(state).Process(TestCommand.Payload(Item(30)));

        Assert.Equal(Sprite, state.Map.Tiles[TileIndex].AnimatedItemSprite);
        Assert.Single(reports);
    }

    [Fact]
    public void NoItem_ShowsNoItemSprite()
    {
        Assert.Equal(0, Run(Item(2, 0), EvenServerTick).AnimatedItemSprite);
    }

    [Fact]
    public void ClientTick_CountsEveryTickPlayed()
    {
        var state = new GameState();
        var processor = new TickProcessor(state);

        for (var tick = 0; tick < 5; tick++)
        {
            processor.Process([]);
        }

        Assert.Equal(5, state.ClientTick);
    }

    [Theory]
    [InlineData(6160, ShimmeringFloor)]
    [InlineData(6167, ShimmeringFloor + 3)]
    [InlineData(6169, ShimmeringFloor + 4)]
    public void FloorSprite_FlickersTheShimmeringFloorWhenItsTurnComes(int clientTick, int sprite)
    {
        Assert.Equal(sprite, MapSpriteRules.FloorSprite(FloorAt100By200(ShimmeringFloor), clientTick));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6170)]
    public void FloorSprite_LeavesTheShimmeringFloorStillAtOtherTimes(int clientTick)
    {
        Assert.Equal(ShimmeringFloor, MapSpriteRules.FloorSprite(FloorAt100By200(ShimmeringFloor), clientTick));
    }

    [Fact]
    public void FloorSprite_DoesNotFlickerOtherFloors()
    {
        Assert.Equal(1002, MapSpriteRules.FloorSprite(FloorAt100By200(1002), 6167));
    }

    private static TestCommand Item(int status, int sprite = Sprite) =>
        TestCommand.Of(AbsoluteSetMap)
            .Byte(ItemSpriteAndStatus)
            .UInt16(TileIndex)
            .UInt16((ushort)sprite)
            .Byte((byte)status);

    private static MapTile FloorAt100By200(int floor)
    {
        var state = new GameState();
        new TickProcessor(state).Process(
            TestCommand.Payload(
                TestCommand.Of(ServerCommand.SetOrigin).UInt16(90).UInt16(190),
                TestCommand.Of(AbsoluteSetMap).Byte(FloorOnly).UInt16(TileIndex).UInt16((ushort)floor)));
        return state.Map.Tiles[TileIndex];
    }

    private static MapTile Run(TestCommand item, int serverTick, int moreTicks = 0)
    {
        var state = new GameState();
        var processor = new TickProcessor(state);
        processor.Process(TestCommand.Payload(item, TestCommand.Of(ServerCommand.Tick).Byte((byte)serverTick)));
        for (var tick = 0; tick < moreTicks; tick++)
        {
            processor.Process([]);
        }

        return state.Map.Tiles[TileIndex];
    }
}
