using TheLastGate.Net;
using TheLastGate.State;
using TheLastGate.World;

namespace TheLastGate.Tests;

public class MapActionsTests
{
    private const byte AbsoluteSetMap = 0x80;
    private const byte FlagsObjectAndCharacter = 0b0110_1010;
    private const int OriginX = 500;
    private const int OriginY = 400;
    private const int BuildItem = 46;
    private const int Candle = 1500;
    private const int Guard = 3000;
    private const int GuardNumber = 812;
    private static readonly (float Column, float Row) OnPlayerTile = (27.5f, 27.5f);
    private static readonly (float Column, float Row) NearTheNextTile = (27.9f, 27.5f);

    [Fact]
    public void NoKeys_HighlightsTheFloorWithTheWalkCursor()
    {
        var action = Decide(new GameState(), OnPlayerTile, MapKeys.None, MapButton.None);

        Assert.Equal(new MapAction(Tile(27, 27), MapLayer.Floor, MapCursor.Walk, null, null), action);
    }

    [Fact]
    public void NoKeys_WalksOnLeftAndTurnsOnRight()
    {
        var state = State();

        Assert.Equal(
            MapCommandPackets.ForTile(ClientCommand.Move, OriginX + 27, OriginY + 27),
            Decide(state, OnPlayerTile, MapKeys.None, MapButton.Left).Command);
        Assert.Equal(
            MapCommandPackets.ForTile(ClientCommand.Turn, OriginX + 27, OriginY + 27),
            Decide(state, OnPlayerTile, MapKeys.None, MapButton.Right).Command);
    }

    [Fact]
    public void AwayFromTheMap_DoesNothing()
    {
        Assert.Equal(MapAction.Nothing, Decide(State(), null, MapKeys.None, MapButton.Left));
    }

    [Fact]
    public void BuildMode_PicksUpOnLeftAndDropsOnRightAtTheFloorTile()
    {
        var state = State(HeldItem(BuildItem));

        var hover = Decide(state, OnPlayerTile, MapKeys.Shift, MapButton.None);
        Assert.Equal(new MapAction(Tile(27, 27), MapLayer.Floor, MapCursor.None, null, null), hover);
        Assert.Equal(
            MapCommandPackets.ForTile(ClientCommand.PickUp, OriginX + 27, OriginY + 27),
            Decide(state, OnPlayerTile, MapKeys.None, MapButton.Left).Command);
        Assert.Equal(
            MapCommandPackets.ForTile(ClientCommand.Drop, OriginX + 27, OriginY + 27),
            Decide(state, OnPlayerTile, MapKeys.Ctrl, MapButton.Right).Command);
    }

    [Fact]
    public void Shift_HighlightsTheNearestItemWithTheTakeCursor()
    {
        var action = Decide(State(Item(28, 27)), NearTheNextTile, MapKeys.Shift, MapButton.None);

        Assert.Equal(new MapAction(Tile(28, 27), MapLayer.Object, MapCursor.Take, null, null), action);
    }

    [Fact]
    public void Shift_PicksUpAnItemOnLeftAndLooksAtItOnRight()
    {
        var state = State(Item(28, 27));

        Assert.Equal(
            MapCommandPackets.ForTile(ClientCommand.PickUp, OriginX + 28, OriginY + 27),
            Decide(state, NearTheNextTile, MapKeys.Shift, MapButton.Left).Command);
        Assert.Equal(
            MapCommandPackets.ForTile(ClientCommand.LookItem, OriginX + 28, OriginY + 27),
            Decide(state, NearTheNextTile, MapKeys.Shift, MapButton.Right).Command);
    }

    [Fact]
    public void Shift_UsesAUsableItemWithTheUseCursor()
    {
        var state = State(Item(28, 27, MapFlags.IsUsable));

        Assert.Equal(MapCursor.Use, Decide(state, NearTheNextTile, MapKeys.Shift, MapButton.None).Cursor);
        Assert.Equal(
            MapCommandPackets.ForTile(ClientCommand.Use, OriginX + 28, OriginY + 27),
            Decide(state, NearTheNextTile, MapKeys.Shift, MapButton.Left).Command);
    }

    [Fact]
    public void Shift_DropsAHeldItemOnEmptyFloor()
    {
        var state = State(HeldItem(Candle));

        var action = Decide(state, OnPlayerTile, MapKeys.Shift, MapButton.Left);

        Assert.Equal(
            (Tile(27, 27), MapLayer.Floor, MapCursor.Drop),
            (action.HighlightedTile, action.HighlightedLayer, action.Cursor));
        Assert.Equal(MapCommandPackets.ForTile(ClientCommand.Drop, OriginX + 27, OriginY + 27), action.Command);
    }

    [Fact]
    public void Shift_WithAHeldItemOverAnItemShowsTheUseCursorOnlyForUsableItems()
    {
        Assert.Equal(
            MapCursor.None,
            Decide(State(HeldItem(Candle), Item(27, 27)), OnPlayerTile, MapKeys.Shift, MapButton.None).Cursor);
        Assert.Equal(
            MapCursor.Use,
            Decide(
                    State(HeldItem(Candle), Item(27, 27, MapFlags.IsUsable)),
                    OnPlayerTile,
                    MapKeys.Shift,
                    MapButton.None)
                .Cursor);
    }

