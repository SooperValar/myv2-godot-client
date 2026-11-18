using TheLastGate.Buffs;
using TheLastGate.Net;
using TheLastGate.State;

namespace TheLastGate.Tests;

public class BuffWatchWiringTests
{
    private const int ChunkLength = 15;

    private readonly TickProcessor processor;
    private readonly List<string> said = [];
    private readonly GameState state = new();

    public BuffWatchWiringTests()
    {
        state.BuffWatch.Say = said.Add;
        processor = new TickProcessor(state);
    }

    [Fact]
    public void ABlessAppearing_AsksForTheBuffsInTheBackground()
    {
        Process(BlessSlot());

        Assert.Equal([BuffWatch.Command], said);
    }

    [Fact]
    public void TheReply_IsKeptOutOfTheChatAndRead()
    {
        Process(BlessSlot());

        Process(Lines(BuffList.Header, " ", "Bless power of 60 for 4m 10s", " "));

        Assert.Empty(state.Chat.Lines);
        Assert.Equal(60, state.BuffWatch.Reading(BuffKind.Bless)?.Power);
    }

    [Fact]
    public void TheLoginTick_ForgetsTheReadings()
    {
        Process(BlessSlot());
        Process(Lines(BuffList.Header, " ", "Bless power of 60 for 4m 10s", " "));

        Process(TestCommand.Of(ServerCommand.Tick).Byte(0));

        Assert.Null(state.BuffWatch.Reading(BuffKind.Bless));
    }

    private static TestCommand BlessSlot() =>
        TestCommand.Of(ServerCommand.SetCharacterSpell).UInt32(0).UInt16(BuffList.BlessSprite).UInt16(16);

    // Each line goes to the client in pieces of fifteen letters, the last ending the line.
    private static TestCommand[] Lines(params string[] lines) =>
        lines.SelectMany(line => (line + "\n").Chunk(ChunkLength)
                .Select(piece => TestCommand.Of(ServerCommand.Log1).Text(new string(piece), ChunkLength)))
            .ToArray();

    private void Process(params TestCommand[] commands) => processor.Process(TestCommand.Payload(commands));
}
