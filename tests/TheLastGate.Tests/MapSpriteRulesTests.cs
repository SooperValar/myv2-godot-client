using TheLastGate.Net;
using TheLastGate.State;
using TheLastGate.World;

namespace TheLastGate.Tests;

public class MapSpriteRulesTests
{
    private const byte AbsoluteSetMap = 0x80;
    private const byte FloorFlagsObjectAndCharacter = 0b0010_1011;
    private const int Centre = WorldMap.Size / 2;
    private const int FacingEast = 1;
    private const int Wall = 1234;
    private const int Waypoint = 598;
    private const int LitWaypoint = 599;
    private static readonly (int Column, int Row) BehindThePlayer = (Centre + 3, Centre - 7);
    private static readonly (int Column, int Row) InFrontOfThePlayer = (Centre - 17, Centre + 13);
    private static readonly (int Column, int Row) HiddenCorner = (WorldMap.Size - 1, 0);

    [Fact]
    public void FloorSprite_IsTheTilesFloor()
    {
        var (tile, _) = Tile(BehindThePlayer, 1002);

        Assert.Equal(1002, MapSpriteRules.FloorSprite(tile));
    }

    [Fact]
    public void FloorSprite_ShowsNothingOnATileOutOfSight()
    {
        var (tile, _) = Tile(BehindThePlayer, 1002, flags: MapFlags.Invisible);

        Assert.Equal(0, MapSpriteRules.FloorSprite(tile));
    }

    [Theory]
    [InlineData(MapFlags.None, SpriteTint.None)]
    [InlineData(MapFlags.Infrared, SpriteTint.Infrared)]
    [InlineData(MapFlags.Underwater, SpriteTint.Underwater)]
    [InlineData(MapFlags.Infrared | MapFlags.Underwater, SpriteTint.Infrared | SpriteTint.Underwater)]
    [InlineData(MapFlags.Stoned | MapFlags.Bloody, SpriteTint.None)]
    public void EnvironmentTint_TintsFloorsAndObjectsForInfraredAndUnderwater(MapFlags flags, SpriteTint tint)
    {
        var (tile, _) = Tile(BehindThePlayer, flags: flags);

        Assert.Equal(tint, MapSpriteRules.EnvironmentTint(tile));
    }

    [Theory]
    [InlineData(MapFlags.None, SpriteTint.None)]
    [InlineData(MapFlags.Stoned, SpriteTint.Grey)]
    [InlineData(MapFlags.Bloody, SpriteTint.Infrared)]
    [InlineData(MapFlags.Infrared, SpriteTint.Infrared)]
    [InlineData(MapFlags.Underwater, SpriteTint.Underwater)]
    [InlineData(MapFlags.Stoned | MapFlags.Underwater, SpriteTint.Grey | SpriteTint.Underwater)]
    public void CharacterTint_AlsoGreysStonedAndReddensBloodyCharacters(MapFlags flags, SpriteTint tint)
    {
        var (tile, _) = Tile(BehindThePlayer, flags: flags, character: 3021);

        Assert.Equal(tint, MapSpriteRules.CharacterTint(tile));
    }

    [Fact]
    public void ObjectSprite_ShowsTheLowWallBehindThePlayer()
    {
        Assert.Equal(Wall + 1, ObjectAt(BehindThePlayer, Wall));
    }

    [Fact]
    public void ObjectSprite_ShowsTheWholeWallWhenWallsArentHidden()
    {
        Assert.Equal(Wall, ObjectAt(BehindThePlayer, Wall, hideWalls: false));
    }

    [Fact]
    public void ObjectSprite_ShowsAWholeItem()
    {
        Assert.Equal(Wall, ObjectAt(BehindThePlayer, Wall, MapFlags.IsItem));
    }

    [Fact]
    public void ObjectSprite_ShowsTheWholeWallInFrontOfThePlayer()
    {
        Assert.Equal(Wall, ObjectAt(InFrontOfThePlayer, Wall));
    }

    [Fact]
    public void ObjectSprite_ShowsNothingWhereThereIsNoObject()
    {
        Assert.Equal(0, ObjectAt(BehindThePlayer, 0));
    }

    [Fact]
    public void ObjectSprite_ShowsNothingOnAnInvisibleTileOrInTheHiddenCorner()
    {
        Assert.Equal(0, ObjectAt(BehindThePlayer, Wall, MapFlags.IsItem | MapFlags.Invisible));
        Assert.Equal(0, ObjectAt(HiddenCorner, Wall, MapFlags.IsItem));
    }

    [Theory]
    [InlineData(16350, 457)]
    [InlineData(16358, 456)]
    [InlineData(16366, 455)]
    [InlineData(16374, 466)]
    [InlineData(16382, 459)]
    [InlineData(16390, 458)]
    [InlineData(16398, 468)]
    [InlineData(16406, 467)]
    [InlineData(16414, 467)]
    public void ObjectSprite_ShowsALowMineWallBehindThePlayer(int mineWall, int lowWall)
    {
        Assert.Equal(lowWall, ObjectAt(BehindThePlayer, mineWall, MapFlags.IsItem));
    }

