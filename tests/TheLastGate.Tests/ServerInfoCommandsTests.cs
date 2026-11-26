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

    [Fact]
    public void Meta_SetsTheStatsValueAndShow()
    {
        var state = new GameState();

        new TickProcessor(state).Process(
            TestCommand.Payload(TestCommand.Of(ServerCommand.Meta).Byte(50).UInt32(unchecked((uint)-150)).Byte(1)));

        var stat = state.Terminology.MetaStats[50];
        Assert.Equal(-150, stat.Value);
        Assert.Equal(1, stat.Show);
        Assert.Equal("-1.50", MetaStat.ValueText(stat.Value, stat.Flag != 0));
    }

    [Fact]
    public void Meta_PastTheLastStat_IsSkipped()
    {
        var state = new GameState();

        new TickProcessor(state).Process(
            TestCommand.Payload(TestCommand.Of(ServerCommand.Meta).Byte(200).UInt32(5).Byte(1)));

        Assert.All(state.Terminology.MetaStats, stat => Assert.Equal(0, stat.Value));
    }
}
