using TheLastGate.Net;

namespace TheLastGate.Tests;

public class TickPacerTests
{
    [Fact]
    public void Adaptive_IsOffByDefault()
    {
        Assert.False(new TickPacer().Adaptive);
    }

    [Theory]
    [InlineData(8, 20)]
    [InlineData(4, 40)]
    [InlineData(3, 53)]
    [InlineData(1, 160)]
    [InlineData(16, 10)]
    public void NextTickMilliseconds_WhenFixed_SpreadsEightNominalTicksOverTheQueue(int queuedTicks, int expected)
    {
        Assert.Equal(expected, new TickPacer().NextTickMilliseconds(queuedTicks));
    }

    [Fact]
    public void NextTickMilliseconds_WhenFixed_ChecksAgainAfterANominalTickWhenNothingIsQueued()
    {
        Assert.Equal(TickPacer.NominalTickMilliseconds, new TickPacer().NextTickMilliseconds(0));
    }

    [Fact]
    public void Adaptive_StartsWithATargetOfFourTicks()
    {
        Assert.Equal(4, AdaptivePacer().BufferTarget);
    }

    [Fact]
    public void Adaptive_SettlesAtTheMinimumTargetOnASteadyConnection()
    {
        var pacer = AdaptivePacer();

        FeedSteady(pacer, 1000, 1000, 21);

        Assert.Equal(1, pacer.BufferTarget);
        Assert.InRange(pacer.MeasuredPeriodMilliseconds, 20.5, 21.5);
    }

    [Fact]
    public void Adaptive_RaisesTheTargetOnABurstyConnection()
    {
        var pacer = AdaptivePacer();
        var now = FeedSteady(pacer, 1000, 1000, 21);

        FeedBursts(pacer, now, 200, 3, 63);

        Assert.Equal(2, pacer.BufferTarget);
    }

    [Fact]
    public void Adaptive_LowersTheTargetSlowlyAfterCalmReturns()
    {
        var pacer = AdaptivePacer();
        var now = FeedBursts(pacer, 1000, 200, 5, 105);
        Assert.Equal(4, pacer.BufferTarget);

        now = FeedSteady(pacer, now, 300, 21);
        Assert.InRange(pacer.BufferTarget, 2, 3);

        FeedSteady(pacer, now, 1000, 21);
        Assert.Equal(1, pacer.BufferTarget);
    }

    [Fact]
    public void Adaptive_PlaysAtTheMeasuredPeriodWhenTheQueueIsOnTarget()
    {
        var pacer = AdaptivePacer();
        FeedSteady(pacer, 1000, 1000, 21);

        var totalMilliseconds = Enumerable.Range(0, 100).Sum(_ => pacer.NextTickMilliseconds(1));

        Assert.InRange(totalMilliseconds, 2095, 2105);
    }

    [Fact]
    public void Adaptive_NudgesTheRateTowardTheTarget()
    {
        var pacer = AdaptivePacer();
        FeedSteady(pacer, 1000, 1000, 20);

        Assert.True(pacer.NextTickMilliseconds(3) < 20);
        Assert.True(pacer.NextTickMilliseconds(0) > 20);
    }

    [Fact]
    public void Adaptive_ClampsTheRate()
    {
        var pacer = AdaptivePacer();
        FeedSteady(pacer, 1000, 1000, 20);

        Assert.Equal(10, pacer.NextTickMilliseconds(100));
        Assert.True(pacer.NextTickMilliseconds(0) <= 25);
    }

    [Fact]
    public void Adaptive_BarelyMovesThePeriodAfterALongStall()
    {
        var pacer = AdaptivePacer();
        var now = FeedSteady(pacer, 1000, 1000, 20);

        FeedSteady(pacer, now + 2000, 32, 20);

        Assert.True(pacer.MeasuredPeriodMilliseconds < 21.0);
    }

    [Fact]
    public void Adaptive_UsesTheNominalTickBeforeAnyMeasurement()
    {
        Assert.Equal(TickPacer.NominalTickMilliseconds, AdaptivePacer().NextTickMilliseconds(4));
    }

    [Fact]
    public void Adaptive_StartsMeasuringAfreshWhenTurnedOn()
    {
        var pacer = AdaptivePacer();
        FeedSteady(pacer, 1000, 1000, 21);

        pacer.Adaptive = false;
        pacer.Adaptive = true;

        Assert.Equal(4, pacer.BufferTarget);
        Assert.Equal(0.0, pacer.MeasuredPeriodMilliseconds);
    }

    [Fact]
    public void IsTickDue_IsTrueAtTheFirstCheck()
    {
        Assert.True(new TickPacer().IsTickDue(5000));
    }

    [Fact]
    public void TickDone_SchedulesTheNextTickAtTheCurrentPace()
    {
        var pacer = new TickPacer();
        pacer.IsTickDue(1000);

        pacer.TickDone(4);

        Assert.False(pacer.IsTickDue(1039));
        Assert.True(pacer.IsTickDue(1040));
    }

    [Fact]
    public void TickDone_CountsTheTicksPlayed()
    {
        var pacer = new TickPacer();
        pacer.IsTickDue(1000);

        pacer.TickDone(2);
        pacer.TickDone(1);

        Assert.Equal(2u, pacer.TicksPlayed);
    }

    private static TickPacer AdaptivePacer() => new() { Adaptive = true };

    private static long FeedSteady(TickPacer pacer, long now, int arrivals, int gapMilliseconds)
    {
        for (var arrivalIndex = 0; arrivalIndex < arrivals; arrivalIndex++)
        {
            pacer.OnArrival(now);
            now += gapMilliseconds;
        }

        return now;
    }

    private static long FeedBursts(TickPacer pacer, long now, int bursts, int ticksPerBurst, int gapMilliseconds)
    {
        for (var burstIndex = 0; burstIndex < bursts; burstIndex++)
        {
            for (var tickIndex = 0; tickIndex < ticksPerBurst; tickIndex++)
            {
                pacer.OnArrival(now);
            }

            now += gapMilliseconds;
        }

        return now;
    }
}
