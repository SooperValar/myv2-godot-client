using TheLastGate.Net;
using TheLastGate.State;
using TheLastGate.World;

namespace TheLastGate.Tests;

public class AreaUseTests
{
    private static PlayerTarget Busy => new(0, 0, 0, 4, 50, 50);

    [Theory]
    [InlineData(10, 10, 10, 10)]
    [InlineData(14, 14, 14, 14)]
    [InlineData(15, 15, 14, 14)]
    [InlineData(19, 12, 17, 12)]
    [InlineData(5, 5, 6, 6)]
    [InlineData(10, 40, 10, 34)]
    [InlineData(12, 20, 12, 17)]
    public void FitEnd_KeepsTheAreaToTwentyFiveTiles(int endX, int endY, int fittedX, int fittedY)
    {
        Assert.Equal((fittedX, fittedY), AreaUse.FitEnd((10, 10), (endX, endY)));
    }

    [Fact]
    public void QueueArea_ReplacesTheQueuePastTheClickLimit()
    {
        var sent = new List<byte[]>();
        var queue = new UseQueue(new TextLog()) { Enabled = true, Send = sent.Add };
        queue.Add(1, 1, Busy);
        var area = Enumerable.Range(0, 8).Select(x => new AreaTarget(0, x, 5, ClientCommand.PickUp));

        Assert.Equal(8, queue.QueueArea(area, Busy));

        Assert.Equal((0, 1, 8), (queue.PositionOf(1, 1), queue.PositionOf(0, 5), queue.Count));
        Assert.Equal(UseQueueAdd.Full, queue.Add(9, 9, Busy));
        queue.TargetChanged(Busy, new PlayerTarget());
        Assert.Equal([MapCommandPackets.ForTile(ClientCommand.PickUp, 0, 5)], sent);
    }
}
