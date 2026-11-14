using TheLastGate.Net;
using TheLastGate.State;
using TheLastGate.World;

namespace TheLastGate.Tests;

public class MagicGlowTests
{
    private const byte AbsoluteSetMap = 0x80;
    private const byte FlagsOnly = 0b0000_0010;
    private const int TileIndex = 1000;

    [Fact]
    public void For_NoGlowWithoutMagic()
    {
        Assert.Null(MagicGlow.For(Tile(0)));
    }

    [Theory]
    [InlineData(22, true, false, false)]
    [InlineData(25, false, true, false)]
    [InlineData(28, false, false, true)]
    public void For_TakesEachColourFromItsOwnMagicBits(int shift, bool red, bool green, bool blue)
    {
        Assert.Equal(new MagicGlow(red, green, blue, 3), MagicGlow.For(Tile(3u << shift)));
    }

    [Fact]
    public void For_MixesColoursAndTakesTheStrongestCount()
    {
        Assert.Equal(new MagicGlow(true, false, true, 5), MagicGlow.For(Tile((2u << 22) | (5u << 28))));
        Assert.Equal(new MagicGlow(true, true, true, 7), MagicGlow.For(Tile((7u << 22) | (1u << 25) | (4u << 28))));
    }

    private static MapTile Tile(uint flags)
    {
        var state = new GameState();
        new TickProcessor(state).Process(
            TestCommand.Payload(TestCommand.Of(AbsoluteSetMap).Byte(FlagsOnly).UInt16(TileIndex).UInt32(flags)));
        return state.Map.Tiles[TileIndex];
    }
}
