using TheLastGate.Assets;
using TheLastGate.Net;
using TheLastGate.State;
using TheLastGate.World;

namespace TheLastGate.Tests;

public class UseQueueTests
{
    private const int Using = 4;
    private const int Idle = 0;

    private readonly TextLog chat = new();
    private readonly UseQueue queue;
    private readonly List<byte[]> sent = [];

    public UseQueueTests() => queue = new UseQueue(chat) { Enabled = true, Send = sent.Add };

    [Fact]
    public void TheFirstUseWhileIdle_IsSentAtOnce()
    {
        Assert.Equal(UseQueueAdd.Queued, queue.Add(10, 20, Target(Idle)));

        Assert.Equal([Use(10, 20)], sent);
        Assert.Equal(1, queue.PositionOf(10, 20));
    }

    [Fact]
    public void UsesQueuedWhileBusy_WaitTheirTurn()
    {
        queue.Add(10, 20, Target(Using, 1, 1));
        queue.Add(11, 20, Target(Using, 1, 1));

        Assert.Empty(sent);
        Assert.Equal((1, 2), (queue.PositionOf(10, 20), queue.PositionOf(11, 20)));
    }

    [Fact]
    public void TheUseInProgress_IsNotQueued()
    {
        Assert.Equal(UseQueueAdd.Ignored, queue.Add(10, 20, Target(Using, 10, 20)));

        Assert.Equal(0, queue.Count);
    }

    [Fact]
    public void AUseAlreadyQueued_IsNotQueuedTwice()
    {
        queue.Add(10, 20, Target(Using, 1, 1));

        Assert.Equal(UseQueueAdd.Ignored, queue.Add(10, 20, Target(Using, 1, 1)));
        Assert.Equal(1, queue.Count);
    }

    [Fact]
    public void ASixthUse_SaysTheQueueIsFull()
    {
        for (var x = 0; x < UseQueue.MostQueued; x++)
        {
            queue.Add(x, 0, Target(Using, 1, 1));
        }

        Assert.Equal(UseQueueAdd.Full, queue.Add(9, 0, Target(Using, 1, 1)));

        Assert.Equal(UseQueue.MostQueued, queue.Count);
        Assert.Equal(new LogLine("Your queue is full.", (int)TextColour.Red), Assert.Single(chat.Lines));
    }

    [Fact]
    public void WhenTheServerStartsTheFirstUse_ItLeavesTheQueue()
    {
        queue.Add(10, 20, Target(Idle));
        queue.Add(11, 20, Target(Idle));

        queue.TargetChanged(Target(Idle), Target(Using, 10, 20));

        Assert.Equal((0, 1), (queue.PositionOf(10, 20), queue.PositionOf(11, 20)));
    }

    [Fact]
    public void AnotherUseStarting_LeavesTheQueueAlone()
    {
        queue.Add(10, 20, Target(Using, 1, 1));

        queue.TargetChanged(Target(Idle), Target(Using, 5, 5));

        Assert.Equal(1, queue.PositionOf(10, 20));
    }

    [Fact]
    public void WhenAUseEnds_TheNextIsSent()
    {
        queue.Add(10, 20, Target(Using, 1, 1));
        queue.Add(11, 20, Target(Using, 1, 1));

        queue.TargetChanged(Target(Using, 1, 1), Target(Idle));

        Assert.Equal([Use(10, 20)], sent);
    }

    [Fact]
    public void AnEmptyQueue_SendsNothingWhenAUseEnds()
    {
        queue.TargetChanged(Target(Using, 1, 1), Target(Idle));

        Assert.Empty(sent);
    }

    [Theory]
    [InlineData(ClientCommand.Move)]
    [InlineData(ClientCommand.Turn)]
    [InlineData(ClientCommand.PickUp)]
    [InlineData(ClientCommand.Drop)]
    [InlineData(ClientCommand.Use)]
    [InlineData(ClientCommand.LookItem)]
    [InlineData(ClientCommand.Shop)]
    [InlineData(ClientCommand.Smith)]
    [InlineData(ClientCommand.Mode)]
    [InlineData(ClientCommand.Reset)]
    [InlineData(ClientCommand.Stat)]
    public void AnOrdinaryCommand_ClearsTheQueue(ClientCommand command)
    {
        queue.Add(10, 20, Target(Using, 1, 1));

        queue.Sent(MapCommandPackets.ForTile(command, 1, 1));

        Assert.Equal(0, queue.Count);
    }

    [Theory]
    [InlineData(ClientCommand.Attack)]
    [InlineData(ClientCommand.Give)]
    [InlineData(ClientCommand.Inventory)]
    [InlineData(ClientCommand.Skill)]
    public void CharacterAndInventoryCommands_KeepTheQueue(ClientCommand command)
    {
        queue.Add(10, 20, Target(Using, 1, 1));

        queue.Sent(MapCommandPackets.ForTile(command, 1, 1));

        Assert.Equal(1, queue.Count);
    }

    [Fact]
    public void TurningTheOptionOff_ForgetsTheQueue()
    {
        queue.Add(10, 20, Target(Using, 1, 1));

        queue.Enabled = false;

        Assert.Equal(0, queue.Count);
    }

    [Fact]
    public void TheServersSetTarget_ReachesTheQueue()
    {
        var state = new GameState();
        state.UseQueue.Enabled = true;
        state.UseQueue.Send = sent.Add;
        state.UseQueue.Add(10, 20, Target(Using, 1, 1));
        state.UseQueue.Add(11, 20, Target(Using, 1, 1));
        var processor = new TickProcessor(state);

        processor.Process(TestCommand.Payload(SetTarget(Using, 1, 1)));
        processor.Process(TestCommand.Payload(SetTarget(Idle, 0, 0)));

        Assert.Equal([Use(10, 20)], sent);
    }

    private static PlayerTarget Target(int action, int x = 0, int y = 0) => new(0, 0, 0, action, x, y);

    private static byte[] Use(int x, int y) => MapCommandPackets.ForTile(ClientCommand.Use, x, y);

    private static TestCommand SetTarget(int action, int x, int y) =>
        TestCommand.Of(ServerCommand.SetTarget)
            .UInt16(0)
            .UInt16(0)
            .UInt16(0)
            .UInt16((ushort)action)
            .UInt16((ushort)x)
            .UInt16((ushort)y);
}
