using TheLastGate.Net;
using TheLastGate.State;

namespace TheLastGate.Tests;

public class ServerInfoCommandsTests
{
    [Fact]
    public void Zone_SetsThePlayersLocation()
    {
        var state = new GameState();

        new TickProcessor(state).Process(
            TestCommand.Payload(TestCommand.Of(ServerCommand.Zone).Text("Aston, Market", 40)));

        Assert.Equal("Aston, Market", state.Player.Location);
    }

    [Fact]
    public void Zone_ClearsTheLocationOutsideEveryZone()
    {
        var state = new GameState();
        var processor = new TickProcessor(state);

        processor.Process(TestCommand.Payload(TestCommand.Of(ServerCommand.Zone).Text("Aston", 40)));
        processor.Process(TestCommand.Payload(TestCommand.Of(ServerCommand.Zone).Text("", 40)));

        Assert.Equal("", state.Player.Location);
    }
}
