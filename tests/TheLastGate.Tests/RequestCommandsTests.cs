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

    private static void Process(GameState state, params TestCommand[] commands) =>
        new TickProcessor(state).Process(TestCommand.Payload(commands));
}