    [Theory]
    [InlineData(16357)]
    [InlineData(16365)]
    [InlineData(16421)]
    [InlineData(16422)]
    public void ObjectSprite_KeepsAMineWallThatHasNoLowVersion(int mineWall)
    {
        Assert.Equal(mineWall, ObjectAt(BehindThePlayer, mineWall, MapFlags.IsItem));
    }

    [Fact]
    public void ObjectSprite_KeepsTheWholeMineWallThePlayerFaces()
    {
        Assert.Equal(16350, ObjectAt((Centre + 1, Centre), 16350, MapFlags.IsItem, direction: FacingEast));
    }

    [Fact]
    public void ObjectSprite_KeepsTheWholeMineWallWhenWallsArentHidden()
    {
        Assert.Equal(16350, ObjectAt(BehindThePlayer, 16350, MapFlags.IsItem, false));
    }

    [Theory]
    [InlineData(0, 1002)]
    [InlineData(31, 549)]
    public void ObjectSprite_LightsAWaypointTheCharacterHasFound(int waypointBit, int floor)
    {
        var (tile, player) = Tile(BehindThePlayer, floor, Waypoint, MapFlags.IsItem, waypoints: 1u << waypointBit);

        Assert.Equal(
            LitWaypoint,
            MapSpriteRules.ObjectSprite(tile, BehindThePlayer.Column, BehindThePlayer.Row, player, true));
    }

    [Fact]
    public void ObjectSprite_LeavesAWaypointDarkUntilItsFound()
    {
        var (unfound, player) = Tile(BehindThePlayer, 1002, Waypoint, MapFlags.IsItem, waypoints: 1u << 1);
        var (otherFloor, playerWithIt) = Tile(BehindThePlayer, 1008, Waypoint, MapFlags.IsItem, waypoints: 1u << 0);

        Assert.Equal(
            Waypoint,
            MapSpriteRules.ObjectSprite(unfound, BehindThePlayer.Column, BehindThePlayer.Row, player, true));
        Assert.Equal(
            Waypoint,
            MapSpriteRules.ObjectSprite(otherFloor, BehindThePlayer.Column, BehindThePlayer.Row, playerWithIt, true));
    }

    [Fact]
    public void CharacterSprite_ShowsTheCharacter()
    {
        var (tile, _) = Tile(BehindThePlayer, character: 3021);

        Assert.Equal(3021, MapSpriteRules.CharacterSprite(tile, BehindThePlayer.Column, BehindThePlayer.Row));
    }

    [Fact]
    public void CharacterSprite_ShowsNothingOnAnInvisibleTileOrInTheHiddenCorner()
    {
        var (invisible, _) = Tile(BehindThePlayer, flags: MapFlags.Invisible, character: 3021);
        var (cornered, _) = Tile(HiddenCorner, character: 3021);

        Assert.Equal(0, MapSpriteRules.CharacterSprite(invisible, BehindThePlayer.Column, BehindThePlayer.Row));
        Assert.Equal(0, MapSpriteRules.CharacterSprite(cornered, HiddenCorner.Column, HiddenCorner.Row));
    }

    private static int ObjectAt(
        (int Column, int Row) position,
        int objectSprite,
        MapFlags flags = MapFlags.None,
        bool hideWalls = true,
        int direction = 0)
    {
        var (tile, player) = Tile(position, objectSprite: objectSprite, flags: flags, direction: direction);
        return MapSpriteRules.ObjectSprite(tile, position.Column, position.Row, player, hideWalls);
    }

    private static (MapTile Tile, PlayerState Player) Tile(
        (int Column, int Row) position,
        int floor = 0,
        int objectSprite = 0,
        MapFlags flags = MapFlags.None,
        int character = 0,
        int direction = 0,
        uint waypoints = 0)
    {
        var tileIndex = position.Column + position.Row * WorldMap.Size;
        var state = new GameState();
        new TickProcessor(state).Process(
            TestCommand.Payload(
                TestCommand.Of(AbsoluteSetMap)
                    .Byte(FloorFlagsObjectAndCharacter)
                    .UInt16((ushort)tileIndex)
                    .UInt16((ushort)floor)
                    .UInt32((uint)flags)
                    .UInt16((ushort)objectSprite)
                    .UInt16((ushort)character)
                    .Bytes(0, 0),
                TestCommand.Of(ServerCommand.SetCharacterDirection).Byte((byte)direction),
                TestCommand.Of(ServerCommand.SetCharacterWaypoints).UInt32(waypoints)));
        return (state.Map.Tiles[tileIndex], state.Player);
    }
}
