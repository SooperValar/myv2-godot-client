using TheLastGate.Net;
using TheLastGate.State;

namespace TheLastGate.Tests;

public class RequestCommandsTests
{
    [Fact]
    public void PlaySound_RequestsTheSoundWithItsVolumeAndPan()
    {
        var state = new GameState();
        var sounds = new List<SoundRequest>();
        state.SoundRequested += sounds.Add;

        Process(state, TestCommand.Of(ServerCommand.PlaySound).UInt32(17).UInt32(unchecked((uint)-500)).UInt32(30));

        Assert.Equal([new SoundRequest(17, -500, 30)], sounds);
    }

    [Theory]
    [InlineData(9, "Server is being shutdown.")]
    [InlineData(14, "You have been banned for an hour. Enhance your social behavior before you come back.")]
    [InlineData(99, "unknown reason")]
    public void Exit_ReportsTheReason(uint code, string reason)
    {
        var state = new GameState();
        var exits = new List<ServerExit>();
        state.ExitRequested += exits.Add;

        Process(state, TestCommand.Of(ServerCommand.Exit).UInt32(code));

        Assert.Equal([new ServerExit((int)code, reason)], exits);
    }

    [Fact]
    public void Load_SetsTheServerLoad()
    {
        var state = new GameState();

        Process(state, TestCommand.Of(ServerCommand.Load).UInt32(73));

        Assert.Equal(73, state.ServerLoad);
    }

    [Fact]
    public void Unique_ReportsTheNewId()
    {
        var state = new GameState();
        var ids = new List<UniqueId>();
        state.UniqueIdReceived += ids.Add;

        Process(state, TestCommand.Of(ServerCommand.Unique).UInt32(0x11223344).UInt32(0x55667788));

        Assert.Equal([new UniqueId(0x11223344, 0x55667788)], ids);
    }

    [Fact]
    public void Ignore_SkipsItsWholeLength()
    {
        var state = new GameState();

        Process(
            state,
            TestCommand.Of(ServerCommand.Ignore).UInt32(12).Bytes(1, 2, 3, 4, 5, 6, 7),
            TestCommand.Of(ServerCommand.SetCharacterMode).Byte(2));

        Assert.Equal(2, state.Player.Mode);
    }

    [Fact]
    public void Waypoints_RequestsTheWaypointWindow()
    {
        var state = new GameState();
        var requests = 0;
        state.WaypointsRequested += () => requests++;

        Process(state, TestCommand.Of(ServerCommand.Waypoints));

        Assert.Equal(1, requests);
    }

    [Theory]
    [InlineData(0, WindowKind.MessageOfTheDay, 0)]
    [InlineData(99, WindowKind.NewPlayerWelcome, 0)]
    [InlineData(7, WindowKind.Book, 7)]
    [InlineData(103, WindowKind.Tutorial, 3)]
    public void ShowMessageOfTheDay_RequestsTheWindowItNames(byte window, WindowKind kind, int number)
    {
        var state = new GameState();
        var requests = new List<WindowRequest>();
        state.WindowRequested += requests.Add;

        Process(state, TestCommand.Of(ServerCommand.ShowMessageOfTheDay).Byte(window));

        Assert.Equal([new WindowRequest(kind, number)], requests);
    }

    [Fact]
    public void ClearBox_ClearsABoxOfTheMinimap()
    {
        var state = new GameState();
        var boxes = new List<MinimapBox>();
        state.MinimapBoxCleared += boxes.Add;

        Process(state, TestCommand.Of(ServerCommand.ClearBox).UInt16(100).UInt16(200).UInt16(30).UInt16(40));

        Assert.Equal([new MinimapBox(100, 200, 30, 40)], boxes);
    }

    private static void Process(GameState state, params TestCommand[] commands) =>
        new TickProcessor(state).Process(TestCommand.Payload(commands));
}
