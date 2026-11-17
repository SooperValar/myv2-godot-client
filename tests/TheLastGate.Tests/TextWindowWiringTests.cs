using TheLastGate.Net;
using TheLastGate.State;

namespace TheLastGate.Tests;

public class TextWindowWiringTests
{
    private const int ChunkLength = 15;
    private readonly TickProcessor processor;

    private readonly GameState state = new();

    public TextWindowWiringTests() => processor = new TickProcessor(state);

    [Theory]
    [InlineData(0, true, false, null)]
    [InlineData(99, false, true, null)]
    [InlineData(104, false, false, 4)]
    public void TheShowCommand_OpensTheWindowItNames(byte window, bool messageOfTheDay, bool welcome, int? tutorial)
    {
        Process(TestCommand.Of(ServerCommand.ShowMessageOfTheDay).Byte(window));

        Assert.Equal(messageOfTheDay, state.TextWindows.MessageOfTheDayOpen);
        Assert.Equal(welcome, state.TextWindows.WelcomeOpen);
        Assert.Equal(tutorial, state.TextWindows.Tutorial);
    }

    [Fact]
    public void MessageOfTheDayText_FillsTheWindowsLines()
    {
        Process(
            TestCommand.Of(ServerCommand.MessageOfTheDay2).Text("Welcome back!\n", ChunkLength),
            TestCommand.Of(ServerCommand.MessageOfTheDay0).Text("Patch notes\n", ChunkLength));

        Assert.Equal(new LogLine("Welcome back!", 2), state.MessageOfTheDayLines.Lines[0]);
        Assert.Equal(new LogLine("Patch notes", 0), state.MessageOfTheDayLines.Lines[1]);
    }

    [Fact]
    public void EachTick_CountsDownThePauseAfterClosingAWindow()
    {
        Process(TestCommand.Of(ServerCommand.ShowMessageOfTheDay).Byte(0));
        state.TextWindows.Close();

        Process(TestCommand.Of(ServerCommand.ShowMessageOfTheDay).Byte(0));

        Assert.False(state.TextWindows.MessageOfTheDayOpen);
    }

    private void Process(params TestCommand[] commands) => processor.Process(TestCommand.Payload(commands));
}
