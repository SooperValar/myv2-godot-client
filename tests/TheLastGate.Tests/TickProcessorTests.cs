using TheLastGate.Net;
using TheLastGate.State;

namespace TheLastGate.Tests;

public class TickProcessorTests
{
    private const int FirstSetMapId = 0x80;
    private const byte EmptyIgnoreLength = 5;
    private const int LongestCommand = 126;

    public static TheoryData<ServerCommand> TickCommands()
    {
        var commands = new TheoryData<ServerCommand>();
        for (var id = 0; id < FirstSetMapId; id++)
        {
            if (TryBuildCommand((ServerCommand)id, out _))
            {
                commands.Add((ServerCommand)id);
            }
        }

        return commands;
    }

    [Theory]
    [MemberData(nameof(TickCommands))]
    public void Process_HandlesEveryCommandATickCanContain(ServerCommand commandId)
    {
        TryBuildCommand(commandId, out var command);

        var exception = Record.Exception(() => new TickProcessor(new GameState()).Process(command));

        Assert.Null(exception);
    }

    [Fact]
    public void Process_SaysTheTickHasEndedOnceItsCommandsAreApplied()
    {
        var state = new GameState();
        var processor = new TickProcessor(state);
        var tickAtTheEnd = new List<int>();
        state.TickEnded += () => tickAtTheEnd.Add(state.ServerTick);

        processor.Process(TestCommand.Payload(TestCommand.Of(ServerCommand.Tick).Byte(42)));

        Assert.Equal([42], tickAtTheEnd);
    }

    [Fact]
    public void Process_SaysWhenTheServerChangesTheTarget()
    {
        var state = new GameState();
        var changes = new List<(PlayerTarget Previous, PlayerTarget Now)>();
        state.TargetChanged += (previous, now) => changes.Add((previous, now));
        var setTarget = TestCommand.Of(ServerCommand.SetTarget)
            .UInt16(0)
            .UInt16(0)
            .UInt16(0)
            .UInt16(4)
            .UInt16(10)
            .UInt16(20);

        new TickProcessor(state).Process(TestCommand.Payload(setTarget));

        Assert.Equal([(default, new PlayerTarget(0, 0, 0, 4, 10, 20))], changes);
    }

    [Fact]
    public void Process_SaysWhenTheServerChangesTheHeldItem()
    {
        var state = new GameState();
        var heldSprites = new List<int>();
        state.HeldItemChanged += () => heldSprites.Add(state.Player.HeldItem.Sprite);
        var setHeldItem = TestCommand.Of(ServerCommand.SetCharacterHeldItem).UInt16(2500).UInt16(0);

        new TickProcessor(state).Process(TestCommand.Payload(setHeldItem));

        Assert.Equal([2500], heldSprites);
    }

    private static bool TryBuildCommand(ServerCommand commandId, out byte[] command)
    {
        var bytes = new byte[LongestCommand];
        bytes[0] = (byte)commandId;
        if (commandId == ServerCommand.Ignore)
        {
            bytes[1] = EmptyIgnoreLength;
        }

        try
        {
            command = bytes[..ServerCommandLength.Of(bytes)];
            return true;
        }
        catch (ProtocolDesyncException)
        {
            command = [];
            return false;
        }
    }
}
