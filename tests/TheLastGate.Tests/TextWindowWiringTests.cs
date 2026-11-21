using TheLastGate.Net;
using TheLastGate.State;

namespace TheLastGate.Tests;

public class TextWindowWiringTests
{
    private const int ChunkLength = 15;
    private readonly TickProcessor processor;

    private readonly GameState state = new();

    public TextWindowWiringTests() => processor = new TickProcessor(state);

    [Fact]
    public void MessageOfTheDayText_FillsTheWindowsLines()
    {
        Process(
            TestCommand.Of(ServerCommand.MessageOfTheDay2).Text("Welcome back!\n", ChunkLength),
            TestCommand.Of(ServerCommand.MessageOfTheDay0).Text("Patch notes\n", ChunkLength));

        Assert.Equal(new LogLine("Welcome back!", 2), state.MessageOfTheDayLines.Lines[0]);
        Assert.Equal(new LogLine("Patch notes", 0), state.MessageOfTheDayLines.Lines[1]);
    }

    private void Process(params TestCommand[] commands) => processor.Process(TestCommand.Payload(commands));
}
