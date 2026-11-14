using TheLastGate.Net;
using TheLastGate.State;
using TheLastGate.World;

namespace TheLastGate.Tests;

public class MapEffectsTests
{
    private const byte AbsoluteSetMap = 0x80;
    private const byte FlagsAndCharacterLook = 0b0010_0010;
    private const int TileIndex = 1000;

    [Theory]
    [InlineData(MapFlags.Injured, 1079)]
    [InlineData(MapFlags.Injured | MapFlags.Injured1, 1080)]
    [InlineData(MapFlags.Injured | MapFlags.Injured2, 1081)]
    [InlineData(MapFlags.Injured | MapFlags.Injured1 | MapFlags.Injured2, 1082)]
    [InlineData(MapFlags.Injured | MapFlags.CriticallyHit, 1206)]
    [InlineData(MapFlags.Injured | MapFlags.Injured1 | MapFlags.Injured2 | MapFlags.CriticallyHit, 1209)]
    [InlineData(MapFlags.Injured1, 0)]
    [InlineData(MapFlags.None, 0)]
    public void InjurySprite_PicksTheMarkForTheInjuryBits(MapFlags flags, int sprite)
    {
        Assert.Equal(sprite, MapEffects.InjurySprite(Tile(flags)));
    }

    [Theory]
    [InlineData(1u, 280)]
    [InlineData(18u, 297)]
    [InlineData(0u, 0)]
    public void DeathSprite_StepsThroughTheDeathAnimation(uint step, int sprite)
    {
        Assert.Equal(sprite, MapEffects.DeathSprite(Tile((MapFlags)(step << 17))));
    }

    [Fact]
    public void DeathFollowsCharacter_OnlyWhileACharacterIsDrawnThere()
    {
        Assert.True(MapEffects.DeathFollowsCharacter(Tile(MapFlags.None, 3000)));
        Assert.False(MapEffects.DeathFollowsCharacter(Tile(MapFlags.None)));
    }

    [Theory]
    [InlineData(1u, 240)]
    [InlineData(28u, 267)]
    [InlineData(0u, 0)]
    public void TombSprite_StepsThroughTheGraveRising(uint step, int sprite)
    {
        Assert.Equal(sprite, MapEffects.TombSprite(Tile((MapFlags)(step << 12))));
    }

    private static MapTile Tile(MapFlags flags, int character = 0)
    {
        var state = new GameState();
        new TickProcessor(state).Process(
            TestCommand.Payload(
                TestCommand.Of(AbsoluteSetMap)
                    .Byte(FlagsAndCharacterLook)
                    .UInt16(TileIndex)
                    .UInt32((uint)flags)
                    .UInt16((ushort)character)
                    .Bytes(0, 0)));
        return state.Map.Tiles[TileIndex];
    }
}
