using TheLastGate.Net;
using TheLastGate.State;

namespace TheLastGate.Tests;

public class TickProcessorTests
{
    private const int FirstSetMapId = 0x80;
    private const byte EmptyIgnoreLength = 5;
    private const int LongestCommand = 64;

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
