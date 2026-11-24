namespace TheLastGate.State;

public sealed class BuffTimers
{
    public const int Slots = 20;

    public BuffTimer[] Timers { get; } = new BuffTimer[Slots];
}

public readonly record struct BuffTimer(int SecondsLeft, bool IsHarmful, string Name);
