using System;

namespace TheLastGate.State;

public sealed class BuffTimers
{
    public const int Slots = 20;

    // The server only resends a timer when its buff changes, so the client counts the seconds down itself.
    private const int ServerTicksPerSecond = 18;

    private readonly int[] receivedAtTick = new int[Slots];
    private int tick;

    public BuffTimer[] Timers { get; } = new BuffTimer[Slots];

    public int SecondsLeft(int slot) =>
        Math.Max(0, Timers[slot].SecondsLeft - (tick - receivedAtTick[slot]) / ServerTicksPerSecond);

    internal void Set(int slot, BuffTimer timer)
    {
        Timers[slot] = timer;
        receivedAtTick[slot] = tick;
    }

    internal void EndOfTick() => tick++;
}

public readonly record struct BuffTimer(int SecondsLeft, bool IsHarmful, string Name);
