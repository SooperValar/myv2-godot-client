using TheLastGate.Net;
using TheLastGate.State;

namespace TheLastGate.Tests;

public class MapCommandsTests
{
    private const byte AbsoluteSetMap = 0x80;
    private const int Middle = 10 * WorldMap.Size + 10;

    [Fact]
    public void SetMap_SetsEveryFlaggedFieldOfAnAbsoluteTile()
    {
        var map = Process(
            TestCommand.Of(AbsoluteSetMap)
                .Byte(0xFF)
                .UInt16(1000)
                .UInt16(0xFFFF)
                .UInt32(0x80000001)
                .UInt32(0x12345678)
                .UInt16(0xFFFE)
                .Byte(9)
                .UInt16(5000)
                .Bytes(3, 4)
                .UInt16(77)
                .UInt16(88)
                .Byte(12)
                .Byte(95));

        var tile = map.Tiles[1000];
        Assert.Equal(65535, tile.BackgroundSprite);
        Assert.Equal(0x80000001u, tile.Flags);
        Assert.Equal(0x12345678u, tile.Flags2);
        Assert.Equal(65534, tile.ItemSprite);
        Assert.Equal(9, tile.ItemStatus);
        Assert.Equal(5000, tile.CharacterSprite);
        Assert.Equal(3, tile.CharacterStatus);
        Assert.Equal(4, tile.CharacterStatusOffset);
        Assert.Equal(77, tile.CharacterNumber);
        Assert.Equal(88, tile.CharacterId);
        Assert.Equal(12, tile.CharacterSpeed);
        Assert.Equal(95, tile.CharacterHealthPercent);
    }

    [Fact]
    public void TileGlow_SetsTheTilesGlow()
    {
        var map = Process(TestCommand.Of(ServerCommand.TileGlow).UInt16(300).Byte(12));

        Assert.Equal(12, map.Tiles[300].Glow);
    }

    [Fact]
    public void SetMap_ChangesOnlyTheFlaggedFields()
    {
        var map = Process(
            TestCommand.Of(AbsoluteSetMap).Byte(0b0000_0001).UInt16(1000).UInt16(42),
            TestCommand.Of(AbsoluteSetMap).Byte(0b0001_0000).UInt16(1000).Byte(6));

        Assert.Equal(42, map.Tiles[1000].BackgroundSprite);
        Assert.Equal(6, map.Tiles[1000].ItemStatus);
    }

    [Fact]
    public void SetMap_CountsRelativeTilesFromTheLastTileUpdatedInTheTick()
    {
        var map = Process(
            TestCommand.Of(0x80 | 1).Byte(0b0000_0001).UInt16(11),
            TestCommand.Of(0x80 | 3).Byte(0b0000_0001).UInt16(13),
            TestCommand.Of(AbsoluteSetMap).Byte(0b0000_0001).UInt16(100).UInt16(21),
            TestCommand.Of(0x80 | 2).Byte(0b0000_0001).UInt16(23));

        Assert.Equal(11, map.Tiles[0].BackgroundSprite);
        Assert.Equal(13, map.Tiles[3].BackgroundSprite);
        Assert.Equal(21, map.Tiles[100].BackgroundSprite);
        Assert.Equal(23, map.Tiles[102].BackgroundSprite);
    }

    [Fact]
    public void SetMap_StartsCountingRelativeTilesAgainEachTick()
    {
        var state = new GameState();
        var processor = new TickProcessor(state);

        processor.Process(TestCommand.Payload(TestCommand.Of(AbsoluteSetMap).Byte(0b0000_0001).UInt16(100).UInt16(1)));
        processor.Process(TestCommand.Payload(TestCommand.Of(0x80 | 1).Byte(0b0000_0001).UInt16(2)));

        Assert.Equal(2, state.Map.Tiles[0].BackgroundSprite);
    }

