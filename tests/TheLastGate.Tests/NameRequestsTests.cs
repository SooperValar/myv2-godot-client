using TheLastGate.Net;
using TheLastGate.State;
using TheLastGate.World;

namespace TheLastGate.Tests;

public class NameRequestsTests
{
    private const int Guard = 552;
    private const int Bishop = 553;
    private const int Id = 41;

    [Fact]
    public void Tick_AsksForAnUnknownNameOnlyAfterTheWait()
    {
        var requests = new NameRequests();
        var names = new GameState().Names;

        for (var tick = 0; tick < NameRequests.TicksBetweenRequests; tick++)
        {
            requests.NoteUnknown(Guard, Id);
            Assert.Null(requests.Tick(names));
        }

        requests.NoteUnknown(Guard, Id);
        Assert.Equal(MapCommandPackets.ForCharacter(ClientCommand.AutoLook, Guard), requests.Tick(names));
    }

    [Fact]
    public void Tick_WaitsAgainAfterEachRequest()
    {
        var requests = new NameRequests();
        var names = new GameState().Names;
        PassTheWait(requests, names);
        requests.NoteUnknown(Guard, Id);
        requests.Tick(names);

        requests.NoteUnknown(Bishop, Id);

        Assert.Null(requests.Tick(names));
    }

    [Fact]
    public void Tick_AsksAtOnceWhenTheWaitPassedWithNothingUnknown()
    {
        var requests = new NameRequests();
        var names = new GameState().Names;
        PassTheWait(requests, names);

        requests.NoteUnknown(Guard, Id);

        Assert.Equal(MapCommandPackets.ForCharacter(ClientCommand.AutoLook, Guard), requests.Tick(names));
    }

    [Fact]
    public void Tick_AsksForTheLastUnknownCharacterNoted()
    {
        var requests = new NameRequests();
        var names = new GameState().Names;
        PassTheWait(requests, names);

        requests.NoteUnknown(Guard, Id);
        requests.NoteUnknown(Bishop, Id);

        Assert.Equal(MapCommandPackets.ForCharacter(ClientCommand.AutoLook, Bishop), requests.Tick(names));
    }

    [Fact]
    public void Tick_SkipsANameThatArrivedMeanwhileButStillWaitsAgain()
    {
        var requests = new NameRequests();
        var state = new GameState();
        PassTheWait(requests, state.Names);
        requests.NoteUnknown(Guard, Id);
        LearnName(state, Guard, Id, "Guard");

        Assert.Null(requests.Tick(state.Names));
        requests.NoteUnknown(Bishop, Id);
        Assert.Null(requests.Tick(state.Names));
    }

    private static void PassTheWait(NameRequests requests, CharacterNames names)
    {
        for (var tick = 0; tick < NameRequests.TicksBetweenRequests; tick++)
        {
            requests.Tick(names);
        }
    }

    private static void LearnName(GameState state, int number, int id, string name) =>
        new TickProcessor(state).Process(
            TestCommand.Payload(
                TestCommand.Of(ServerCommand.Look1)
                    .UInt16(0)
                    .UInt16(0)
                    .UInt16(0)
                    .UInt16(0)
                    .UInt16(0)
                    .UInt16(0)
                    .UInt16(0)
                    .Byte(1),
                TestCommand.Of(ServerCommand.Look2),
                TestCommand.Of(ServerCommand.Look3)
                    .UInt16(0)
                    .UInt16(0)
                    .UInt16(0)
                    .UInt16((ushort)number)
                    .UInt16((ushort)id),
                TestCommand.Of(ServerCommand.Look4),
                TestCommand.Of(ServerCommand.Look5).Text(name, 15)));
}
