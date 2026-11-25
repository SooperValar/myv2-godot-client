using TheLastGate.Net;
using TheLastGate.State;

namespace TheLastGate.Tests;

public class ItemInfoCacheTests
{
    private const int Sword = 1234;
    private readonly List<byte[]> sent = [];
    private readonly GameState state = new();

    public ItemInfoCacheTests() => state.ItemInfo.Send = sent.Add;

    [Fact]
    public void For_AsksTheServerOnceAndShowsNothingUntilItAnswers()
    {
        Assert.Null(state.ItemInfo.For(ItemInfoCache.Backpack, 5, Sword, 0, 0));
        Assert.Null(state.ItemInfo.For(ItemInfoCache.Backpack, 5, Sword, 0, 100));

        var request = Assert.Single(sent);
        Assert.Equal([(byte)ClientCommand.ItemInfo, ItemInfoCache.Backpack, 5, 0, 0, 0, 0, 0], request[..8]);
    }

    [Fact]
    public void For_ShowsTheServersLinesOnceTheyAreComplete()
    {
        state.ItemInfo.For(ItemInfoCache.Worn, 3, Sword, 0, 0);

        Answer(ItemInfoCache.Worn, 3, ("Steel Sword", 12), ("A sharp blade.", 1));

        Assert.Equal(
            [new LogLine("Steel Sword", 12), new LogLine("A sharp blade.", 1)],
            state.ItemInfo.For(ItemInfoCache.Worn, 3, Sword, 0, 100));
    }

    [Fact]
    public void For_AsksAgainAfterFiveSecondsButKeepsShowingTheOldLines()
    {
        state.ItemInfo.For(ItemInfoCache.Backpack, 0, Sword, 0, 0);
        Answer(ItemInfoCache.Backpack, 0, ("Steel Sword", 12));

        var lines = state.ItemInfo.For(ItemInfoCache.Backpack, 0, Sword, 0, 6000);

        Assert.Equal(2, sent.Count);
        Assert.Equal([new LogLine("Steel Sword", 12)], lines);
    }

    [Fact]
    public void For_AsksAfreshWhenTheSlotHoldsAnotherItem()
    {
        state.ItemInfo.For(ItemInfoCache.Backpack, 0, Sword, 0, 0);
        Answer(ItemInfoCache.Backpack, 0, ("Steel Sword", 12));

        Assert.Null(state.ItemInfo.For(ItemInfoCache.Backpack, 0, Sword + 1, 0, 10));
        Assert.Equal(2, sent.Count);
    }

    private void Answer(int where, int slot, params (string Text, int Font)[] lines)
    {
        var commands = lines.Select((line, index) => TestCommand.Of(ServerCommand.ItemInfo)
                .Bytes((byte)where, (byte)slot, (byte)index, (byte)line.Font)
                .Text(line.Text, 59))
            .Append(TestCommand.Of(ServerCommand.ItemInfo).Bytes((byte)where, (byte)slot, 255, (byte)lines.Length).Text("", 59))
            .ToArray();
        new TickProcessor(state).Process(TestCommand.Payload(commands));
    }
}