    [Fact]
    public void SetMap_RejectsATileOutsideTheMap()
    {
        Assert.Throws<ProtocolDesyncException>(() =>
            Process(TestCommand.Of(AbsoluteSetMap).Byte(0b0000_0001).UInt16(WorldMap.TileCount).UInt16(1)));
    }

    [Fact]
    public void SetMapLight_SetsATilesLightThenPacksTwoTilesPerByte()
    {
        var map = Process(TestCommand.Of(ServerCommand.SetMap6).UInt16(10 | (7 << 12)).Bytes(0x12, 0x34, 0x56));

        Assert.Equal(7, map.Tiles[10].Light);
        Assert.Equal(1, map.Tiles[11].Light);
        Assert.Equal(2, map.Tiles[12].Light);
        Assert.Equal(3, map.Tiles[13].Light);
        Assert.Equal(4, map.Tiles[14].Light);
        Assert.Equal(5, map.Tiles[15].Light);
        Assert.Equal(6, map.Tiles[16].Light);
        Assert.Equal(0, map.Tiles[17].Light);
    }

    [Fact]
    public void SetMapLight_SkipsPairsPastTheEndOfTheMap()
    {
        var map = Process(
            TestCommand.Of(ServerCommand.SetMap5).UInt16((WorldMap.TileCount - 1) | (7 << 12)).Byte(0x99),
            TestCommand.Of(ServerCommand.SetMap4).UInt16(3 << 12));

        Assert.Equal(7, map.Tiles[WorldMap.TileCount - 1].Light);
        Assert.Equal(3, map.Tiles[0].Light);
    }

    [Fact]
    public void SetMapLight_RejectsATileOutsideTheMap()
    {
        Assert.Throws<ProtocolDesyncException>(() =>
            Process(TestCommand.Of(ServerCommand.SetMap4).UInt16(WorldMap.TileCount)));
    }

    [Fact]
    public void SetOrigin_GivesEachTileItsWorldPosition()
    {
        var map = Process(TestCommand.Of(ServerCommand.SetOrigin).UInt16(100).UInt16(0xFFFB));

        var tile = map.Tiles[2 * WorldMap.Size + 3];
        Assert.Equal(103, tile.X);
        Assert.Equal(65533, tile.Y);
    }

    [Theory]
    [InlineData(ServerCommand.ScrollRight, 11, 10)]
    [InlineData(ServerCommand.ScrollLeft, 9, 10)]
    [InlineData(ServerCommand.ScrollDown, 10, 11)]
    [InlineData(ServerCommand.ScrollUp, 10, 9)]
    [InlineData(ServerCommand.ScrollLeftUp, 9, 9)]
    [InlineData(ServerCommand.ScrollLeftDown, 9, 11)]
    [InlineData(ServerCommand.ScrollRightUp, 11, 9)]
    [InlineData(ServerCommand.ScrollRightDown, 11, 11)]
    public void Scroll_MovesEveryTileOneStep(ServerCommand scroll, int expectedX, int expectedY)
    {
        var map = Process(TestCommand.Of(ServerCommand.SetOrigin).UInt16(0).UInt16(0), TestCommand.Of(scroll));

        Assert.Equal(expectedX, map.Tiles[Middle].X);
        Assert.Equal(expectedY, map.Tiles[Middle].Y);
    }

    [Fact]
    public void Scroll_LeavesTheUncoveredEdgeAsItWas()
    {
        var map = Process(
            TestCommand.Of(ServerCommand.SetOrigin).UInt16(0).UInt16(0),
            TestCommand.Of(ServerCommand.ScrollRight));

        Assert.Equal(WorldMap.Size - 1, map.Tiles[WorldMap.TileCount - 1].X);
        Assert.Equal(WorldMap.Size - 1, map.Tiles[WorldMap.TileCount - 1].Y);
    }

    private static WorldMap Process(params TestCommand[] commands)
    {
        var state = new GameState();
        new TickProcessor(state).Process(TestCommand.Payload(commands));
        return state.Map;
    }
}
