using System.Buffers.Binary;
using System.Diagnostics;
using TheLastGate.Net;

namespace TheLastGate.Tests;

public class TickPlaybackTests
{
    [Fact]
    public void PlayDueTicks_PlaysTheFirstTickAtOnce()
    {
        var playback = new TickPlayback(new TickPacer());
        playback.Receive(Tick(1, 1000));

        var played = PlayDue(playback, 1000);

        Assert.Equal([[1]], played);
        Assert.Equal(0, playback.QueuedTicks);
    }

    [Fact]
    public void PlayDueTicks_SpacesTicksByThePacersPace()
    {
        var playback = new TickPlayback(new TickPacer());
        ReceiveTicks(playback, 4, 1000);

        Assert.Equal([[0]], PlayDue(playback, 1000));
        Assert.Empty(PlayDue(playback, 1052));
        Assert.Equal([[1]], PlayDue(playback, 1053));
    }

    [Fact]
    public void PlayDueTicks_CatchesUpAfterAStall()
    {
        var playback = new TickPlayback(new TickPacer());
        ReceiveTicks(playback, 4, 1000);
        PlayDue(playback, 1000);

        var played = PlayDue(playback, 2000);

        Assert.Equal([[1], [2], [3]], played);
    }

    [Fact]
    public void PlayDueTicks_KeepsTheScheduleMovingWhileNothingIsQueued()
    {
        var playback = new TickPlayback(new TickPacer());
        playback.Receive(Tick(0, 1000));
        PlayDue(playback, 1000);
        PlayDue(playback, 1500);

        ReceiveTicks(playback, 2, 1500);

        Assert.Empty(PlayDue(playback, 1519));
        Assert.Single(PlayDue(playback, 1520));
    }

    [Fact]
    public void PlayDueTicks_ReportsEverySixteenthTickPlayed()
    {
        var playback = new TickPlayback(new TickPacer());
        ReceiveTicks(playback, 33, 1000);
        var sent = new List<byte[]>();

        playback.PlayDueTicks(1000, _ => { }, sent.Add);
        playback.PlayDueTicks(100_000, _ => { }, sent.Add);

        Assert.Equal(2, sent.Count);
        Assert.All(sent, command => Assert.Equal((byte)ClientCommand.TickReport, command[0]));
        Assert.Equal(16u, BinaryPrimitives.ReadUInt32LittleEndian(sent[0].AsSpan(1)));
        Assert.Equal(32u, BinaryPrimitives.ReadUInt32LittleEndian(sent[1].AsSpan(1)));
    }

    [Fact]
    public void Receive_GivesThePacerEachTicksArrivalTime()
    {
        var pacer = new TickPacer { Adaptive = true };
        var playback = new TickPlayback(pacer);

        for (var tickIndex = 0; tickIndex < 1000; tickIndex++)
        {
            playback.Receive(Tick(0, 1000 + tickIndex * 21));
        }

        Assert.InRange(pacer.MeasuredPeriodMilliseconds, 20.5, 21.5);
    }

    private static ReceivedTick Tick(byte marker, long arrivedAtMilliseconds) =>
        new([marker], arrivedAtMilliseconds * Stopwatch.Frequency / 1000);

    private static void ReceiveTicks(TickPlayback playback, int count, long arrivedAtMilliseconds)
    {
        for (var tickIndex = 0; tickIndex < count; tickIndex++)
        {
            playback.Receive(Tick((byte)tickIndex, arrivedAtMilliseconds));
        }
    }

    private static List<byte[]> PlayDue(TickPlayback playback, long nowMilliseconds)
    {
        var played = new List<byte[]>();
        playback.PlayDueTicks(nowMilliseconds, played.Add, _ => { });
        return played;
    }
}