    [Fact]
    public void Shift_WithNothingNearDoesNothing()
    {
        Assert.Equal(MapAction.Nothing, Decide(State(), OnPlayerTile, MapKeys.Shift, MapButton.Left));
    }

    [Fact]
    public void Ctrl_HighlightsTheNearestCharacterWithTheHitCursor()
    {
        var action = Decide(State(Character(28, 27)), NearTheNextTile, MapKeys.Ctrl, MapButton.None);

        Assert.Equal(new MapAction(Tile(28, 27), MapLayer.Character, MapCursor.Hit, null, null), action);
    }

    [Fact]
    public void Ctrl_AttacksOnLeftAndLooksOnRight()
    {
        var state = State(Character(28, 27));

        Assert.Equal(
            MapCommandPackets.ForCharacter(ClientCommand.Attack, GuardNumber),
            Decide(state, NearTheNextTile, MapKeys.Ctrl, MapButton.Left).Command);
        Assert.Equal(
            MapCommandPackets.ForCharacter(ClientCommand.Look, GuardNumber),
            Decide(state, NearTheNextTile, MapKeys.Ctrl, MapButton.Right).Command);
    }

    [Fact]
    public void Ctrl_GivesTheHeldItemWithTheGiveCursor()
    {
        var state = State(HeldItem(Candle), Character(28, 27));

        var action = Decide(state, NearTheNextTile, MapKeys.Ctrl, MapButton.Left);

        Assert.Equal(MapCursor.Give, action.Cursor);
        Assert.Equal(MapCommandPackets.ForCharacter(ClientCommand.Give, GuardNumber), action.Command);
    }

    [Fact]
    public void Ctrl_WithNobodyNearDoesNothing()
    {
        Assert.Equal(MapAction.Nothing, Decide(State(), OnPlayerTile, MapKeys.Ctrl, MapButton.Left));
    }

    [Fact]
    public void Alt_HighlightsTheNearestCharacterWithTheSelectCursor()
    {
        var action = Decide(State(Character(28, 27)), NearTheNextTile, MapKeys.Alt, MapButton.None);

        Assert.Equal(new MapAction(Tile(28, 27), MapLayer.Character, MapCursor.Select, null, null), action);
    }

    [Fact]
    public void Alt_SelectsOnLeftThenDeselectsAndLooksOnRight()
    {
        var state = State(Character(28, 27));

        Assert.Equal(GuardNumber, Decide(state, NearTheNextTile, MapKeys.Alt, MapButton.Left).NewSelection);
        Assert.Equal(0, Decide(state, NearTheNextTile, MapKeys.Alt, MapButton.Left, GuardNumber).NewSelection);
        Assert.Equal(
            MapCommandPackets.ForCharacter(ClientCommand.Look, GuardNumber),
            Decide(state, NearTheNextTile, MapKeys.Alt, MapButton.Right).Command);
    }

    [Fact]
    public void Alt_ClickingWithNobodyNearClearsTheSelection()
    {
        Assert.Equal(0, Decide(State(), OnPlayerTile, MapKeys.Alt, MapButton.Left, GuardNumber).NewSelection);
    }

    [Theory]
    [InlineData(MapKeys.Shift | MapKeys.Ctrl)]
    [InlineData(MapKeys.Ctrl | MapKeys.Alt)]
    [InlineData(MapKeys.Shift | MapKeys.Ctrl | MapKeys.Alt)]
    public void OtherKeyCombinations_DoNothingYet(MapKeys keys)
    {
        var state = State(Item(27, 27), Character(27, 27));

        Assert.Equal(MapAction.Nothing, Decide(state, OnPlayerTile, keys, MapButton.Left));
    }

    private static int Tile(int column, int row) => column + row * WorldMap.Size;

    private static TestCommand Item(int column, int row, MapFlags extraFlags = MapFlags.None) =>
        Place(column, row, MapFlags.IsItem | extraFlags, Candle, 0, 0);

    private static TestCommand Character(int column, int row) =>
        Place(column, row, MapFlags.IsCharacter, 0, Guard, GuardNumber);

    private static TestCommand Place(int column, int row, MapFlags flags, int item, int character, int number) =>
        TestCommand.Of(AbsoluteSetMap)
            .Byte(FlagsObjectAndCharacter)
            .UInt16((ushort)Tile(column, row))
            .UInt32((uint)flags)
            .UInt16((ushort)item)
            .UInt16((ushort)character)
            .Bytes(0, 0)
            .UInt16((ushort)number)
            .UInt16(1)
            .UInt16(100);

    private static TestCommand HeldItem(int sprite) =>
        TestCommand.Of(ServerCommand.SetCharacterHeldItem).UInt16((ushort)sprite).UInt16(0).Byte(1);

    private static GameState State(params TestCommand[] commands)
    {
        var state = new GameState();
        new TickProcessor(state).Process(
            TestCommand.Payload(
            [
                TestCommand.Of(ServerCommand.SetOrigin).UInt16(OriginX).UInt16(OriginY),
                .. commands,
            ]));
        return state;
    }

    private static MapAction Decide(
        GameState state,
        (float Column, float Row)? point,
        MapKeys keys,
        MapButton button,
        int selectedCharacter = 0) =>
        MapActions.Decide(state, point, keys, button, selectedCharacter);
}
