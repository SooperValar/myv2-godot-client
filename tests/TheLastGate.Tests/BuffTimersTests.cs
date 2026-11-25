using TheLastGate.Net;
using TheLastGate.State;

namespace TheLastGate.Tests;

public class BuffTimersTests
{
    private const int TicksPerSecond = 18;
    private const int Slot = 3;

    [Fact]
    public void SpellTime_SetsTheSlotsTimer()
    {
        var state = new GameState();

        new TickProcessor(state).Process(TestCommand.Payload(SpellTime(Slot, 90, true, "Curse")));

        Assert.Equal(new BuffTimer(90, true, "Curse"), state.BuffTimers.Timers[Slot]);
        Assert.Equal(90, state.BuffTimers.SecondsLeft(Slot));
    }

    [Fact]
    public void SecondsLeft_CountsDownOneSecondEveryEighteenTicks()
    {
        var state = new GameState();
        var processor = new TickProcessor(state);
        processor.Process(TestCommand.Payload(SpellTime(Slot, 90, false, "Bless")));

        RunTicks(processor, TicksPerSecond * 3 - 1);

        Assert.Equal(87, state.BuffTimers.SecondsLeft(Slot));
    }

    [Fact]
    public void SecondsLeft_NeverGoesBelowZero()
    {
        var state = new GameState();
        var processor = new TickProcessor(state);
        processor.Process(TestCommand.Payload(SpellTime(Slot, 1, false, "Bless")));

        RunTicks(processor, TicksPerSecond * 5);

        Assert.Equal(0, state.BuffTimers.SecondsLeft(Slot));
    }

    [Fact]
    public void SpellTime_RestartsTheCountdownWhenResent()
    {
        var state = new GameState();
        var processor = new TickProcessor(state);
        processor.Process(TestCommand.Payload(SpellTime(Slot, 90, false, "Bless")));
        RunTicks(processor, TicksPerSecond * 10);

        processor.Process(TestCommand.Payload(SpellTime(Slot, 300, false, "Bless")));

        Assert.Equal(300, state.BuffTimers.SecondsLeft(Slot));
    }

    private static TestCommand SpellTime(byte slot, ushort seconds, bool harmful, string name) =>
        TestCommand.Of(ServerCommand.SpellTime).Byte(slot).UInt16(seconds).Byte(harmful ? (byte)1 : (byte)0)
            .Text(name, 15);

    private static void RunTicks(TickProcessor processor, int ticks)
    {
        for (var tickNumber = 0; tickNumber < ticks; tickNumber++)
        {
            processor.Process([]);
        }
    }
}
