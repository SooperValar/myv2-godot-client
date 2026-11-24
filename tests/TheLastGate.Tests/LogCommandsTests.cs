using TheLastGate.Net;
using TheLastGate.State;

namespace TheLastGate.Tests;

public class LogCommandsTests
{
    private const int ChunkLength = 15;

    [Fact]
    public void Log_AddsALineWhenItsNewlineArrives()
    {
        var state = Process(Chunk(ServerCommand.Log3, "Hello\n"));

        Assert.Equal([new LogLine("Hello", 3)], state.Chat.Lines);
    }

    [Fact]
    public void Log_JoinsALineSentInSeveralChunks()
    {
        var state = Process(
            Chunk(ServerCommand.Log1, "The quick brown"),
            Chunk(ServerCommand.Log1, " fox jumps over"),
            Chunk(ServerCommand.Log1, " the dog\n"));

        Assert.Equal([new LogLine("The quick brown fox jumps over the dog", 1)], state.Chat.Lines);
    }

    [Fact]
    public void Log_TakesTheFontOfTheChunkThatEndsTheLine()
    {
        var state = Process(Chunk(ServerCommand.Log0, "abcdefghijklmno"), Chunk(ServerCommand.Log3, "pq\n"));

        Assert.Equal([new LogLine("abcdefghijklmnopq", 3)], state.Chat.Lines);
    }

    [Fact]
    public void Log_DropsWhatFollowsTheNewlineInItsChunk()
    {
        var state = Process(Chunk(ServerCommand.Log2, "Hi\nthere"), Chunk(ServerCommand.Log2, "Bye\n"));

        Assert.Equal([new LogLine("Hi", 2), new LogLine("Bye", 2)], state.Chat.Lines);
    }

    [Fact]
    public void Log_IgnoresAnEmptyLine()
    {
        var state = Process(Chunk(ServerCommand.Log2, "\n"));

        Assert.Empty(state.Chat.Lines);
    }

    [Theory]
    [InlineData(33, true)]
    [InlineData(34, false)]
    public void Log_DropsTextThatRunsPastFiveHundredBytesWithoutANewline(int chunksBefore, bool keptBefore)
    {
        var chunks = Enumerable.Repeat(Chunk(ServerCommand.Log0, new string('x', ChunkLength)), chunksBefore)
            .Append(Chunk(ServerCommand.Log0, "ok\n"))
            .ToArray();

        var state = Process(chunks);

        var expected = (keptBefore ? new string('x', chunksBefore * ChunkLength) : "") + "ok";
        Assert.Equal([new LogLine(expected, 0)], state.Chat.Lines);
    }

    [Fact]
    public void Log_KeepsTheLatestLinesUpToTheLimit()
    {
        var chunks = Enumerable.Range(0, TextLog.MaxLines + 5)
            .Select(lineIndex => Chunk(ServerCommand.Log0, $"line {lineIndex}\n"))
            .ToArray();

        var state = Process(chunks);

        Assert.Equal(TextLog.MaxLines, state.Chat.Lines.Count);
        Assert.Equal("line 5", state.Chat.Lines[0].Text);
        Assert.Equal($"line {TextLog.MaxLines + 4}", state.Chat.Lines[^1].Text);
    }

    [Fact]
    public void Log_ReportsEachLineAdded()
    {
        var state = new GameState();
        var added = new List<LogLine>();
        state.Chat.LineAdded += added.Add;

        new TickProcessor(state).Process(TestCommand.Payload(Chunk(ServerCommand.Log3, "Ding\n")));

        Assert.Equal([new LogLine("Ding", 3)], added);
    }

    [Fact]
    public void ServerTick_CountsUpEachTickAndWrapsAtTwenty()
    {
        var state = new GameState();
        var processor = new TickProcessor(state);

        processor.Process(TestCommand.Payload(TestCommand.Of(ServerCommand.Tick).Byte(18)));
        Assert.Equal(18, state.ServerTick);

        processor.Process([]);
        Assert.Equal(19, state.ServerTick);

        processor.Process([]);
        Assert.Equal(0, state.ServerTick);
    }

    private static TestCommand Chunk(ServerCommand commandId, string text) =>
        TestCommand.Of(commandId).Text(text, ChunkLength);

    private static GameState Process(params TestCommand[] commands)
    {
        var state = new GameState();
        new TickProcessor(state).Process(TestCommand.Payload(commands));
        return state;
    }
}
