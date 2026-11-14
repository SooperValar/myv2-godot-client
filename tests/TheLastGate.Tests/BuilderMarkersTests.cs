using TheLastGate.Net;
using TheLastGate.State;
using TheLastGate.World;

namespace TheLastGate.Tests;

public class BuilderMarkersTests
{
    private const byte AbsoluteSetMap = 0x80;
    private const byte Flags2Only = 0b0000_0100;
    private const int TileIndex = 1000;

    [Fact]
    public void For_NothingWithoutTileRules()
    {
        Assert.Empty(BuilderMarkers.For(Tile(TileRules.None)));
    }

    [Theory]
    [InlineData(TileRules.Bank, 60)]
    [InlineData(TileRules.SightBlock, 84)]
    [InlineData(TileRules.NoExpire, 82)]
    [InlineData(TileRules.UnnamedTopBit, 72)]
    public void For_MarksEachRuleWithItsSprite(TileRules rules, int sprite)
    {
        Assert.Equal([sprite], BuilderMarkers.For(Tile(rules)));
    }

    [Fact]
    public void For_StacksSeveralRulesInTheCurrentClientsOrder()
    {
        Assert.Equal(
            [55, 58, 57, 72],
            BuilderMarkers.For(
                Tile(TileRules.NoLag | TileRules.NoFight | TileRules.MoveBlock | TileRules.UnnamedTopBit)));
    }

    [Fact]
    public void For_EveryRuleTogether()
    {
        var allRules = (TileRules)0x80006FFFu;

        Assert.Equal([55, 84, 56, 75, 58, 59, 60, 61, 62, 73, 57, 76, 77, 82, 72], BuilderMarkers.For(Tile(allRules)));
    }

    private static MapTile Tile(TileRules rules)
    {
        var state = new GameState();
        new TickProcessor(state).Process(
            TestCommand.Payload(TestCommand.Of(AbsoluteSetMap).Byte(Flags2Only).UInt16(TileIndex).UInt32((uint)rules)));
        return state.Map.Tiles[TileIndex];
    }
}
